using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

using PepperDash.Essentials.Devices.Common.DSP;

using PepperDash.Essentials.Devices.Common;

namespace PepperDash.Essentials.Devices.Common
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
			var groupName = dc.Group.ToLower();

			if (typeName == "appletv")
			{
				//var ir = IRPortHelper.GetIrPort(properties);
				//if (ir != null)
				//    return new AppleTV(key, name, ir.Port, ir.FileName);

				var irCont = IRPortHelper.GetIrOutputPortController(dc);
				return new AppleTV(key, name, irCont);
			}

			else if (typeName == "basicirdisplay")
			{
				var ir = IRPortHelper.GetIrPort(properties);
				if (ir != null)
					return new BasicIrDisplay(key, name, ir.Port, ir.FileName);
			}

            else if (typeName == "biamptesira")
            {
                var comm = CommFactory.CreateCommForDevice(dc);
                var props = JsonConvert.DeserializeObject<BiampTesiraFortePropertiesConfig>(
                    properties.ToString());
                return new BiampTesiraForteDsp(key, name, comm, props);
            }

			else if (typeName == "cenrfgwex")
			{
				return CenRfgwController.GetNewExGatewayController(key, name,
					properties.Value<string>("id"), properties.Value<string>("gatewayType"));
			}

			else if (typeName == "cenerfgwpoe")
			{
				return CenRfgwController.GetNewErGatewayController(key, name,
					properties.Value<string>("id"), properties.Value<string>("gatewayType"));
			}

            else if (groupName == "discplayer") // (typeName == "irbluray")
            {
                if (properties["control"]["method"].Value<string>() == "ir")
                {
                    var irCont = IRPortHelper.GetIrOutputPortController(dc);
                    return new IRBlurayBase(key, name, irCont);
                }
                else if (properties["control"]["method"].Value<string>() == "com")
                {
                    Debug.Console(0, "[{0}] COM Device type not implemented YET!", key);
                }
            }

			else if (typeName == "genericaudiooutwithvolume")
			{
				var zone = dc.Properties.Value<uint>("zone");
				return new GenericAudioOutWithVolume(key, name,
					dc.Properties.Value<string>("volumeDeviceKey"), zone);
			}

            else if (groupName == "genericsource")
            {
                return new GenericSource(key, name);
            }

            else if (typeName == "inroompc")
            {
                return new InRoomPc(key, name);
            }

            else if (typeName == "laptop")
            {
                return new Laptop(key, name);
            }

            else if (groupName == "settopbox") //(typeName == "irstbbase")
            {
                var irCont = IRPortHelper.GetIrOutputPortController(dc);
                var config = dc.Properties.ToObject<SetTopBoxPropertiesConfig>();
                var stb = new IRSetTopBoxBase(key, name, irCont, config);

                //stb.HasDvr = properties.Value<bool>("hasDvr");
                var listName = properties.Value<string>("presetsList");
                if (listName != null)
                    stb.LoadPresets(listName);
                return stb;
            }

            else if (typeName == "roku")
            {
                var irCont = IRPortHelper.GetIrOutputPortController(dc);
                return new Roku2(key, name, irCont);
            }

			return null;
		}
	}
}