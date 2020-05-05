using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.DM.Config;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.DM
{
    [Description("Wrapper class for all DM-RMC variants")]
	public abstract class DmRmcControllerBase : CrestronGenericBridgeableBaseDevice
	{
        public StringFeedback VideoOutputResolutionFeedback { get; protected set; }
        public StringFeedback EdidManufacturerFeedback { get; protected set; }
        public StringFeedback EdidNameFeedback { get; protected set; }
        public StringFeedback EdidPreferredTimingFeedback { get; protected set; }
        public StringFeedback EdidSerialNumberFeedback { get; protected set; }

        protected DmRmcControllerBase(string key, string name, EndpointReceiverBase device)
			: base(key, name, device)
		{
			// if wired to a chassis, skip registration step in base class
            PreventRegistration = device.DMOutput != null;
			
            AddToFeedbackList(VideoOutputResolutionFeedback, EdidManufacturerFeedback, EdidSerialNumberFeedback, EdidNameFeedback, EdidPreferredTimingFeedback);
        }

        protected void LinkDmRmcToApi(DmRmcControllerBase rmc, BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DmRmcControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmRmcControllerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, rmc, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            rmc.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            if (rmc.VideoOutputResolutionFeedback != null)
                rmc.VideoOutputResolutionFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentOutputResolution]);
            if (rmc.EdidManufacturerFeedback != null)
                rmc.EdidManufacturerFeedback.LinkInputSig(trilist.StringInput[joinMap.EdidManufacturer]);
            if (rmc.EdidNameFeedback != null)
                rmc.EdidNameFeedback.LinkInputSig(trilist.StringInput[joinMap.EdidName]);
            if (rmc.EdidPreferredTimingFeedback != null)
                rmc.EdidPreferredTimingFeedback.LinkInputSig(trilist.StringInput[joinMap.EdidPrefferedTiming]);
            if (rmc.EdidSerialNumberFeedback != null)
                rmc.EdidSerialNumberFeedback.LinkInputSig(trilist.StringInput[joinMap.EdidSerialNumber]);
            
            //If the device is an DM-RMC-4K-Z-SCALER-C
            var routing = rmc as IRmcRouting;

            if (routing == null)
            {
                return;
            }

            if (routing.AudioVideoSourceNumericFeedback != null)
                routing.AudioVideoSourceNumericFeedback.LinkInputSig(trilist.UShortInput[joinMap.AudioVideoSource]);

            trilist.SetUShortSigAction(joinMap.AudioVideoSource, a => routing.ExecuteNumericSwitch(a, 1, eRoutingSignalType.AudioVideo));
        }
	}

    public abstract class DmHdBaseTControllerBase : CrestronGenericBaseDevice
    {
        public HDBaseTBase Rmc { get; protected set; }

        /// <summary>
        ///  Make a Crestron RMC and put it in here
        /// </summary>
        protected DmHdBaseTControllerBase(string key, string name, HDBaseTBase rmc)
            : base(key, name, rmc)
        {

        }
    }

	public class DmRmcHelper
	{
	    /// <summary>
	    /// A factory method for various DmRmcControllers
	    /// </summary>
	    /// <param name="key">device key. Used to uniquely identify device</param>
	    /// <param name="name">device name</param>
	    /// <param name="typeName">device type name. Used to retrived the correct device</param>
	    /// <param name="props">Config from config file</param>
	    /// <returns></returns>
	    public static CrestronGenericBaseDevice GetDmRmcController(string key, string name, string typeName, DmRmcPropertiesConfig props)
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

	        // grab the tie line that has this key as 
            // THIS DOESN'T WORK BECAUSE THE RMC THAT WE NEED (THIS) HASN'T BEEN MADE
            // YET AND THUS WILL NOT HAVE A TIE LINE...
			var pKey = props.ParentDeviceKey.ToLower();

			// Non-DM-chassis endpoints
			if (pKey == "processor")
			{
				// Catch constructor failures, mainly dues to IPID
				try
				{
                    if (typeName.StartsWith("dmrmc100c"))
						return new DmRmcX100CController(key, name, new DmRmc100C(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmrmc100s"))
                        return new DmRmc100SController(key, name, new DmRmc100S(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmrmc4k100c"))
                        return new DmRmcX100CController(key, name, new DmRmc4k100C(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmrmc4kz100c"))
                        return new DmRmc4kZ100CController(key, name, new DmRmc4kz100C(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmrmc150s"))
                        return new DmRmc150SController(key, name, new DmRmc150S(ipid, Global.ControlSystem));
					if (typeName.StartsWith("dmrmc200c"))
						return new DmRmc200CController(key, name, new DmRmc200C(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmrmc200s"))
                        return new DmRmc200SController(key, name, new DmRmc200S(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmrmc200s2"))
                        return new DmRmc200S2Controller(key, name, new DmRmc200S2(ipid, Global.ControlSystem));
					if (typeName.StartsWith("dmrmcscalerc"))
						return new DmRmcScalerCController(key, name, new DmRmcScalerC(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmrmcscalers"))
                        return new DmRmcScalerSController(key, name, new DmRmcScalerS(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmrmcscalers2"))
                        return new DmRmcScalerS2Controller(key, name, new DmRmcScalerS2(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmrmc4kscalerc"))
						return new DmRmc4kScalerCController(key, name, new DmRmc4kScalerC(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmrmc4kscalercdsp"))
                        return new DmRmc4kScalerCDspController(key, name, new DmRmc4kScalerCDsp(ipid, Global.ControlSystem));
                    if (typeName.StartsWith("dmrmc4kzscalerc"))
                        return new DmRmc4kZScalerCController(key, name, new DmRmc4kzScalerC(ipid, Global.ControlSystem));
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
                if (!(parentDev is IDmSwitch))
				{
					Debug.Console(0, "Cannot create DM device '{0}'. '{1}' is not a DM Chassis.",
						key, pKey);
					return null;
				}

                var chassis = (parentDev as IDmSwitch).Chassis;
				var num = props.ParentOutputNumber;

				if (num <= 0 || num > chassis.NumberOfOutputs)
				{
					Debug.Console(0, "Cannot create DM device '{0}'. Output number '{1}' is out of range",
						key, num);
					return null;
				}

			    var controller = (parentDev as IDmSwitch);
			    controller.RxDictionary.Add(num, key);
			    // Catch constructor failures, mainly dues to IPID
				try
				{

					// Must use different constructor for CPU3 chassis types. No IPID
					if (chassis is DmMd8x8Cpu3 || chassis is DmMd16x16Cpu3 ||
						chassis is DmMd32x32Cpu3 || chassis is DmMd8x8Cpu3rps ||
						chassis is DmMd16x16Cpu3rps || chassis is DmMd32x32Cpu3rps ||
                        chassis is DmMd128x128 || chassis is DmMd64x64)
					{
						if (typeName.StartsWith("hdbasetrx"))
							return new HDBaseTRxController(key, name, new HDRx3CB(chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc4k100c1g"))
							return new DmRmc4k100C1GController(key, name, new DmRmc4K100C1G(chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc100c"))
							return new DmRmcX100CController(key, name, new DmRmc100C(chassis.Outputs[num]));
                        if (typeName.StartsWith("dmrmc100s"))
                            return new DmRmc100SController(key, name, new DmRmc100S(chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc4k100c"))
							return new DmRmcX100CController(key, name, new DmRmc4k100C(chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc4kz100c"))
                            return new DmRmc4kZ100CController(key, name, new DmRmc4kz100C(chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc150s"))
							return new DmRmc150SController(key, name, new DmRmc150S(chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc200c"))
							return new DmRmc200CController(key, name, new DmRmc200C(chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc200s"))
							return new DmRmc200SController(key, name, new DmRmc200S(chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc200s2"))
							return new DmRmc200S2Controller(key, name, new DmRmc200S2(chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmcscalerc"))
							return new DmRmcScalerCController(key, name, new DmRmcScalerC(chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmcscalers"))
							return new DmRmcScalerSController(key, name, new DmRmcScalerS(chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmcscalers2"))
							return new DmRmcScalerS2Controller(key, name, new DmRmcScalerS2(chassis.Outputs[num]));
                        if (typeName.StartsWith("dmrmc4kscalerc"))
							return new DmRmc4kScalerCController(key, name, new DmRmc4kScalerC(chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc4kscalercdsp"))
							return new DmRmc4kScalerCDspController(key, name, new DmRmc4kScalerCDsp(chassis.Outputs[num]));
                        if (typeName.StartsWith("dmrmc4kzscalerc"))
                            return new DmRmc4kZScalerCController(key, name, new DmRmc4kzScalerC(chassis.Outputs[num]));
					}
					else
					{
						if (typeName.StartsWith("hdbasetrx"))
							return new HDBaseTRxController(key, name, new HDRx3CB(ipid, chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc4k100c1g"))
							return new DmRmc4k100C1GController(key, name, new DmRmc4K100C1G(ipid, chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc100c"))
							return new DmRmcX100CController(key, name, new DmRmc100C(ipid, chassis.Outputs[num]));
                        if (typeName.StartsWith("dmrmc100s"))
                            return new DmRmc100SController(key, name, new DmRmc100S(ipid, chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc4k100c"))
							return new DmRmcX100CController(key, name, new DmRmc4k100C(ipid, chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc4kz100c"))
                            return new DmRmc4kZ100CController(key, name, new DmRmc4kz100C(ipid, chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc150s"))
							return new DmRmc150SController(key, name, new DmRmc150S(ipid, chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc200c"))
							return new DmRmc200CController(key, name, new DmRmc200C(ipid, chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc200s"))
							return new DmRmc200SController(key, name, new DmRmc200S(ipid, chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc200s2"))
							return new DmRmc200S2Controller(key, name, new DmRmc200S2(ipid, chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmcscalerc"))
							return new DmRmcScalerCController(key, name, new DmRmcScalerC(ipid, chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmcscalers"))
							return new DmRmcScalerSController(key, name, new DmRmcScalerS(ipid, chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmcscalers2"))
							return new DmRmcScalerS2Controller(key, name, new DmRmcScalerS2(ipid, chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc4kscalerc"))
							return new DmRmc4kScalerCController(key, name, new DmRmc4kScalerC(ipid, chassis.Outputs[num]));
						if (typeName.StartsWith("dmrmc4kscalercdsp"))
							return new DmRmc4kScalerCDspController(key, name, new DmRmc4kScalerCDsp(ipid, chassis.Outputs[num]));
                        if (typeName.StartsWith("dmrmc4kzscalerc"))
                            return new DmRmc4kZScalerCController(key, name, new DmRmc4kzScalerC(chassis.Outputs[num]));

					}
				}
				catch (Exception e)
				{
					Debug.Console(0, "[{0}] WARNING: Cannot create DM-RMC device: {1}", key, e.Message);
				}
			}

			return null;
		}
	}

    public class DmRmcControllerFactory : EssentialsDeviceFactory<DmRmcControllerBase>
    {
        public DmRmcControllerFactory()
        {
            TypeNames = new List<string>
            { "hdbasetrx", "dmrmc4k100c1g", "dmrmc100c", "dmrmc100s", "dmrmc4k100c", "dmrmc150s",
                "dmrmc200c", "dmrmc200s", "dmrmc200s2", "dmrmcscalerc", "dmrmcscalers", "dmrmcscalers2", "dmrmc4kscalerc", "dmrmc4kscalercdsp",
                "dmrmc4kz100c", "dmrmckzscalerc" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var type = dc.Type.ToLower();

            Debug.Console(1, "Factory Attempting to create new DM-RMC Device");

            var props = JsonConvert.DeserializeObject
                <DmRmcPropertiesConfig>(dc.Properties.ToString());
            return DmRmcHelper.GetDmRmcController(dc.Key, dc.Name, type, props);
            
        }
    }

}