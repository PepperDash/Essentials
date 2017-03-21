using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM
{
	public abstract class DmRmcControllerBase : CrestronGenericBaseDevice
	{
		public DmRmcControllerBase(string key, string name, EndpointReceiverBase device)
			: base(key, name, device)
		{ }
	}

	public class DmRmcHelper
	{
		/// <summary>
		/// A factory method for various DmTxControllers
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="props"></param>
		/// <returns></returns>
		public static DmRmcControllerBase GetDmRmcController(string key, string name, string typeName, DmRmcPropertiesConfig props)
		{
			// switch on type name... later...

			typeName = typeName.ToLower();
			uint ipid = props.Control.IpIdInt; // Convert.ToUInt16(props.Id, 16);



            // right here, we need to grab the tie line that associates this
            // RMC with a chassis or processor.  If the RMC input's tie line is not
            // connected to a chassis, then it's parent is the processor.
            // If the RMC is connected to a chassis, then we need to grab the 
            // output number from the tie line and use that to plug it in.
            // Example of chassis-connected:
            //{
            //  "sourceKey": "dmMd8x8-1",
            //  "sourcePort": "anyOut2",
            //  "destinationKey": "dmRmc100C-2",
            //  "destinationPort": "DmIn"
            //}

            // Tx -> RMC link:
            //{
            //  "sourceKey": "dmTx201C-1",
            //  "sourcePort": "DmOut",
            //  "destinationKey": "dmRmc100C-2",
            //  "destinationPort": "DmIn"
            //}

            var tlc = TieLineCollection.Default;
            // grab the tie line that has this key as 
            // THIS DOESN'T WORK BECAUSE THE RMC THAT WE NEED (THIS) HASN'T BEEN MADE
            // YET AND THUS WILL NOT HAVE A TIE LINE...
            var inputTieLine = tlc.FirstOrDefault(t => 
                {
                    var d = t.DestinationPort.ParentDevice;
                    return d.Key.Equals(key, StringComparison.OrdinalIgnoreCase)
                        && d is DmChassisController;
                });

			var pKey = props.ParentDeviceKey.ToLower();




			// Non-DM-chassis endpoints
			if (pKey == "processor")
			{
				// Catch constructor failures, mainly dues to IPID
				try
				{
					if (typeName.StartsWith("dmrmc100c"))
						return new DmRmc100CController(key, name, new DmRmc100C(ipid, Global.ControlSystem));
					if (typeName.StartsWith("dmrmcscalerc"))
						return new DmRmcScalerCController(key, name, new DmRmcScalerC(ipid, Global.ControlSystem));
					if (typeName.StartsWith("dmrmc4kscalerc"))
						return new DmRmc4kScalerCController(key, name, new DmRmc4kScalerC(ipid, Global.ControlSystem));
				}
				catch (Exception e)
				{
					Debug.Console(0, "[{0}] WARNING: Cannot create DM-RMC device: {1}", key, e.Message);
				}


				Debug.Console(0, "Cannot create DM-RMC of type: '{0}'", typeName);
			}
			// Endpoints attached to DM Chassis
			else
			{
				var parentDev = DeviceManager.GetDeviceForKey(pKey);
				if (!(parentDev is DmChassisController))
				{
					Debug.Console(0, "Cannot create DM device '{0}'. '{1}' is not a DM Chassis.",
						key, pKey);
					return null;
				}

				var chassis = (parentDev as DmChassisController).Chassis;
				var num = props.ParentOutputNumber;
				if (num <= 0 || num > chassis.NumberOfOutputs)
				{
					Debug.Console(0, "Cannot create DM device '{0}'. Output number '{1}' is out of range",
						key, num);
					return null;
				}

				try
				{
					if (typeName.StartsWith("dmrmc100c"))
						return new DmRmc100CController(key, name, new DmRmc100C(ipid, chassis.Outputs[num]));
					if (typeName.StartsWith("dmrmcscalerc"))
						return new DmRmcScalerCController(key, name, new DmRmcScalerC(ipid, chassis.Outputs[num]));
					if (typeName.StartsWith("dmrmc4kscalerc"))
						return new DmRmc4kScalerCController(key, name, new DmRmc4kScalerC(ipid, chassis.Outputs[num]));
				}
				catch (Exception e)
				{
					Debug.Console(0, "[{0}] WARNING: Cannot create DM-RMC device: {1}", key, e.Message);
				}
			}

			return null;
		}
	}
}