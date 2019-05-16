using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

using Newtonsoft.Json.Linq;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices;
//using PepperDash.Essentials.Devices.Dm;
using PepperDash.Core;

namespace PepperDash.Essentials
{
	public class DmFactory
	{
		public static Device Create(JToken devToken)
		{
			Device dev = null;
			try
			{
				var devType = devToken.Value<string>("type");
				var devKey = devToken.Value<string>("key");
				var devName = devToken.Value<string>("name");
				// Catch all 200 series TX
				var devprops = devToken["properties"];
				var ipId = Convert.ToUInt32(devprops.Value<string>("ipId"), 16);
				var parent = devprops.Value<string>("parent");
				if (parent == null)
					parent = "controlSystem";

				if (devType.StartsWith("DmTx2", StringComparison.OrdinalIgnoreCase))
				{
					DmTx201C tx;
					if (parent.Equals("controlSystem", StringComparison.OrdinalIgnoreCase))
					{
						tx = new DmTx201C(ipId, Global.ControlSystem);
						//dev = new DmTx201SBasicController(devKey, devName, tx);
					}
					
				}
				else if (devType.StartsWith("DmRmc", StringComparison.OrdinalIgnoreCase))
				{
					DmRmc100C rmc;
					if (parent.Equals("controlSystem", StringComparison.OrdinalIgnoreCase))
					{
						rmc = new DmRmc100C(ipId, Global.ControlSystem);
						//dev = new DmRmcBaseController(devKey, devName, rmc);
					}
				}
				else
					FactoryHelper.HandleUnknownType(devToken, devType);
			}
			catch (Exception e)
			{
				FactoryHelper.HandleDeviceCreationError(devToken, e);
			}
			return dev;
		}


		public static Device CreateChassis(JToken devToken)
		{
			Device dev = null;
			try
			{
				var devType = devToken.Value<string>("type");
				var devKey = devToken.Value<string>("key");
				var devName = devToken.Value<string>("name");
				// Catch all 200 series TX
				var devprops = devToken["properties"];
				var ipId = Convert.ToUInt32(devprops.Value<string>("ipId"), 16);
				var parent = devprops.Value<string>("parent");
				if (parent == null)
					parent = "controlSystem";

				if (devType.Equals("dmmd8x8", StringComparison.OrdinalIgnoreCase))
				{
					var dm = new DmMd8x8(ipId, Global.ControlSystem);
					//dev = new DmChassisController(devKey, devName, dm);
				}
				else if (devType.Equals("dmmd16x16", StringComparison.OrdinalIgnoreCase))
				{
					var dm = new DmMd16x16(ipId, Global.ControlSystem);
					//dev = new DmChassisController(devKey, devName, dm);
				}
				else if (devType.Equals("dmmd32x32", StringComparison.OrdinalIgnoreCase))
				{
					var dm = new DmMd32x32(ipId, Global.ControlSystem);
					//dev = new DmChassisController(devKey, devName, dm);
				}
			}
			catch (Exception e)
			{
				FactoryHelper.HandleDeviceCreationError(devToken, e);
			}
			return dev;
		}
	}
}