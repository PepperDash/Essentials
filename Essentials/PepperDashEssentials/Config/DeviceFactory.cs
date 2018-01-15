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
                var comm = CommFactory.GetControlPropertiesConfig(dc);

                var props = JsonConvert.DeserializeObject<CrestronTouchpanelPropertiesConfig>(
                    properties.ToString());
                return new EssentialsTouchpanelController(key, name, typeName, props, comm.IpIdInt);
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

            else if (typeName == "webserver")
            {
                var props = JsonConvert.DeserializeObject<CotijaConfig>(properties.ToString());
                return new CotijaSystemController(key, name, props);
            }

			else if (typeName == "cotijaddvc01room")
			{
				var comm = CommFactory.GetControlPropertiesConfig(dc);
				return new PepperDash.Essentials.Room.Cotija.CotijaDdvc01RoomBridge(key, name, comm.IpIdInt);
			}

			return null;
		}
	}
}
