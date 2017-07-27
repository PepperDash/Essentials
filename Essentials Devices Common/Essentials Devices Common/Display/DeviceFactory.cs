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

namespace PepperDash.Essentials.Devices.Displays
{
	public class DisplayDeviceFactory
	{
		public static IKeyed GetDevice(DeviceConfig dc)
		{
			var key = dc.Key;
			var name = dc.Name;
			var type = dc.Type;
			var properties = dc.Properties;

			var typeName = dc.Type.ToLower();
			//if (typeName == "dmmd8x8")
			//{
			//    var props = JsonConvert.DeserializeObject
			//        <PepperDash.Essentials.DM.Config.DMChassisPropertiesConfig>(properties.ToString());
			//    return PepperDash.Essentials.DM.DmChassisController.
			//        GetDmChassisController(key, name, type, props);
			//}

			try
			{
				if (typeName == "necmpsx")
				{
					var comm = CommFactory.CreateCommForDevice(dc);
					if (comm != null)
						return new NecPSXMDisplay(dc.Key, dc.Name, comm);
				}
                else if(typeName == "samsungmdc")
                {
                    var comm = CommFactory.CreateCommForDevice(dc);
                    if (comm != null)
                        return new SamsungMDC(dc.Key, dc.Name, comm, Convert.ToByte(dc.Properties["byte"]));
                }
                   
			}
			catch (Exception e)
			{
				Debug.Console(0, "Displays factory: Exception creating device type {0}, key {1}: {2}", dc.Type, dc.Key, e.Message);
				return null;
			}

			return null;
		}
	}
}