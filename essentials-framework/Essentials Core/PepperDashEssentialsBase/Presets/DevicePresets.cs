using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Core;

//using SSMono.IO;

namespace PepperDash.Essentials.Core.Presets
{
	/// <summary>
	/// Class that represents the model behind presets display
	/// </summary>
	public class DevicePresetsModel : Device
	{
		public event EventHandler PresetsLoaded;

		public int PulseTime { get; set; }
		public int DigitSpacingMS { get; set; }
		public bool PresetsAreLoaded { get; private set; }

		public List<PresetChannel> PresetsList { get { return _PresetsList.ToList(); } }
		List<PresetChannel> _PresetsList;
		public int Count { get { return PresetsList != null ? PresetsList.Count : 0; } }

		public bool UseLocalImageStorage { get; set; }
		public string ImagesLocalHostPrefix { get; set; }
		public string ImagesPathPrefix { get; set; }
		public string ListPathPrefix { get; set; }

		/// <summary>
		/// The methods on the STB device to call when dialing
		/// </summary>
		Dictionary<char, Action<bool>> DialFunctions;
		Action<bool> EnterFunction;

		bool DialIsRunning;
		string FilePath;
		bool InitSuccess;
        //SSMono.IO.FileSystemWatcher ListWatcher;

		public DevicePresetsModel(string key, ISetTopBoxNumericKeypad setTopBox, string fileName)
			: base(key)
		{
			PulseTime = 150;
			DigitSpacingMS = 150;

			try
			{
				// Grab the digit functions from the device
				// If any fail, the whole thing fails peacefully
				DialFunctions = new Dictionary<char, Action<bool>>(10)
				{
					{ '1', setTopBox.Digit1 },
					{ '2', setTopBox.Digit2 },
					{ '3', setTopBox.Digit3 },
					{ '4', setTopBox.Digit4 },
					{ '5', setTopBox.Digit5 },
					{ '6', setTopBox.Digit6 },
					{ '7', setTopBox.Digit7 },
					{ '8', setTopBox.Digit8 },
					{ '9', setTopBox.Digit9 },
					{ '0', setTopBox.Digit0 },
					{ '-', setTopBox.Dash }
				};
			}
			catch
			{
				Debug.Console(0, "DevicePresets '{0}', not attached to INumericKeypad device. Ignoring", key);
				DialFunctions = null;
				return;
			}

			EnterFunction = setTopBox.KeypadEnter;

			UseLocalImageStorage = true;

			ImagesLocalHostPrefix = "http://" + CrestronEthernetHelper.GetEthernetParameter(
				CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS,0);
			ImagesPathPrefix = @"/presets/images.zip/";
			ListPathPrefix = @"/html/presets/lists/";

			SetFileName(fileName);

            //ListWatcher = new FileSystemWatcher(@"\HTML\presets\lists");
            //ListWatcher.NotifyFilter = NotifyFilters.LastWrite;
            //ListWatcher.EnableRaisingEvents = true;
            //ListWatcher.Changed += ListWatcher_Changed;
			InitSuccess = true;
		}


		public void SetFileName(string path)
		{
			FilePath = ListPathPrefix + path;
			LoadChannels();
		}	

		public void LoadChannels()
		{
			PresetsAreLoaded = false;
			try
			{
				var pl = JsonConvert.DeserializeObject<PresetsList>(Crestron.SimplSharp.CrestronIO.File.ReadToEnd(FilePath, Encoding.ASCII));
				Name = pl.Name;
				_PresetsList = pl.Channels;
			}
			catch (Exception e)
			{
				Debug.Console(2, this, "LoadChannels: Error reading presets file. These presets will be empty:\r  '{0}'\r  Error:{1}", FilePath, e.Message);
				// Just save a default empty list
				_PresetsList = new List<PresetChannel>();
			}
			PresetsAreLoaded = true;

			var handler = PresetsLoaded;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		public void Dial(int presetNum)
		{
			if (presetNum <= _PresetsList.Count)
				Dial(_PresetsList[presetNum - 1].Channel);
		}

		public void Dial(string chanNum)
		{
			if (DialIsRunning || !InitSuccess) return;
			if (DialFunctions == null)
			{
				Debug.Console(1, "DevicePresets '{0}', not attached to keypad device. Ignoring channel", Key);
				return;
			}

			DialIsRunning = true;
			CrestronInvoke.BeginInvoke(o =>
				{
					foreach (var c in chanNum.ToCharArray())
					{
						if (DialFunctions.ContainsKey(c))
							Pulse(DialFunctions[c]);
						CrestronEnvironment.Sleep(DigitSpacingMS);
					}

					if (EnterFunction != null)
						Pulse(EnterFunction);
					DialIsRunning = false;
				});
		}

		void Pulse(Action<bool> act)
		{
			act(true);
			CrestronEnvironment.Sleep(PulseTime);
			act(false);
		}

		/// <summary>
		/// Event handler for filesystem watcher.  When directory changes, this is called
		/// </summary>
        //void ListWatcher_Changed(object sender, FileSystemEventArgs e)
        //{
        //    Debug.Console(1, this, "folder modified: {0}", e.FullPath);
        //    if (e.FullPath.Equals(FilePath, StringComparison.OrdinalIgnoreCase))
        //    {
        //        Debug.Console(1, this, "File changed: {0}", e.ChangeType);			
        //        LoadChannels();
        //    }
        //}
	}
}