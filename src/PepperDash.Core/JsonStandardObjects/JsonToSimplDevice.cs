using System;
using System.Linq;
using Crestron.SimplSharp;
using PepperDash.Core.JsonToSimpl;
using Serilog.Events;

namespace PepperDash.Core.JsonStandardObjects
{
	/// <summary>
	/// Device class
	/// </summary>
	public class DeviceConfig
	{		
		/// <summary>
		/// JSON config key property
		/// </summary>
		public string key { get; set; }
		/// <summary>
		/// JSON config name property
		/// </summary>
		public string name { get; set; }
		/// <summary>
		/// JSON config type property
		/// </summary>
		public string type { get; set; }
		/// <summary>
		/// JSON config properties 
		/// </summary>
		public PropertiesConfig properties { get; set; }

		/// <summary>
		/// Bool change event handler
		/// </summary>
		public event EventHandler<BoolChangeEventArgs> BoolChange;
		/// <summary>
		/// Ushort change event handler
		/// </summary>
		public event EventHandler<UshrtChangeEventArgs> UshrtChange;
		/// <summary>
		/// String change event handler
		/// </summary>
		public event EventHandler<StringChangeEventArgs> StringChange;
		/// <summary>
		/// Object change event handler
		/// </summary>
		public event EventHandler<DeviceChangeEventArgs> DeviceChange;

		/// <summary>
		/// Constructor
		/// </summary>
		public DeviceConfig()
		{
			properties = new PropertiesConfig();
		}

		/// <summary>
		/// Initialize method
		/// </summary>
		/// <param name="uniqueID"></param>
		/// <param name="deviceKey"></param>
		public void Initialize(string uniqueID, string deviceKey)
		{
			// S+ set EvaluateFb low
			OnBoolChange(false, 0, JsonStandardDeviceConstants.JsonObjectEvaluated);
			// validate parameters
			if (string.IsNullOrEmpty(uniqueID) || string.IsNullOrEmpty(deviceKey))
			{
				Debug.LogMessage(LogEventLevel.Debug, "UniqueID ({0} or key ({1} is null or empty", uniqueID, deviceKey);
				// S+ set EvaluteFb high
				OnBoolChange(true, 0, JsonStandardDeviceConstants.JsonObjectEvaluated);
				return;
			}

			key = deviceKey;

			try
			{
				// get the file using the unique ID
				JsonToSimplMaster jsonMaster = J2SGlobal.GetMasterByFile(uniqueID);
				if (jsonMaster == null)
				{
					Debug.LogMessage(LogEventLevel.Debug, "Could not find JSON file with uniqueID {0}", uniqueID);
					return;
				}

				// get the device configuration using the key
				var devices = jsonMaster.JsonObject.ToObject<RootObject>().devices;
				var device = devices.FirstOrDefault(d => d.key.Equals(key));
				if (device == null)
				{
					Debug.LogMessage(LogEventLevel.Debug, "Could not find device with key {0}", key);
					return;
				}
				OnObjectChange(device, 0, JsonStandardDeviceConstants.JsonObjectChanged);

				var index = devices.IndexOf(device);
				OnStringChange(string.Format("devices[{0}]", index), 0, JsonToSimplConstants.FullPathToArrayChange);
			}
			catch (Exception e)
			{
				var msg = string.Format("Device {0} lookup failed:\r{1}", key, e);
				CrestronConsole.PrintLine(msg);
				ErrorLog.Error(msg);
			}
			finally
			{
				// S+ set EvaluteFb high
				OnBoolChange(true, 0, JsonStandardDeviceConstants.JsonObjectEvaluated);
			}
		}

		#region EventHandler Helpers

		/// <summary>
		/// BoolChange event handler helper
		/// </summary>
		/// <param name="state"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnBoolChange(bool state, ushort index, ushort type)
		{
			var handler = BoolChange;
			if (handler != null)
			{
				var args = new BoolChangeEventArgs(state, type);
				args.Index = index;
				BoolChange(this, args);
			}
		}

		/// <summary>
		/// UshrtChange event handler helper
		/// </summary>
		/// <param name="state"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnUshrtChange(ushort state, ushort index, ushort type)
		{
			var handler = UshrtChange;
			if (handler != null)
			{
				var args = new UshrtChangeEventArgs(state, type);
				args.Index = index;
				UshrtChange(this, args);
			}
		}

		/// <summary>
		/// StringChange event handler helper
		/// </summary>
		/// <param name="value"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnStringChange(string value, ushort index, ushort type)
		{
			var handler = StringChange;
			if (handler != null)
			{
				var args = new StringChangeEventArgs(value, type);
				args.Index = index;
				StringChange(this, args);
			}
		}

		/// <summary>
		/// ObjectChange event handler helper
		/// </summary>
		/// <param name="device"></param>
		/// <param name="index"></param>
		/// <param name="type"></param>
		protected void OnObjectChange(DeviceConfig device, ushort index, ushort type)
		{
			if (DeviceChange != null)
			{
				var args = new DeviceChangeEventArgs(device, type);
				args.Index = index;
				DeviceChange(this, args);
			}
		}

		#endregion EventHandler Helpers
	}
}