 

using System;
using System.Collections.Generic;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using PepperDash.Core;

//using SSMono.IO;
using PepperDash.Core.WebApi.Presets;

namespace PepperDash.Essentials.Core.Presets
{
    /// <summary>
    /// Class that represents the model behind presets display
    /// </summary>
    public class DevicePresetsModel : Device
    {
        public delegate void PresetRecalledCallback(ISetTopBoxNumericKeypad device, string channel);

        public delegate void PresetsSavedCallback(List<PresetChannel> presets);

        private readonly CCriticalSection _fileOps = new CCriticalSection();
        private readonly bool _initSuccess;

        private readonly ISetTopBoxNumericKeypad _setTopBox;

        /// <summary>
        /// The methods on the STB device to call when dialing
        /// </summary>
        private Dictionary<char, Action<bool>> _dialFunctions;

        private bool _dialIsRunning;
        private Action<bool> _enterFunction;
        private string _filePath;

        public DevicePresetsModel(string key, ISetTopBoxNumericKeypad setTopBox, string fileName)
            : this(key, fileName)
        {
            try
            {
                _setTopBox = setTopBox;

                // Grab the digit functions from the device
                // If any fail, the whole thing fails peacefully
                _dialFunctions = new Dictionary<char, Action<bool>>(10)
                {
                    {'1', setTopBox.Digit1},
                    {'2', setTopBox.Digit2},
                    {'3', setTopBox.Digit3},
                    {'4', setTopBox.Digit4},
                    {'5', setTopBox.Digit5},
                    {'6', setTopBox.Digit6},
                    {'7', setTopBox.Digit7},
                    {'8', setTopBox.Digit8},
                    {'9', setTopBox.Digit9},
                    {'0', setTopBox.Digit0},
                    {'-', setTopBox.Dash}
                };
            }
            catch
            {
                Debug.Console(0, "DevicePresets '{0}', not attached to INumericKeypad device. Ignoring", key);
                _dialFunctions = null;
                return;
            }

            _enterFunction = setTopBox.KeypadEnter;
        }

        public DevicePresetsModel(string key, string fileName) : base(key)
        {
            PulseTime = 150;
            DigitSpacingMs = 150;

            UseLocalImageStorage = true;

            ImagesLocalHostPrefix = "http://" + CrestronEthernetHelper.GetEthernetParameter(
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0);
            ImagesPathPrefix = @"/presets/images.zip/";
            ListPathPrefix = @"/html/presets/lists/";

            SetFileName(fileName);

            _initSuccess = true;
        }

        public event PresetRecalledCallback PresetRecalled;
        public event PresetsSavedCallback PresetsSaved;

        public int PulseTime { get; set; }
        public int DigitSpacingMs { get; set; }
        public bool PresetsAreLoaded { get; private set; }

        public List<PresetChannel> PresetsList { get; private set; }

        public int Count
        {
            get { return PresetsList != null ? PresetsList.Count : 0; }
        }

        public bool UseLocalImageStorage { get; set; }
        public string ImagesLocalHostPrefix { get; set; }
        public string ImagesPathPrefix { get; set; }
        public string ListPathPrefix { get; set; }
        public event EventHandler PresetsLoaded;


        public void SetFileName(string path)
        {
            _filePath = ListPathPrefix + path;

            Debug.Console(2, this, "Setting presets file path to {0}", _filePath);
            LoadChannels();
        }

        public void LoadChannels()
        {
            try
            {
                _fileOps.Enter();

                Debug.Console(2, this, "Loading presets from {0}", _filePath);
                PresetsAreLoaded = false;
                try
                {
                    var pl = JsonConvert.DeserializeObject<PresetsList>(File.ReadToEnd(_filePath, Encoding.ASCII));
                    Name = pl.Name;
                    PresetsList = pl.Channels;
                }
                catch (Exception e)
                {
                    Debug.Console(2, this,
                        "LoadChannels: Error reading presets file. These presets will be empty:\r  '{0}'\r  Error:{1}",
                        _filePath, e.Message);
                    // Just save a default empty list
                    PresetsList = new List<PresetChannel>();
                }
                PresetsAreLoaded = true;

                var handler = PresetsLoaded;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
            finally
            {
                _fileOps.Leave();
            }
        }

        public void Dial(int presetNum)
        {
            if (presetNum <= PresetsList.Count)
            {
                Dial(PresetsList[presetNum - 1].Channel);
            }
        }

        public void Dial(string chanNum)
        {
            if (_dialIsRunning || !_initSuccess)
            {
                return;
            }
            if (_dialFunctions == null)
            {
                Debug.Console(1, "DevicePresets '{0}', not attached to keypad device. Ignoring channel", Key);
                return;
            }

            _dialIsRunning = true;
            CrestronInvoke.BeginInvoke(o =>
            {
                foreach (var c in chanNum.ToCharArray())
                {
                    if (_dialFunctions.ContainsKey(c))
                    {
                        Pulse(_dialFunctions[c]);
                    }
                    CrestronEnvironment.Sleep(DigitSpacingMs);
                }

                if (_enterFunction != null)
                {
                    Pulse(_enterFunction);
                }
                _dialIsRunning = false;
            });

            if (_setTopBox == null) return;

            OnPresetRecalled(_setTopBox, chanNum);
        }

        public void Dial(int presetNum, ISetTopBoxNumericKeypad setTopBox)
        {
            if (presetNum <= PresetsList.Count)
            {
                Dial(PresetsList[presetNum - 1].Channel, setTopBox);
            }
        }

        public void Dial(string chanNum, ISetTopBoxNumericKeypad setTopBox)
        {
            _dialFunctions = new Dictionary<char, Action<bool>>(10)
            {
                {'1', setTopBox.Digit1},
                {'2', setTopBox.Digit2},
                {'3', setTopBox.Digit3},
                {'4', setTopBox.Digit4},
                {'5', setTopBox.Digit5},
                {'6', setTopBox.Digit6},
                {'7', setTopBox.Digit7},
                {'8', setTopBox.Digit8},
                {'9', setTopBox.Digit9},
                {'0', setTopBox.Digit0},
                {'-', setTopBox.Dash}
            };

            _enterFunction = setTopBox.KeypadEnter;

            OnPresetRecalled(setTopBox, chanNum);

            Dial(chanNum);
        }

        private void OnPresetRecalled(ISetTopBoxNumericKeypad setTopBox, string channel)
        {
            var handler = PresetRecalled;

            if (handler == null)
            {
                return;
            }

            handler(setTopBox, channel);
        }

        public void UpdatePreset(int index, PresetChannel preset)
        {
            if (index >= PresetsList.Count)
            {
                return;
            }

            PresetsList[index] = preset;

            SavePresets();

            OnPresetsSaved();
        }

        public void UpdatePresets(List<PresetChannel> presets)
        {
            PresetsList = presets;

            SavePresets();

            OnPresetsSaved();
        }

        private void SavePresets()
        {
            try
            {
                _fileOps.Enter();
                var pl = new PresetsList {Channels = PresetsList, Name = Name};
                var json = JsonConvert.SerializeObject(pl, Formatting.Indented);

                using (var file = File.Open(_filePath, FileMode.Truncate))
                {
                    file.Write(json, Encoding.UTF8);
                }
            }
            finally
            {
                _fileOps.Leave();
            }
            
        }

        private void OnPresetsSaved()
        {
            var handler = PresetsSaved;

            if (handler == null) return;

            handler(PresetsList);
        }

        private void Pulse(Action<bool> act)
        {
            act(true);
            CrestronEnvironment.Sleep(PulseTime);
            act(false);
        }
    }
}