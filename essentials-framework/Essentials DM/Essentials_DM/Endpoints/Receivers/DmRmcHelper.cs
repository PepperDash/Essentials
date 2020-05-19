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
        private EndpointReceiverBase _rmc; //kept here just in case. Only property or method on this class that's not device-specific is the DMOutput that it's attached to.

        public StringFeedback VideoOutputResolutionFeedback { get; protected set; }
        public StringFeedback EdidManufacturerFeedback { get; protected set; }
        public StringFeedback EdidNameFeedback { get; protected set; }
        public StringFeedback EdidPreferredTimingFeedback { get; protected set; }
        public StringFeedback EdidSerialNumberFeedback { get; protected set; }

        protected DmRmcControllerBase(string key, string name, EndpointReceiverBase device)
			: base(key, name, device)
        {
            SetRmc(device);
        }

        protected DmRmcControllerBase(string key, string name):base(key, name)
        {
            
        }

        protected void SetRmc(EndpointReceiverBase rmc)
        {
            _rmc = rmc;
            // if wired to a chassis, skip registration step in base class
            PreventRegistration = _rmc.DMOutput != null;

            AddToFeedbackList(VideoOutputResolutionFeedback, EdidManufacturerFeedback, EdidSerialNumberFeedback, EdidNameFeedback, EdidPreferredTimingFeedback);
        }

        protected void LinkDmRmcToApi(DmRmcControllerBase rmc, BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DmRmcControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmRmcControllerJoinMap>(joinMapSerialized);

            bridge.AddJoinMap(Key, joinMap);

            Debug.Console(1, rmc, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            rmc.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
            if (rmc.VideoOutputResolutionFeedback != null)
                rmc.VideoOutputResolutionFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentOutputResolution.JoinNumber]);
            if (rmc.EdidManufacturerFeedback != null)
                rmc.EdidManufacturerFeedback.LinkInputSig(trilist.StringInput[joinMap.EdidManufacturer.JoinNumber]);
            if (rmc.EdidNameFeedback != null)
                rmc.EdidNameFeedback.LinkInputSig(trilist.StringInput[joinMap.EdidName.JoinNumber]);
            if (rmc.EdidPreferredTimingFeedback != null)
                rmc.EdidPreferredTimingFeedback.LinkInputSig(trilist.StringInput[joinMap.EdidPrefferedTiming.JoinNumber]);
            if (rmc.EdidSerialNumberFeedback != null)
                rmc.EdidSerialNumberFeedback.LinkInputSig(trilist.StringInput[joinMap.EdidSerialNumber.JoinNumber]);
            
            //If the device is an DM-RMC-4K-Z-SCALER-C
            var routing = rmc as IRmcRouting;

            if (routing == null)
            {
                return;
            }

            if (routing.AudioVideoSourceNumericFeedback != null)
                routing.AudioVideoSourceNumericFeedback.LinkInputSig(trilist.UShortInput[joinMap.AudioVideoSource.JoinNumber]);

            trilist.SetUShortSigAction(joinMap.AudioVideoSource.JoinNumber, a => routing.ExecuteNumericSwitch(a, 1, eRoutingSignalType.AudioVideo));
        }
	}

    public abstract class DmHdBaseTControllerBase : CrestronGenericBaseDevice
    {
        protected HDBaseTBase Rmc;

        /// <summary>
        ///  Make a Crestron RMC and put it in here
        /// </summary>
        protected DmHdBaseTControllerBase(string key, string name, HDBaseTBase rmc)
            : base(key, name, rmc)
        {
            SetRmc(rmc);
        }

        protected DmHdBaseTControllerBase(string key, string name) : base(key, name)
        {
            
        }

        protected void SetRmc(HDBaseTBase rmc)
        {
            Rmc = rmc;
        }
    }

	public class DmRmcHelper
	{
	    private static readonly Dictionary<string, Func<string, string, uint, CrestronGenericBaseDevice>> ProcessorFactoryDict;

	    private static readonly
	        Dictionary
	            <string,
	                Func<string, string, DmRmcPropertiesConfig, Func<string, string, IDmSwitch>, CrestronGenericBaseDevice>>
	        ChassisDict; 

	    static DmRmcHelper()
	    {
	        ProcessorFactoryDict = new Dictionary<string, Func<string, string, uint, CrestronGenericBaseDevice>>
	        {
	            {"dmrmc100c", (k, n, i) => new DmRmcX100CController(k, n, new DmRmc100C(i, Global.ControlSystem))},
	            {"dmrmc100s", (k, n, i) => new DmRmc100SController(k, n, new DmRmc100S(i, Global.ControlSystem))},
	            {"dmrmc4k100c", (k, n, i) => new DmRmcX100CController(k, n, new DmRmc4k100C(i, Global.ControlSystem))},
	            {"dmrmc4kz100c", (k, n, i) => new DmRmc4kZ100CController(k, n, new DmRmc4kz100C(i, Global.ControlSystem))},
	            {"dmrmc150s", (k, n, i) => new DmRmc150SController(k, n, new DmRmc150S(i, Global.ControlSystem))},
	            {"dmrmc200c", (k, n, i) => new DmRmc200CController(k, n, new DmRmc200C(i, Global.ControlSystem))},
	            {"dmrmc200s", (k, n, i) => new DmRmc200SController(k, n, new DmRmc200S(i, Global.ControlSystem))},
	            {"dmrmc200s2", (k, n, i) => new DmRmc200S2Controller(k, n, new DmRmc200S2(i, Global.ControlSystem))},
	            {"dmrmcscalerc", (k, n, i) => new DmRmcScalerCController(k, n, new DmRmcScalerC(i, Global.ControlSystem))},
	            {"dmrmcscalers", (k, n, i) => new DmRmcScalerSController(k, n, new DmRmcScalerS(i, Global.ControlSystem))},
	            {
	                "dmrmcscalers2",
	                (k, n, i) => new DmRmcScalerS2Controller(k, n, new DmRmcScalerS2(i, Global.ControlSystem))
	            },
	            {
	                "dmrmc4kscalerc",
	                (k, n, i) => new DmRmc4kScalerCController(k, n, new DmRmc4kScalerC(i, Global.ControlSystem))
	            },
	            {
	                "dmrmc4kscalercdsp",
	                (k, n, i) => new DmRmc4kScalerCDspController(k, n, new DmRmc4kScalerCDsp(i, Global.ControlSystem))
	            },
	            {
	                "dmrmc4kzscalerc",
	                (k, n, i) => new DmRmc4kZScalerCController(k, n, new DmRmc4kzScalerC(i, Global.ControlSystem))
	            }
	        };

            /*ChassisCpu3Dict = new Dictionary<string, Func<string, string, DMOutput, CrestronGenericBaseDevice>>
	        {
	            {"dmrmc100c", (k, n, d) => new DmRmcX100CController(k, n, new DmRmc100C(d))},
	            {"dmrmc100s", (k, n, d) => new DmRmc100SController(k, n, d)},
	            {"dmrmc4k100c", (k, n, d) => new DmRmcX100CController(k, n, new DmRmc4k100C(d))},
	            {"dmrmc4kz100c", (k, n, d) => new DmRmc4kZ100CController(k, n, new DmRmc4kz100C(d))},
	            {"dmrmc150s", (k, n, d) => new DmRmc150SController(k, n, new DmRmc150S(d))},
	            {"dmrmc200c", (k, n, d) => new DmRmc200CController(k, n, new DmRmc200C(d))},
	            {"dmrmc200s", (k, n, d) => new DmRmc200SController(k, n, new DmRmc200S(d))},
	            {"dmrmc200s2", (k, n, d) => new DmRmc200S2Controller(k, n, new DmRmc200S2(d))},
	            {"dmrmcscalerc", (k, n, d) => new DmRmcScalerCController(k, n, new DmRmcScalerC(d))},
	            {"dmrmcscalers", (k, n, d) => new DmRmcScalerSController(k, n, new DmRmcScalerS(d))},
	            {
	                "dmrmcscalers2",
	                (k, n, d) => new DmRmcScalerS2Controller(k, n, new DmRmcScalerS2(d))
	            },
	            {
	                "dmrmc4kscalerc",
	                (k, n, d) => new DmRmc4kScalerCController(k, n, new DmRmc4kScalerC(d))
	            },
	            {
	                "dmrmc4kscalercdsp",
	                (k, n, d) => new DmRmc4kScalerCDspController(k, n, new DmRmc4kScalerCDsp(d))
	            },
	            {
	                "dmrmc4kzscalerc",
	                (k, n, d) => new DmRmc4kZScalerCController(k, n, new DmRmc4kzScalerC(d))
	            },
                {"hdbasetrx", (k,n,d) => new HDBaseTRxController(k,n, new HDRx3CB(d))},
                {"dmrmc4k100c1g", (k,n,d) => new DmRmc4k100C1GController(k,n, new DmRmc4K100C1G(d))}
	        };*/

	        ChassisDict = new Dictionary
	            <string,
	                Func<string, string, DmRmcPropertiesConfig, Func<string, string, IDmSwitch>, CrestronGenericBaseDevice>
	                >
	        {
	            {"dmrmc100s", (k, n, p, f) => new DmRmc100SController(k, n, p, f)}
	        };

	        /*ChassisDict = new Dictionary<string, Func<string, string, uint, DMOutput, CrestronGenericBaseDevice>>
	        {
	            {"dmrmc100c", (k, n, i, d) => new DmRmcX100CController(k, n, new DmRmc100C(i, d))},
	            {"dmrmc100s", (k, n, i, d) => new DmRmc100SController(k, n, i, d)},
	            {"dmrmc4k100c", (k, n, i, d) => new DmRmcX100CController(k, n, new DmRmc4k100C(i, d))},
	            {"dmrmc4kz100c", (k, n, i, d) => new DmRmc4kZ100CController(k, n, new DmRmc4kz100C(i, d))},
	            {"dmrmc150s", (k, n, i, d) => new DmRmc150SController(k, n, new DmRmc150S(i, d))},
	            {"dmrmc200c", (k, n, i, d) => new DmRmc200CController(k, n, new DmRmc200C(i, d))},
	            {"dmrmc200s", (k, n, i, d) => new DmRmc200SController(k, n, new DmRmc200S(i, d))},
	            {"dmrmc200s2", (k, n, i, d) => new DmRmc200S2Controller(k, n, new DmRmc200S2(i, d))},
	            {"dmrmcscalerc", (k, n, i, d) => new DmRmcScalerCController(k, n, new DmRmcScalerC(i, d))},
	            {"dmrmcscalers", (k, n, i, d) => new DmRmcScalerSController(k, n, new DmRmcScalerS(i, d))},
	            {
	                "dmrmcscalers2",
	                (k, n, i, d) => new DmRmcScalerS2Controller(k, n, new DmRmcScalerS2(d))
	            },
	            {
	                "dmrmc4kscalerc",
	                (k, n, i, d) => new DmRmc4kScalerCController(k, n, new DmRmc4kScalerC(d))
	            },
	            {
	                "dmrmc4kscalercdsp",
	                (k, n, i, d) => new DmRmc4kScalerCDspController(k, n, new DmRmc4kScalerCDsp(d))
	            },
	            {
	                "dmrmc4kzscalerc",
	                (k, n, i, d) => new DmRmc4kZScalerCController(k, n, new DmRmc4kzScalerC(d))
	            },
	            {"hdbasetrx", (k, n, i, d) => new HDBaseTRxController(k, n, new HDRx3CB(d))},
	            {"dmrmc4k100c1g", (k, n, i, d) => new DmRmc4k100C1GController(k, n, new DmRmc4K100C1G(d))}
	        };*/
	    }
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
			typeName = typeName.ToLower();
			var ipid = props.Control.IpIdInt;

			var pKey = props.ParentDeviceKey.ToLower();

			// Non-DM-chassis endpoints
			return pKey == "processor" ? GetDmRmcControllerForProcessor(key, name, typeName, ipid) : GetDmRmcControllerForChassis(key, name, typeName, props);
		}

	    private static CrestronGenericBaseDevice GetDmRmcControllerForChassis(string key, string name, string typeName,
	        DmRmcPropertiesConfig props)
	    {
	        Func<string, string, DmRmcPropertiesConfig, Func<string, string, IDmSwitch>, CrestronGenericBaseDevice> handler;

	        if (ChassisDict.TryGetValue(typeName.ToLower(), out handler))
	        {
	            return handler(key, name, props, GetDmSwitch);
	        }

	        Debug.Console(0, "Cannot create DM-RMC of type '{0}' with parent device {1}", typeName, props.ParentDeviceKey);
	        return null;
	    }

	    private static CrestronGenericBaseDevice GetDmRmcControllerForProcessor(string key, string name, string typeName, uint ipid)
	    {
	        try
	        {
	            Func<string, string, uint, CrestronGenericBaseDevice> handler;

	            if (ProcessorFactoryDict.TryGetValue(typeName.ToLower(), out handler))
	            {
	                return handler(key, name, ipid);
	            }
	            Debug.Console(0, "Cannot create DM-RMC of type: '{0}'", typeName);

	            return null;
	        }
	        catch (Exception e)
	        {
	            Debug.Console(0, "[{0}] WARNING: Cannot create DM-RMC device: {1}", key, e.Message);
                return null;
	        }
	    }

	    public static IDmSwitch GetDmSwitch(string key, string pKey)
	    {
            var parentDev = DeviceManager.GetDeviceForKey(pKey) as IDmSwitch;

	        if (parentDev != null)
	        {
	            return parentDev;
	        }

	        Debug.Console(0, "Cannot create DM device '{0}'. '{1}' is not a DM Chassis.",
	            key, pKey);
	        return null;
	    }

	    public static DMOutput AddRmcAndGetDmOutput(string key, uint output, IDmSwitch dmSwitch)
	    {
	        var chassis = dmSwitch.Chassis;

            if (output <= 0 || output > chassis.NumberOfOutputs)
            {
                Debug.Console(0, "Cannot add DM device '{0}' to switch '{2}'. Output number '{1}' is out of range",
                    key, output, dmSwitch);
            }
            dmSwitch.RxDictionary.Add(output, key);

	        return dmSwitch.Chassis.Outputs[output];
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