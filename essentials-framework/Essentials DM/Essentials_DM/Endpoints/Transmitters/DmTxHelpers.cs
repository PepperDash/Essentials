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
using PepperDash.Essentials.Core.Config;


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
                    if (typeName.StartsWith("dmtx4kz100"))
                        return new DmTx4kz100Controller(key, name, new DmTx4kz100C1G(ipid, Global.ControlSystem));
					if (typeName.StartsWith("dmtx201c"))
						return new DmTx201CController(key, name, new DmTx201C(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmtx201s"))
                        return new DmTx201SController(key, name, new DmTx201S(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmtx4k202"))
                        return new DmTx4k202CController(key, name, new DmTx4k202C(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmtx4kz202"))
                        return new DmTx4kz202CController(key, name, new DmTx4kz202C(ipid, Global.ControlSystem));
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
						if (typeName.StartsWith("dmtx201c"))
							return new DmTx201CController(key, name, new DmTx201C(chassis.Inputs[num]));
                        if (typeName.StartsWith("dmtx201s"))
                            return new DmTx201SController(key, name, new DmTx201S(chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4k100"))
							return new DmTx4k100Controller(key, name, new DmTx4K100C1G(chassis.Inputs[num]));
                        if (typeName.StartsWith("dmtx4kz100"))
                            return new DmTx4kz100Controller(key, name, new DmTx4kz100C1G(chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4k202"))
							return new DmTx4k202CController(key, name, new DmTx4k202C(chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4kz202"))
							return new DmTx4kz202CController(key, name, new DmTx4kz202C(chassis.Inputs[num]));
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
						if (typeName.StartsWith("dmtx201c"))
							return new DmTx201CController(key, name, new DmTx201C(ipid, chassis.Inputs[num]));
                        if (typeName.StartsWith("dmtx201s"))
                            return new DmTx201SController(key, name, new DmTx201S(ipid, chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4k100"))
							return new DmTx4k100Controller(key, name, new DmTx4K100C1G(ipid, chassis.Inputs[num]));
                        if (typeName.StartsWith("dmtx4kz100"))
                            return new DmTx4kz100Controller(key, name, new DmTx4kz100C1G(ipid, chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4k202"))
							return new DmTx4k202CController(key, name, new DmTx4k202C(ipid, chassis.Inputs[num]));
						if (typeName.StartsWith("dmtx4kz202"))
							return new DmTx4kz202CController(key, name, new DmTx4kz202C(ipid, chassis.Inputs[num]));
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

    public abstract class BasicDmTxControllerBase : CrestronGenericBridgeableBaseDevice
    {
        protected BasicDmTxControllerBase(string key, string name, GenericBase hardware)
            : base(key, name, hardware)
        {

        }
    }

	/// <summary>
	/// 
	/// </summary>
    [Description("Wrapper class for all DM-TX variants")]
	public abstract class DmTxControllerBase : BasicDmTxControllerBase
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

        protected DmTxControllerJoinMap GetDmTxJoinMap(uint joinStart, string joinMapKey)
        {
            var joinMap = new DmTxControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmTxControllerJoinMap>(joinMapSerialized);

            return joinMap;
        }

	    protected void LinkDmTxToApi(DmTxControllerBase tx, BasicTriList trilist, DmTxControllerJoinMap joinMap, EiscApiAdvanced bridge)
	    {
	        Debug.Console(1, tx, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            tx.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
            tx.AnyVideoInput.VideoStatus.VideoSyncFeedback.LinkInputSig(trilist.BooleanInput[joinMap.VideoSyncStatus.JoinNumber]);
            tx.AnyVideoInput.VideoStatus.VideoResolutionFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentInputResolution.JoinNumber]);
            trilist.UShortInput[joinMap.HdcpSupportCapability.JoinNumber].UShortValue = (ushort)tx.HdcpSupportCapability;

            bool hdcpTypeSimple;

            if (tx.Hardware is DmTx4kX02CBase)
                hdcpTypeSimple = false;
            else
                hdcpTypeSimple = true;

            if (tx is ITxRouting)
            {
                var txR = tx as ITxRouting;

                trilist.SetUShortSigAction(joinMap.VideoInput.JoinNumber,
                    i => txR.ExecuteNumericSwitch(i, 0, eRoutingSignalType.Video));
                trilist.SetUShortSigAction(joinMap.AudioInput.JoinNumber,
                    i => txR.ExecuteNumericSwitch(i, 0, eRoutingSignalType.Audio));

                txR.VideoSourceNumericFeedback.LinkInputSig(trilist.UShortInput[joinMap.VideoInput.JoinNumber]);
                txR.AudioSourceNumericFeedback.LinkInputSig(trilist.UShortInput[joinMap.AudioInput.JoinNumber]);

                trilist.UShortInput[joinMap.HdcpSupportCapability.JoinNumber].UShortValue = (ushort)tx.HdcpSupportCapability;

                if (txR.InputPorts[DmPortName.HdmiIn] != null)
                {
                    var inputPort = txR.InputPorts[DmPortName.HdmiIn];

                    if (tx.Feedbacks["HdmiInHdcpCapability"] != null)
                    {
                        var intFeedback = tx.Feedbacks["HdmiInHdcpCapability"] as IntFeedback;
                        if (intFeedback != null)
                            intFeedback.LinkInputSig(trilist.UShortInput[joinMap.Port1HdcpState.JoinNumber]);
                    }

                    if (inputPort.ConnectionType == eRoutingPortConnectionType.Hdmi && inputPort.Port != null)
                    {
                        var port = inputPort.Port as EndpointHdmiInput;

                        SetHdcpCapabilityAction(hdcpTypeSimple, port, joinMap.Port1HdcpState.JoinNumber, trilist);
                    }
                }

                if (txR.InputPorts[DmPortName.HdmiIn1] != null)
                {
                    var inputPort = txR.InputPorts[DmPortName.HdmiIn1];

                    if (tx.Feedbacks["HdmiIn1HdcpCapability"] != null)
                    {
                        var intFeedback = tx.Feedbacks["HdmiIn1HdcpCapability"] as IntFeedback;
                        if (intFeedback != null)
                            intFeedback.LinkInputSig(trilist.UShortInput[joinMap.Port1HdcpState.JoinNumber]);
                    }

                    if (inputPort.ConnectionType == eRoutingPortConnectionType.Hdmi && inputPort.Port != null)
                    {
                        var port = inputPort.Port as EndpointHdmiInput;

                        SetHdcpCapabilityAction(hdcpTypeSimple, port, joinMap.Port1HdcpState.JoinNumber, trilist);
                    }
                }

                if (txR.InputPorts[DmPortName.HdmiIn2] != null)
                {
                    var inputPort = txR.InputPorts[DmPortName.HdmiIn2];

                    if (tx.Feedbacks["HdmiIn2HdcpCapability"] != null)
                    {
                        var intFeedback = tx.Feedbacks["HdmiIn2HdcpCapability"] as IntFeedback;
                        if (intFeedback != null)
                            intFeedback.LinkInputSig(trilist.UShortInput[joinMap.Port1HdcpState.JoinNumber]);
                    }

                    if (inputPort.ConnectionType == eRoutingPortConnectionType.Hdmi && inputPort.Port != null)
                    {
                        var port = inputPort.Port as EndpointHdmiInput;

                        SetHdcpCapabilityAction(hdcpTypeSimple, port, joinMap.Port2HdcpState.JoinNumber, trilist);
                    }
                }

            }

            var txFreeRun = tx as IHasFreeRun;
            if (txFreeRun != null)
            {
                txFreeRun.FreeRunEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.FreeRunEnabled.JoinNumber]);
                trilist.SetBoolSigAction(joinMap.FreeRunEnabled.JoinNumber, txFreeRun.SetFreeRunEnabled);
            }

            var txVga = tx as IVgaBrightnessContrastControls;
            {
                if (txVga == null) return;

                txVga.VgaBrightnessFeedback.LinkInputSig(trilist.UShortInput[joinMap.VgaBrightness.JoinNumber]);
                txVga.VgaContrastFeedback.LinkInputSig(trilist.UShortInput[joinMap.VgaContrast.JoinNumber]);

                trilist.SetUShortSigAction(joinMap.VgaBrightness.JoinNumber, txVga.SetVgaBrightness);
                trilist.SetUShortSigAction(joinMap.VgaContrast.JoinNumber, txVga.SetVgaContrast);
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

    public class DmTxControllerFactory : EssentialsDeviceFactory<DmTxControllerBase>
    {
        public DmTxControllerFactory()
        {
            TypeNames = new List<string>() { "dmtx200c", "dmtx201c", "dmtx201s", "dmtx4k100c", "dmtx4k202c", "dmtx4kz202c", "dmtx4k302c", "dmtx4kz302c",
                "dmtx401c", "dmtx401s", "dmtx4k100c1g", "dmtx4kz100c1g"  };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var type = dc.Type.ToLower();

            Debug.Console(1, "Factory Attempting to create new DM-TX Device");

            var props = JsonConvert.DeserializeObject
                <PepperDash.Essentials.DM.Config.DmTxPropertiesConfig>(dc.Properties.ToString());
            return PepperDash.Essentials.DM.DmTxHelper.GetDmTxController(dc.Key, dc.Name, type, props);
        }
    }

}

