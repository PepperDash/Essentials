using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM.AirMedia;
using Crestron.SimplSharpPro.UI;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.DM.AirMedia;
using PepperDash.Essentials.DM.Endpoints.DGEs;

namespace PepperDash.Essentials.DM
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

            if (typeName.StartsWith("am"))
            {
                var props = JsonConvert.DeserializeObject<AirMediaPropertiesConfig>(properties.ToString());
                AmX00 amDevice = null;
                if (typeName == "am200")
                    amDevice = new Crestron.SimplSharpPro.DM.AirMedia.Am200(props.Control.IpIdInt, Global.ControlSystem);
                else if(typeName == "am300")
                    amDevice = new Crestron.SimplSharpPro.DM.AirMedia.Am300(props.Control.IpIdInt, Global.ControlSystem);

                return new AirMediaController(key, name, amDevice, dc, props);
            }
			else if (typeName.StartsWith("dmmd8x") || typeName.StartsWith("dmmd16x") || typeName.StartsWith("dmmd32x"))
			{
				var props = JsonConvert.DeserializeObject
					<PepperDash.Essentials.DM.Config.DMChassisPropertiesConfig>(properties.ToString());
				return PepperDash.Essentials.DM.DmChassisController.
					GetDmChassisController(key, name, type, props);
			}
			// Hand off to DmTxHelper class
			else if (typeName.StartsWith("dmtx"))
			{
				var props = JsonConvert.DeserializeObject
					<PepperDash.Essentials.DM.Config.DmTxPropertiesConfig>(properties.ToString());
				return PepperDash.Essentials.DM.DmTxHelper.GetDmTxController(key, name, type, props);
			}

			// Hand off to DmRmcHelper class
			else if (typeName.StartsWith("dmrmc"))
			{
				var props = JsonConvert.DeserializeObject
					<PepperDash.Essentials.DM.Config.DmRmcPropertiesConfig>(properties.ToString());
				return PepperDash.Essentials.DM.DmRmcHelper.GetDmRmcController(key, name, type, props);
			}

			else if (typeName.Equals("hdmd4x14ke"))
			{
				var props = JsonConvert.DeserializeObject
					<PepperDash.Essentials.DM.Config.HdMdNxM4kEPropertiesConfig>(properties.ToString());
				return PepperDash.Essentials.DM.Chassis.HdMdNxM4kEController.GetController(key, name, type, props);
			}

            

			return null;
		}


	}

}