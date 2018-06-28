using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials
{
	public class DeviceFactory
	{
		public static IKeyed GetDevice(DeviceConfig dc)
		{
			var key = dc.Key;
			var name = dc.Name;
			var type = dc.Type;
			var properties = dc.Properties;

			var typeName = dc.Type.ToLower();

            if (typeName == "amplifier")
            {
                return new Amplifier(dc.Key, dc.Name);
            } 
            else if (dc.Group.ToLower() == "touchpanel") //  typeName.StartsWith("tsw"))
            {
                return UiDeviceFactory.GetUiDevice(dc);
            }

            else if (typeName == "mockdisplay")
            {
                return new MockDisplay(key, name);
            }

            else if (typeName == "generic")
            {
                return new Device(key, name);
            }

            // MOVE into something else???
            else if (typeName == "basicirdisplay")
            {
                var ir = IRPortHelper.GetIrPort(properties);
                if (ir != null)
                    return new BasicIrDisplay(key, name, ir.Port, ir.FileName);
            }

            else if (typeName == "commmock")
            {
                var comm = CommFactory.CreateCommForDevice(dc);
                var props = JsonConvert.DeserializeObject<ConsoleCommMockDevicePropertiesConfig>(
                    properties.ToString());
                return new ConsoleCommMockDevice(key, name, props, comm);
            }

            else if (typeName == "appserver")
            {
                var props = JsonConvert.DeserializeObject<CotijaConfig>(properties.ToString());
                return new CotijaSystemController(key, name, props);
            }

			else if (typeName == "mobilecontrolbridge-ddvc01")
			{
				var comm = CommFactory.GetControlPropertiesConfig(dc);

				var bridge = new PepperDash.Essentials.Room.Cotija.CotijaDdvc01RoomBridge(key, name, comm.IpIdInt);
				bridge.AddPreActivationAction(() =>
				{
					var parent = DeviceManager.AllDevices.FirstOrDefault(d => d.Key == "appServer") as CotijaSystemController;
					if (parent == null)
					{
						Debug.Console(0, bridge, "ERROR: Cannot connect bridge. System controller not present");
					}
					Debug.Console(0, bridge, "Linking to parent controller");
					bridge.AddParent(parent);
					parent.AddBridge(bridge);
				});

				return bridge;
			}

			return null;
		}
	}

}
