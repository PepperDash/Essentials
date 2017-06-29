using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Crestron.SimplSharpPro.EthernetCommunication;
using Crestron.SimplSharpPro.UI;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices;
//using PepperDash.Essentials.Core.Devices.Dm;
//using PepperDash.Essentials.Fusion;
using PepperDash.Core;

namespace PepperDash.Essentials
{
	public static class Configuration
	{

		public static string LastPath { get; private set; }
		public static CrestronControlSystem ControlSystem { get; private set; }
		
		public static void Initialize(CrestronControlSystem cs)
		{
			CrestronConsole.AddNewConsoleCommand(ReloadFromConsole, "configreload", "Reloads configuration file", 
				ConsoleAccessLevelEnum.AccessOperator);
			ControlSystem = cs;
		}

		public static bool ReadConfiguration(string filePath)
		{
			try
			{
				// Read file
				if (File.Exists(filePath))
				{
					LastPath = filePath;
					string json = File.ReadToEnd(filePath, System.Text.Encoding.ASCII);
					JObject jo = JObject.Parse(json);

					var info = JsonConvert.DeserializeObject<ConfigInfo>(jo["info"].ToString());
					Debug.Console(0, "\r[Config] file read:");
					Debug.Console(0, "    File: {0}", filePath);
					Debug.Console(0, "    Name: {0}", info.Name);
					Debug.Console(0, "    Type: {0}", info.SystemTemplateType);
					Debug.Console(0, "    Date: {0}", info.EditDate);
					Debug.Console(0, "    ConfigVersion: {0}", info.Version);
					Debug.Console(0, "    EditedBy: {0}", info.EditedBy);
					Debug.Console(0, "    Comment: {0}\r", info.Comment);

					// Get the main config object
					var jConfig = jo["configuration"];

					// Devices
					var jDevices = (JArray)jConfig["devices"];
					CreateDevices(jDevices);

					// TieLines
					var jRouting = jConfig["routing"];
					CreateRouting(jRouting);	
				
					/// Parse the available source list(s)
					var jSourceLists = (JArray)jConfig["sourceLists"];
					var jSourceListJson = jSourceLists.ToString();
					List<ConfigSourceList> sourceLists = JsonConvert.DeserializeObject<List<ConfigSourceList>>(jSourceListJson);

					// System
					var jSystems = (JArray)jConfig["systems"];
					CreateSystems(jSystems, sourceLists);

					// Activate everything
					DeviceManager.ActivateAll();
				}
				else
				{
					Debug.Console(0, "[Config] file not found '{0}'", filePath);
					return false;
				}
			}
			catch (Exception e)
			{
				Debug.Console(0, "Configuration read error: \r {0}", e);
				return false;
			}

			return true;
		}

		static void CreateDevices(JArray jDevices)
		{
			//Debug.Console(0, "  Creating {0} devices", jDevices.Count);
			//for (int i = 0; i < jDevices.Count; i++)
			//{
			//    var jDev = jDevices[i];

			//    //var devConfig = JsonConvert.DeserializeObject<DeviceConfig>(jDev.ToString());
			//    //Debug.Console(0, "++++++++++++{0}", devConfig);


			//    var group = jDev["group"].Value<string>();
			//    Debug.Console(0, "  [{0}], creating {1}:{2}", jDev["key"].Value<string>(),
			//        group, jDev["type"].Value<string>());

			//    Device dev = null;
			//    if (group.Equals("Display", StringComparison.OrdinalIgnoreCase))
			//        dev = DisplayFactory.CreateDisplay(jDev);
			//    else if (group.Equals("DeviceMonitor", StringComparison.OrdinalIgnoreCase))
			//        dev = DeviceManagerFactory.Create(jDev);
			//    //else if (group.Equals("Pc", StringComparison.OrdinalIgnoreCase))
			//    //    dev = PcFactory.Create(jDev);
			//    //else if (group.Equals("SetTopBox", StringComparison.OrdinalIgnoreCase))
			//    //    dev = SetTopBoxFactory.Create(jDev);
			//    //else if (group.Equals("DiscPlayer", StringComparison.OrdinalIgnoreCase))
			//    //    dev = DiscPlayerFactory.Create(jDev);
			//    //else if (group.Equals("Touchpanel", StringComparison.OrdinalIgnoreCase))
			//    //    dev = TouchpanelFactory.Create(jDev);
			//    else if (group.Equals("dmEndpoint", StringComparison.OrdinalIgnoreCase)) // Add Transmitter and Receiver
			//        dev = DmFactory.Create(jDev);
			//    else if (group.Equals("dmChassic", StringComparison.OrdinalIgnoreCase))
			//        dev = DmFactory.CreateChassis(jDev);
			//    else if (group.Equals("processor", StringComparison.OrdinalIgnoreCase))
			//        continue; // ignore it.  Has no value right now.
			//    //else if (group.Equals("remote", StringComparison.OrdinalIgnoreCase))
			//    //    dev = RemoteFactory.Create(jDev);
			//    else
			//    {
			//        Debug.Console(0, "      ERROR: Device [{0}] has unknown Group '{1}'. Skipping",
			//            jDev["key"].Value<string>(), group);
			//        continue;
			//    }

			//    if (dev != null)
			//        DeviceManager.AddDevice(dev);
			//    else
			//        Debug.Console(0, "      ERROR: failed to create device {0}",
			//            jDev["key"].Value<string>());
			//}
		}

