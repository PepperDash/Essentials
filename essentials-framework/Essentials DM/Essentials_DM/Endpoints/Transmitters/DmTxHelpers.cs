using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
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
					Debug.Console(0, "[{0}] WARNING: Cannot create DM-TX device: {1}", key, e);
				}
			}
			else
			{
				var parentDev = DeviceManager.GetDeviceForKey(pKey);
				if (!(parentDev is IDmSwitch))
				{
					Debug.Console(0, "Cannot create DM device '{0}'. '{1}' is not a DM Chassis.",
						key, pKey);
					return null;
				}

                // Get the Crestron chassis and link stuff up
				var switchDev = (parentDev as IDmSwitch);
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
                    var controller = (parentDev as IDmSwitch);
                    controller.TxDictionary.Add(num, key);
                }

				// Catch constructor failures, mainly dues to IPID
				try
				{
					// Must use different constructor for CPU3 chassis types. No IPID
					if (chassis is DmMd8x8Cpu3 || chassis is DmMd16x16Cpu3 ||
					chassis is DmMd32x32Cpu3 || chassis is DmMd8x8Cpu3rps ||
					chassis is DmMd16x16Cpu3rps || chassis is DmMd32x32Cpu3rps||
                    chassis is DmMd128x128 || chassis is DmMd64x64)
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
					Debug.Console(0, "[{0}] WARNING: Cannot create DM-TX device: {1}", key, e);
				}
			}
			
			return null;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public abstract class DmTxControllerBase : CrestronGenericBridgeableBaseDevice
	{
        public virtual void SetPortHdcpCapability(eHdcpCapabilityType hdcpMode, uint port) { }
        public virtual eHdcpCapabilityType HdcpSupportCapability { get; protected set; }
        public abstract StringFeedback ActiveVideoInputFeedback { get; protected set; }
        public RoutingInputPortWithVideoStatuses AnyVideoInput { get; protected set; }

	    protected DmTxControllerBase(string key, string name, EndpointTransmitterBase hardware)
			: base(key, name, hardware) 
		{
			// if wired to a chassis, skip registration step in base class
			if (hardware.DMInput != null)
			{
				this.PreventRegistration = true;
			}
            AddToFeedbackList(ActiveVideoInputFeedback);
		}

	    protected DmTxControllerBase(string key, string name, DmHDBasedTEndPoint hardware) : base(key, name, hardware)
	    {
	    }

	    protected void LinkDmTxToApi(DmTxControllerBase tx, BasicTriList trilist, uint joinStart, string joinMapKey,
	        EiscApiAdvanced bridge)
	    {
            var joinMap = new DmTxControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmTxControllerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

	        if (tx.Hardware is DmHDBasedTEndPoint)
	        {
	            Debug.Console(1, tx, "No properties to link. Skipping device {0}", tx.Name);
	            return;
	        }

            Debug.Console(1, tx, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            tx.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            tx.AnyVideoInput.VideoStatus.VideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus]);
            tx.AnyVideoInput.VideoStatus.VideoResolutionFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentInputResolution]);
            trilist.UShortInput[joinMap.HdcpSupportCapability].UShortValue = (ushort)tx.HdcpSupportCapability;

            bool hdcpTypeSimple;

            if (tx.Hardware is DmTx4kX02CBase)
                hdcpTypeSimple = false;
            else
                hdcpTypeSimple = true;

            if (tx is ITxRouting)
            {
                var txR = tx as ITxRouting;

                trilist.SetUShortSigAction(joinMap.VideoInput,
                    i => txR.ExecuteNumericSwitch(i, 0, eRoutingSignalType.Video));
                trilist.SetUShortSigAction(joinMap.AudioInput,
                    i => txR.ExecuteNumericSwitch(i, 0, eRoutingSignalType.Audio));

                txR.VideoSourceNumericFeedback.LinkInputSig(trilist.UShortInput[joinMap.VideoInput]);
                txR.AudioSourceNumericFeedback.LinkInputSig(trilist.UShortInput[joinMap.AudioInput]);

                trilist.UShortInput[joinMap.HdcpSupportCapability].UShortValue = (ushort)tx.HdcpSupportCapability;

                if (txR.InputPorts[DmPortName.HdmiIn] != null)
                {
                    var inputPort = txR.InputPorts[DmPortName.HdmiIn];

                    if (tx.Feedbacks["HdmiInHdcpCapability"] != null)
                    {
                        var intFeedback = tx.Feedbacks["HdmiInHdcpCapability"] as IntFeedback;
                        if (intFeedback != null)
                            intFeedback.LinkInputSig(trilist.UShortInput[joinMap.Port1HdcpState]);
                    }

                    if (inputPort.ConnectionType == eRoutingPortConnectionType.Hdmi && inputPort.Port != null)
                    {
                        var port = inputPort.Port as EndpointHdmiInput;

                        SetHdcpCapabilityAction(hdcpTypeSimple, port, joinMap.Port1HdcpState, trilist);
                    }
                }

                if (txR.InputPorts[DmPortName.HdmiIn1] != null)
                {
                    var inputPort = txR.InputPorts[DmPortName.HdmiIn1];

                    if (tx.Feedbacks["HdmiIn1HdcpCapability"] != null)
                    {
                        var intFeedback = tx.Feedbacks["HdmiIn1HdcpCapability"] as IntFeedback;
                        if (intFeedback != null)
                            intFeedback.LinkInputSig(trilist.UShortInput[joinMap.Port1HdcpState]);
                    }

                    if (inputPort.ConnectionType == eRoutingPortConnectionType.Hdmi && inputPort.Port != null)
                    {
                        var port = inputPort.Port as EndpointHdmiInput;

                        SetHdcpCapabilityAction(hdcpTypeSimple, port, joinMap.Port1HdcpState, trilist);
                    }
                }

                if (txR.InputPorts[DmPortName.HdmiIn2] != null)
                {
                    var inputPort = txR.InputPorts[DmPortName.HdmiIn2];

                    if (tx.Feedbacks["HdmiIn2HdcpCapability"] != null)
                    {
                        var intFeedback = tx.Feedbacks["HdmiIn2HdcpCapability"] as IntFeedback;
                        if (intFeedback != null)
                            intFeedback.LinkInputSig(trilist.UShortInput[joinMap.Port1HdcpState]);
                    }

                    if (inputPort.ConnectionType == eRoutingPortConnectionType.Hdmi && inputPort.Port != null)
                    {
                        var port = inputPort.Port as EndpointHdmiInput;

                        SetHdcpCapabilityAction(hdcpTypeSimple, port, joinMap.Port2HdcpState, trilist);
                    }
                }

            }

            var txFreeRun = tx as IHasFreeRun;
            if (txFreeRun != null)
            {
                txFreeRun.FreeRunEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.FreeRunEnabled]);
                trilist.SetBoolSigAction(joinMap.FreeRunEnabled, new Action<bool>(txFreeRun.SetFreeRunEnabled));
            }

            var txVga = tx as IVgaBrightnessContrastControls;
            {
                if (txVga == null) return;

                txVga.VgaBrightnessFeedback.LinkInputSig(trilist.UShortInput[joinMap.VgaBrightness]);
                txVga.VgaContrastFeedback.LinkInputSig(trilist.UShortInput[joinMap.VgaContrast]);

                trilist.SetUShortSigAction(joinMap.VgaBrightness, txVga.SetVgaBrightness);
                trilist.SetUShortSigAction(joinMap.VgaContrast, txVga.SetVgaContrast);
            }
        }

        private void SetHdcpCapabilityAction(bool hdcpTypeSimple, EndpointHdmiInput port, uint join, BasicTriList trilist)
        {
            if (hdcpTypeSimple)
            {
                trilist.SetUShortSigAction(join,
                    s =>
                    {
                        if (s == 0)
                        {
                            port.HdcpSupportOff();
                        }
                        else if (s > 0)
                        {
                            port.HdcpSupportOn();
                        }
                    });
            }
            else
            {
                trilist.SetUShortSigAction(join,
                        s =>
                        {
                            port.HdcpCapability = (eHdcpCapabilityType)s;
                        });
            }
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