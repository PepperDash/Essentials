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
        public static BasicDmTxControllerBase GetDmTxController(string key, string name, string typeName, DmTxPropertiesConfig props)
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
                    if(typeName.StartsWith("dmtx200"))
                        return new DmTx200Controller(key, name, new DmTx200C2G(ipid, Global.ControlSystem));
					if (typeName.StartsWith("dmtx201"))
						return new DmTx201XController(key, name, new DmTx201S(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmtx4k202"))
                        return new DmTx4k202CController(key, name, new DmTx4k202C(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmtx4kz202"))
                        return new DmTx4k202CController(key, name, new DmTx4kz202C(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmtx4k302"))
						return new DmTx4k302CController(key, name, new DmTx4k302C(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmtx4kz302"))
                        return new DmTx4kz302CController(key, name, new DmTx4kz302C(ipid, Global.ControlSystem));
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
                else
                {
                    var controller = (parentDev as DmChassisController);
                    controller.TxDictionary.Add(num, key);
                }

				// Catch constructor failures, mainly dues to IPID
				try
				{
					// Must use different constructor for CPU3 chassis types. No IPID
					if (chassis is DmMd8x8Cpu3 || chassis is DmMd16x16Cpu3 ||
					chassis is DmMd32x32Cpu3 || chassis is DmMd8x8Cpu3rps ||
					chassis is DmMd16x16Cpu3rps || chassis is DmMd32x32Cpu3rps)
					{
						if (typeName.StartsWith("dmtx200"))
							return new DmTx200Controller(key, name, new DmTx200C2G(chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx201"))
							return new DmTx201XController(key, name, new DmTx201C(chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4k100"))
							return new DmTx4k100Controller(key, name, new DmTx4K100C1G(chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4k202"))
							return new DmTx4k202CController(key, name, new DmTx4k202C(chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4kz202"))
							return new DmTx4k202CController(key, name, new DmTx4kz202C(chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4k302"))
							return new DmTx4k302CController(key, name, new DmTx4k302C(chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4kz302"))
							return new DmTx4kz302CController(key, name, new DmTx4kz302C(chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx401"))
							return new DmTx401CController(key, name, new DmTx401C(chassis.Inputs[num]));
					}
					else
					{
						if (typeName.StartsWith("dmtx200"))
							return new DmTx200Controller(key, name, new DmTx200C2G(ipid, chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx201"))
							return new DmTx201XController(key, name, new DmTx201C(ipid, chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4k100"))
							return new DmTx4k100Controller(key, name, new DmTx4K100C1G(ipid, chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4k202"))
							return new DmTx4k202CController(key, name, new DmTx4k202C(ipid, chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4kz202"))
							return new DmTx4k202CController(key, name, new DmTx4kz202C(ipid, chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4k302"))
							return new DmTx4k302CController(key, name, new DmTx4k302C(ipid, chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4kz302"))
							return new DmTx4kz302CController(key, name, new DmTx4kz302C(ipid, chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx401"))
							return new DmTx401CController(key, name, new DmTx401C(ipid, chassis.Inputs[num]));
					}
				}
				catch (Exception e)
				{
					Debug.Console(0, "[{0}] WARNING: Cannot create DM-TX device: {1}", key, e.Message);
				}
			}
			
			return null;
		}
	}

    public abstract class BasicDmTxControllerBase : CrestronGenericBaseDevice
    {
        public BasicDmTxControllerBase(string key, string name, GenericBase hardware)
            : base(key, name, hardware)
        {

        }
    }

	/// <summary>
	/// 
	/// </summary>
	public abstract class DmTxControllerBase : BasicDmTxControllerBase
	{
        public virtual void SetPortHdcpCapability(eHdcpCapabilityType hdcpMode, uint port) { }
        public virtual eHdcpCapabilityType HdcpSupportCapability { get; protected set; }
        public abstract StringFeedback ActiveVideoInputFeedback { get; protected set; }
        public RoutingInputPortWithVideoStatuses AnyVideoInput { get; protected set; }

        public DmTxControllerBase(string key, string name, EndpointTransmitterBase hardware)
			: base(key, name, hardware) 
		{
			// if wired to a chassis, skip registration step in base class
			if (hardware.DMInput != null)
			{
				this.PreventRegistration = true;
			}
            AddToFeedbackList(ActiveVideoInputFeedback);
		}
	}
    //public enum ePdtHdcpSupport
    //{
    //    HdcpOff = 0,
    //    Hdcp1 = 1,
    //    Hdcp2 = 2,
    //    Hdcp2_2= 3,
    //    Auto = 99
    //}
}