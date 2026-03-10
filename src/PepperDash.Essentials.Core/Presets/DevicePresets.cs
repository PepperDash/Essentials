

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using PepperDash.Core;

using Serilog.Events;

namespace PepperDash.Essentials.Core.Presets;

/// <summary>
/// Class that represents the model behind presets display
/// </summary>
public class DevicePresetsModel : Device
{
    /// <summary>
    /// Delegate for PresetRecalled event, which is fired when a preset is recalled. Provides the device and channel that was recalled.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="channel"></param>
    public delegate void PresetRecalledCallback(ISetTopBoxNumericKeypad device, string channel);

    /// <summary>
    /// Delegate for PresetsSaved event, which is fired when presets are saved. Provides the list of presets that were saved.
    /// </summary> <param name="presets"></param>
    public delegate void PresetsSavedCallback(List<PresetChannel> presets);

    private readonly object _fileOps = new();
    private readonly bool _initSuccess;

    private readonly ISetTopBoxNumericKeypad _setTopBox;

    /// <summary>
    /// The methods on the STB device to call when dialing
    /// </summary>
    private Dictionary<char, Action<bool>> _dialFunctions;

    private bool _dialIsRunning;
    private Action<bool> _enterFunction;
    private string _filePath;

    /// <summary>
    /// Constructor for DevicePresetsModel when a set top box device is included.  If the set top box does not implement the required INumericKeypad interface, the model will still be created but dialing functionality will be disabled and a message will be logged.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="setTopBox"></param>
    /// <param name="fileName"></param>
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
            Debug.LogMessage(LogEventLevel.Information, "DevicePresets '{0}', not attached to INumericKeypad device. Ignoring", key);
            _dialFunctions = null;
            return;
        }

        _enterFunction = setTopBox.KeypadEnter;
    }

    /// <summary>
    /// Constructor for DevicePresetsModel when only a file name is provided. Dialing functionality will be disabled.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="fileName"></param>
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

    /// <summary>
    /// Event fired when a preset is recalled, providing the device and channel that was recalled
    /// </summary>
    public event PresetRecalledCallback PresetRecalled;

    /// <summary>
    /// Event fired when presets are saved, providing the list of presets that were saved
    /// </summary>
    public event PresetsSavedCallback PresetsSaved;

    /// <summary>
    /// Time in milliseconds to pulse the digit for when dialing a channel
    /// </summary>
    public int PulseTime { get; private set; }

    /// <summary> 
    /// Time in milliseconds to wait between pulsing digits when dialing a channel
    /// </summary>
    public int DigitSpacingMs { get; private set; }

    /// <summary>
    /// Whether the presets have finished loading from the file or not
    /// </summary>
    public bool PresetsAreLoaded { get; private set; }

    /// <summary>
    /// The list of presets to display
    /// </summary>
    public List<PresetChannel> PresetsList { get; private set; }

    /// <summary>
    /// The number of presets in the list
    /// </summary>
    public int Count
    {
        get { return PresetsList != null ? PresetsList.Count : 0; }
    }

    /// <summary>
    /// Indicates whether to use local image storage for preset images, which allows for more and larger images than the SIMPL+ zip file method
    /// </summary>
    public bool UseLocalImageStorage { get; private set; }

    /// <summary>
    /// The prefix for the local host URL for preset images
    /// </summary>
    public string ImagesLocalHostPrefix { get; private set; }

    /// <summary>
    /// The path prefix for preset images
    /// </summary>
    public string ImagesPathPrefix { get; private set; }

    /// <summary>
    /// The path prefix for preset lists
    /// </summary>
    public string ListPathPrefix { get; private set; }

    /// <summary>
    /// Event fired when presets are loaded
    /// </summary>
    public event EventHandler PresetsLoaded;

    /// <summary>
    /// Sets the file name for the presets list and loads the presets from that file. The file should be a JSON file in the format of the PresetsList class. If the file cannot be read, an empty list will be created and a message will be logged. This method is thread safe.
    /// </summary>
    /// <param name="path">The path to the presets file.</param>
    public void SetFileName(string path)
    {
        _filePath = ListPathPrefix + path;

        Debug.LogMessage(LogEventLevel.Verbose, this, "Setting presets file path to {0}", _filePath);
        LoadChannels();
    }

    /// <summary>
    /// Loads the presets from the file specified by _filePath. 
    /// </summary>
    public void LoadChannels()
    {
        lock (_fileOps)
        {
            Debug.LogMessage(LogEventLevel.Verbose, this, "Loading presets from {0}", _filePath);
            PresetsAreLoaded = false;
            try
            {
                var pl = JsonConvert.DeserializeObject<PresetsList>(File.ReadToEnd(_filePath, Encoding.ASCII));
                Name = pl.Name;
                PresetsList = pl.Channels;

                Debug.LogMessage(LogEventLevel.Verbose, this, "Loaded {0} presets", PresetsList.Count);
            }
            catch (Exception e)
            {
                Debug.LogMessage(LogEventLevel.Verbose, this,
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
    }

    /// <summary>
    /// Dials a preset by its number in the list (starting at 1). If the preset number is out of range, nothing will happen.
    /// </summary> 
    /// <param name="presetNum">The number of the preset to dial, starting at 1</param>
    public void Dial(int presetNum)
    {
        if (presetNum <= PresetsList.Count)
        {
            Dial(PresetsList[presetNum - 1].Channel);
        }
    }

    /// <summary>
    /// Dials a preset by its channel number. If the channel number contains characters that are not 0-9 or '-', those characters will be ignored. 
    /// If the model was not initialized with a valid set top box device, dialing will be disabled and a message will be logged.
    /// </summary> <param name="chanNum">The channel number to dial</param>
    public void Dial(string chanNum)
    {
        if (_dialIsRunning || !_initSuccess)
        {
            return;
        }
        if (_dialFunctions == null)
        {
            Debug.LogMessage(LogEventLevel.Debug, "DevicePresets '{0}', not attached to keypad device. Ignoring channel", Key);
            return;
        }

        _dialIsRunning = true;
        Task.Run(() =>
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

    /// <summary>
    /// Dials a preset by its number in the list (starting at 1) using the provided set top box device. If the preset number is out of range, nothing will happen.
    /// </summary>
    /// <param name="presetNum"></param>
    /// <param name="setTopBox"></param>
    public void Dial(int presetNum, ISetTopBoxNumericKeypad setTopBox)
    {
        if (presetNum <= PresetsList.Count)
        {
            Dial(PresetsList[presetNum - 1].Channel, setTopBox);
        }
    }

    /// <summary>
    /// Dials a preset by its channel number using the provided set top box device. If the channel number contains characters that are not 0-9 or '-', those characters will be ignored.
    /// If the provided set top box device does not implement the required INumericKeypad interface, dialing will be disabled and a message will be logged.
    /// If the model was not initialized with a valid set top box device, dialing will be disabled and a message will be logged.
    /// </summary> 
    /// <param name="chanNum"></param>
    /// <param name="setTopBox"></param>
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

    /// <summary>
    /// Updates the preset at the given index with the provided preset information, then saves the updated presets list to the file. If the index is out of range, nothing will happen.
    /// </summary> 
    /// <param name="index">The index of the preset to update, starting at 0</param>
    /// <param name="preset">The preset information to update</param>
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

    /// <summary>
    /// Updates the entire presets list with the provided list, then saves the updated presets list to the file. If the provided list is null, nothing will happen.
    /// </summary>
    /// <param name="presets"></param>
    public void UpdatePresets(List<PresetChannel> presets)
    {
        PresetsList = presets;

        SavePresets();

        OnPresetsSaved();
    }

    private void SavePresets()
    {
        lock (_fileOps)
        {
            var pl = new PresetsList { Channels = PresetsList, Name = Name };
            var json = JsonConvert.SerializeObject(pl, Formatting.Indented);

            using (var file = File.Open(_filePath, FileMode.Truncate))
            {
                file.Write(json, Encoding.UTF8);
            }
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