		static void CreateSystems(JArray jSystems, List<ConfigSourceList> sourceLists)
		{
//            // assuming one system
//            var jSystem = jSystems[0];
//            var name = jSystem.Value<string>("name");
//            var key = FactoryHelper.KeyOrConvertName(jSystem.Value<string>("key"), name);

//            if (jSystem.Value<string>("type").Equals("EssentialsHuddleSpace", StringComparison.OrdinalIgnoreCase))
//            {
//                var sys = new HuddleSpaceRoom(key, name);
//                var props = jSystem["properties"];
//                var displayKey = props["displayKey"].Value<string>();
//                if (displayKey != null)
//                    sys.DefaultDisplay = (DisplayBase)DeviceManager.GetDeviceForKey(displayKey);

//                // Add sources from passed in config list
//                var myList = sourceLists.FirstOrDefault(
//                    l => l.Key.Equals(props.Value<string>("sourceListKey"), StringComparison.OrdinalIgnoreCase));
//                if (myList != null)
//                    AddSourcesToSystem(sys, myList);

//                DeviceManager.AddDevice(sys);

//                //spin up a fusion thing too
//#warning add this fusion connector back in later
//                //DeviceManager.AddDevice(new EssentialsHuddleSpaceFusionSystemController(sys, 0xf1));
			//}
		}

		//static void AddSourcesToSystem(Room system, ConfigSourceList configList)
		//{
			//foreach (var configItem in configList.PresentationSources)
			//{
			//    var src = (IPresentationSource)DeviceManager.GetDeviceForKey(configItem.SourceKey);
			//    if (src != null)
			//        system.Sources.Add(configItem.Number, src);
			//    else
			//        Debug.Console(0, system, "cannot find source '{0}' from list {1}", 
			//            configItem.SourceKey, configList.Name);
			//}
		//}

		/// <summary>
		/// Links up routing, creates tie lines
		/// </summary>
		/// <param name="jRouting">The "Routing" JArray from configuration</param>
		static void CreateRouting(JToken jRouting)
		{
			var jsonTieLines = jRouting["tieLines"].ToString();
			var tieLineConfigs = JsonConvert.DeserializeObject<List<ConfigTieLine>>(jsonTieLines);
			foreach (var c in tieLineConfigs)
			{
				var tl = c.GetTieLine();
				if (tl != null)
					TieLineCollection.Default.Add(tl);
			}
		}


		/// <summary>
		/// Returns the IIROutputPorts device (control system, etc) that contains a given IR port
		/// </summary>
		/// <param name="propsToken"></param>
		static IROutputPort GetIrPort(JToken propsToken)
		{
			var portDevName = propsToken.Value<string>("IrPortDevice");
			var portNum = propsToken.Value<uint>("IrPortNumber");
			if (portDevName.Equals("controlSystem", StringComparison.OrdinalIgnoreCase))
			{
				IIROutputPorts irDev = ControlSystem;
				if (portNum <= irDev.NumberOfIROutputPorts)
					return ControlSystem.IROutputPorts[portNum];
				else
					Debug.Console(0, "[Config] ERROR: IR Port {0} out of range. Range 0-{1} on {2}", portNum,
						ControlSystem.IROutputPorts.Count, irDev.ToString());
			}
			return null;
		}

		static void HandleUnknownType(JToken devToken, string type)
		{
			Debug.Console(0, "[Config] ERROR: Type '{0}' not found in group '{1}'", type, 
				devToken.Value<string>("Group"));
		}

		static void HandleDeviceCreationError(JToken devToken, Exception e)
		{
			Debug.Console(0, "[Config] ERROR creating device [{0}]: \r{1}", 
				devToken["Key"].Value<string>(), e);
		}

		/// <summary>
		/// Console helper to reload
		/// </summary>
		static void ReloadFromConsole(string s)
		{
			// Gotta tear down everything first!

			if (!string.IsNullOrEmpty(LastPath))
			{
				ReadConfiguration(LastPath);
			}
		}
	}

	public class ConfigSourceList
	{
		[JsonProperty(Required = Required.Always)]
		public string Key { get; set; }
		
		[JsonProperty(Required = Required.Always)]
		public string Name { get; set; }
		
		[JsonProperty(Required = Required.Always)]
		public List<ConfigSourceItem> PresentationSources { get; set; }
		
	}

	public class ConfigSourceItem
	{
		[JsonProperty(Required = Required.Always)]
		public uint Number { get; set; }

		[JsonProperty(Required = Required.Always)]
		public string SourceKey { get; set; }
		
		public string AlternateName { get; set; }
	}

	public class ConfigInfo
	{
		public string SystemTemplateType { get; set; }
		public string ProcessorType { get; set; }
		public string Name { get; set; }
		public uint ProgramSlotNumber { get; set; }
		public string Version { get; set; }
		public string EditedBy { get; set; }
		public string EditDate { get; set; }
		public string Comment { get; set; }
	}
}