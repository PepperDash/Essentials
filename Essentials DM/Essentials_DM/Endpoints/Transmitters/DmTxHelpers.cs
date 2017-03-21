using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM
{
	public class DmTxHelper
	{
		/// <summary>
		/// A factory method for various DmTxControllers
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="props"></param>
		/// <returns></returns>
		public static DmTxControllerBase GetDmTxController(string key, string name, string typeName, DmTxPropertiesConfig props)
		{
			// switch on type name... later...

			typeName = typeName.ToLower();
			//uint ipid = Convert.ToUInt16(props.Id, 16);
			var ipid = props.Control.IpIdInt;
			var pKey = props.ParentDeviceKey.ToLower();

			if (pKey == "processor")
			{
				// Catch constructor failures, mainly dues to IPID
				try
				{
					if (typeName.StartsWith("dmtx201"))
						return new DmTx201SBasicController(key, name, new DmTx201C(ipid, Global.ControlSystem));
					if (typeName.StartsWith("dmtx4k302"))
						return new DmTx4k302CController(key, name, new DmTx4k302C(ipid, Global.ControlSystem));
					if (typeName.StartsWith("dmtx401"))
						return new DmTx401CController(key, name, new DmTx401C(ipid, Global.ControlSystem));
					Debug.Console(0, "{1} WARNING: Cannot create DM-TX of type: '{0}'", typeName, key);
				}
				catch (Exception e)
				{
					Debug.Console(0, "[{0}] WARNING: Cannot create DM-TX device: {1}", key, e.Message);
				}
			}
			else
			{
				var parentDev = DeviceManager.GetDeviceForKey(pKey);
				if (!(parentDev is DmChassisController))
				{
					Debug.Console(0, "Cannot create DM device '{0}'. '{1}' is not a DM Chassis.",
						key, pKey);
					return null;
				}

                // Get the Crestron chassis and link stuff up
				var switchDev = parentDev as DmChassisController;
				var chassis = switchDev.Chassis;

				var num = props.ParentInputNumber;
				if (num <= 0 || num > chassis.NumberOfInputs)
				{
					Debug.Console(0, "Cannot create DM device '{0}'.  Input number '{1}' is out of range",
						key, num);
					return null;
				}

				// Catch constructor failures, mainly dues to IPID
				try
				{
					if (typeName.StartsWith("dmtx201"))
						return new DmTx201SBasicController(key, name, new DmTx201S(ipid, chassis.Inputs[num]));

					if (typeName.StartsWith("dmtx4k302"))
						return new DmTx4k302CController(key, name, new DmTx4k302C(ipid, chassis.Inputs[num]));

					if (typeName.StartsWith("dmtx401"))
						return new DmTx401CController(key, name, new DmTx401C(ipid, chassis.Inputs[num]));
				}
				catch (Exception e)
				{
					Debug.Console(0, "[{0}] WARNING: Cannot create DM-TX device: {1}", key, e.Message);
				}
			}
			
			return null;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public abstract class DmTxControllerBase : CrestronGenericBaseDevice
	{
		public DmTxControllerBase(string key, string name, EndpointTransmitterBase hardware)
			: base(key, name, hardware) { }
	}
}