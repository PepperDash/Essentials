extern alias Full;

using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Full.Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.DeviceInfo;
using PepperDash.Essentials.DM.Config;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.DM
{
    [Description("Wrapper class for all DM-RMC variants")]
	public abstract class DmRmcControllerBase : CrestronGenericBridgeableBaseDevice, IDeviceInfoProvider
    {
        private const int CtpPort = 41795;
        private readonly EndpointReceiverBase _rmc; //kept here just in case. Only property or method on this class that's not device-specific is the DMOutput that it's attached to.

        public StringFeedback VideoOutputResolutionFeedback { get; protected set; }
        public StringFeedback EdidManufacturerFeedback { get; protected set; }
        public StringFeedback EdidNameFeedback { get; protected set; }
        public StringFeedback EdidPreferredTimingFeedback { get; protected set; }
        public StringFeedback EdidSerialNumberFeedback { get; protected set; }

        protected DmRmcControllerBase(string key, string name, EndpointReceiverBase device)
			: base(key, name, device)
        {
            _rmc = device;

			// if wired to a chassis, skip registration step in base class
            PreventRegistration = _rmc.DMOutput != null;
			
            AddToFeedbackList(VideoOutputResolutionFeedback, EdidManufacturerFeedback, EdidSerialNumberFeedback, EdidNameFeedback, EdidPreferredTimingFeedback);
            
            DeviceInfo = new DeviceInfo();

            IsOnline.OutputChange += (currentDevice, args) => { if (args.BoolValue) UpdateDeviceInfo(); };
        }

        protected void LinkDmRmcToApi(DmRmcControllerBase rmc, BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DmRmcControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmRmcControllerJoinMap>(joinMapSerialized);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            Debug.Console(1, rmc, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
            trilist.StringInput[joinMap.Name.JoinNumber].StringValue = rmc.Name;
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

        #region Implementation of IDeviceInfoProvider

        public DeviceInfo DeviceInfo { get; private set; }
        public event DeviceInfoChangeHandler DeviceInfoChanged;

        public void UpdateDeviceInfo()
        {
            Debug.Console(1, this, "Updating Device Info");

            if (_rmc.ConnectedIpList.Count == 0)
            {
                Debug.Console(1, this, "IP Address information not yet received. No device is online");
                return;
            }

            DeviceInfo.IpAddress = _rmc.ConnectedIpList[0].DeviceIpAddress;

            foreach (var ip in _rmc.ConnectedIpList)
            {
                Debug.Console(0, this, "Connected IP Address: {0}", ip.DeviceIpAddress);
            }

            GetFirmwareAndSerialInfo();

            OnDeviceInfoChange();
        }

        private void GetFirmwareAndSerialInfo()
        {
            var tcpClient = new GenericTcpIpClient(String.Format("{0}-devInfoSocket", Key), _rmc.ConnectedIpList[0].DeviceIpAddress, CtpPort, 1024)
            {
                AutoReconnect = false,
            };

            var gather = new CommunicationGather(tcpClient, "\r\n\r\n");

            tcpClient.ConnectionChange += (sender, args) =>
            {
                if (!args.Client.IsConnected)
                {
                    OnDeviceInfoChange();
                    return;
                }

                args.Client.SendText("ver\r\n");
            };

            gather.LineReceived += (sender, args) =>
            {
                //ignore console prompt
                if (args.Text.ToLower().Contains(">"))
                {
                    return;
                }

                
                if (args.Text.ToLower().Contains("host"))
                {
                    DeviceInfo.HostName = args.Text.Split(':')[1].Trim();

                    tcpClient.SendText("maca\r\n");
                    
                    return;
                }

                if (args.Text.ToLower().Contains("mac"))
                {
                    DeviceInfo.MacAddress = args.Text.Split(':')[1].Trim().Replace(" ", ":");

                    tcpClient.Disconnect();

                    return;
                }

                if (!args.Text.ToLower().Contains("rmc"))
                {
                    return;
                }

                DeviceInfo.SerialNumber = args.Text.Split('[')[1].Split(' ')[4].Replace("#", "");
                DeviceInfo.FirmwareVersion = args.Text.Split('[')[1].Split(' ')[1];

                tcpClient.SendText("host\r\n");
            };

            tcpClient.Connect();
        }

        private void OnDeviceInfoChange()
        {
            var handler = DeviceInfoChanged;

            if (handler == null) return;

            handler(this, new DeviceInfoEventArgs(DeviceInfo));
        }

        #endregion
    }

    public abstract class DmHdBaseTControllerBase : CrestronGenericBridgeableBaseDevice
    {
        protected HDBaseTBase Rmc;

        /// <summary>
        ///  Make a Crestron RMC and put it in here
        /// </summary>
        protected DmHdBaseTControllerBase(string key, string name, HDBaseTBase rmc)
            : base(key, name, rmc)
        {
            Rmc = rmc;
        }
    }

	public class DmRmcHelper
	{
	    private static readonly Dictionary<string, Func<string, string, uint, CrestronGenericBaseDevice>> ProcessorFactoryDict;
	    private static readonly Dictionary<string, Func<string, string, DMOutput, CrestronGenericBaseDevice>> ChassisCpu3Dict;

	    private static readonly Dictionary<string, Func<string, string, uint, DMOutput, CrestronGenericBaseDevice>>
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

            ChassisCpu3Dict = new Dictionary<string, Func<string, string, DMOutput, CrestronGenericBaseDevice>>
	        {
	            {"dmrmc100c", (k, n, d) => new DmRmcX100CController(k, n, new DmRmc100C(d))},
	            {"dmrmc100s", (k, n, d) => new DmRmc100SController(k, n, new DmRmc100S(d))},
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
	        };

            ChassisDict = new Dictionary<string, Func<string, string, uint, DMOutput, CrestronGenericBaseDevice>>
	        {
	            {"dmrmc100c", (k, n, i, d) => new DmRmcX100CController(k, n, new DmRmc100C(i,d))},
	            {"dmrmc100s", (k, n,i, d) => new DmRmc100SController(k, n, new DmRmc100S(i,d))},
	            {"dmrmc4k100c", (k, n,i, d) => new DmRmcX100CController(k, n, new DmRmc4k100C(i,d))},
	            {"dmrmc4kz100c", (k, n,i, d) => new DmRmc4kZ100CController(k, n, new DmRmc4kz100C(i,d))},
	            {"dmrmc150s", (k, n,i, d) => new DmRmc150SController(k, n, new DmRmc150S(i,d))},
	            {"dmrmc200c", (k, n,i, d) => new DmRmc200CController(k, n, new DmRmc200C(i,d))},
	            {"dmrmc200s", (k, n,i, d) => new DmRmc200SController(k, n, new DmRmc200S(i,d))},
	            {"dmrmc200s2", (k, n,i, d) => new DmRmc200S2Controller(k, n, new DmRmc200S2(i,d))},
	            {"dmrmcscalerc", (k, n,i, d) => new DmRmcScalerCController(k, n, new DmRmcScalerC(i,d))},
	            {"dmrmcscalers", (k, n,i, d) => new DmRmcScalerSController(k, n, new DmRmcScalerS(i,d))},
	            {
	                "dmrmcscalers2",
	                (k, n,i, d) => new DmRmcScalerS2Controller(k, n, new DmRmcScalerS2(i, d))
	            },
	            {
	                "dmrmc4kscalerc",
	                (k, n,i, d) => new DmRmc4kScalerCController(k, n, new DmRmc4kScalerC(i, d))
	            },
	            {
	                "dmrmc4kscalercdsp",
	                (k, n,i, d) => new DmRmc4kScalerCDspController(k, n, new DmRmc4kScalerCDsp(i, d))
	            },
	            {
	                "dmrmc4kzscalerc",
	                (k, n,i, d) => new DmRmc4kZScalerCController(k, n, new DmRmc4kzScalerC(i, d))
	            },
                {"hdbasetrx", (k,n,i,d) => new HDBaseTRxController(k,n, new HDRx3CB(i, d))},
                {"dmrmc4k100c1g", (k,n,i,d) => new DmRmc4k100C1GController(k,n, new DmRmc4K100C1G(i, d))}
	        };
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
			return pKey == "processor" ? GetDmRmcControllerForProcessor(key, name, typeName, ipid) : GetDmRmcControllerForChassis(key, name, typeName, props, pKey, ipid);
		}

	    private static CrestronGenericBaseDevice GetDmRmcControllerForChassis(string key, string name, string typeName,
	        DmRmcPropertiesConfig props, string pKey, uint ipid)
	    {
	        var parentDev = DeviceManager.GetDeviceForKey(pKey);
	        if (parentDev is DmpsRoutingController)
	        {
                var dmps = parentDev as DmpsRoutingController;
                //Check that the input is within range of this chassis' possible inputs
                var num = props.ParentOutputNumber;
                Debug.Console(1, "Creating DMPS device '{0}'. Output number '{1}'.", key, num);
                if (num <= 0 || num > dmps.Dmps.SwitcherOutputs.Count)
                {
                    Debug.Console(0, "Cannot create DMPS device '{0}'. Output number '{1}' is out of range",
                        key, num);
                    return null;
                }
                // Must use different constructor for DMPS4K types. No IPID
                if (Global.ControlSystemIsDmps4kType || typeName == "hdbasetrx" || typeName == "dmrmc4k100c1g")
                {
                    var rmc = GetDmRmcControllerForDmps4k(key, name, typeName, dmps, props.ParentOutputNumber);
                    Debug.Console(0, "DM endpoint output {0} is for Dmps4k, changing online feedback to chassis", num);
                    rmc.IsOnline.SetValueFunc(() => dmps.OutputEndpointOnlineFeedbacks[num].BoolValue);
                    dmps.OutputEndpointOnlineFeedbacks[num].OutputChange += (o, a) =>
                    {
                        foreach (var feedback in rmc.Feedbacks)
                        {
                            if (feedback != null)
                                feedback.FireUpdate();
                        }
                    };
                    return rmc;
                }
                return GetDmRmcControllerForDmps(key, name, typeName, ipid, dmps, props.ParentOutputNumber);
	        }
            else if (parentDev is DmChassisController)
            {
                var controller = parentDev as DmChassisController;
                var chassis = controller.Chassis;
                var num = props.ParentOutputNumber;
                Debug.Console(1, "Creating DM Chassis device '{0}'. Output number '{1}'.", key, num);

                if (num <= 0 || num > chassis.NumberOfOutputs)
                {
                    Debug.Console(0, "Cannot create DM device '{0}'. Output number '{1}' is out of range",
                        key, num);
                    return null;
                }                
                controller.RxDictionary.Add(num, key);
                // Catch constructor failures, mainly dues to IPID
                try
                {
                    // Must use different constructor for CPU3 chassis types. No IPID
                    if (chassis is DmMd8x8Cpu3 || chassis is DmMd16x16Cpu3 ||
                        chassis is DmMd32x32Cpu3 || chassis is DmMd8x8Cpu3rps ||
                        chassis is DmMd16x16Cpu3rps || chassis is DmMd32x32Cpu3rps ||
                        chassis is DmMd128x128 || chassis is DmMd64x64
                        || typeName == "hdbasetrx" || typeName == "dmrmc4k100c1g")
                    {
                        var rmc = GetDmRmcControllerForCpu3Chassis(key, name, typeName, chassis, num, parentDev);
                        Debug.Console(0, "DM endpoint output {0} is for Cpu3, changing online feedback to chassis", num);
                        rmc.IsOnline.SetValueFunc(() => controller.OutputEndpointOnlineFeedbacks[num].BoolValue);
                        controller.OutputEndpointOnlineFeedbacks[num].OutputChange += (o, a) =>
                            {
                                foreach (var feedback in rmc.Feedbacks)
                                {
                                    if (feedback != null)
                                        feedback.FireUpdate();
                                }
                            };
                        return rmc;
                    }
                    return GetDmRmcControllerForCpu2Chassis(key, name, typeName, ipid, chassis, num, parentDev);
                }
                catch (Exception e)
                {
                    Debug.Console(0, "[{0}] WARNING: Cannot create DM-RMC device: {1}", key, e.Message);
                    return null;
                }
            }
            else
            {
                Debug.Console(0, "Cannot create DM device '{0}'. '{1}' is not a DM Chassis or DMPS.",
                    key, pKey);
                return null;
            }
	    }

	    private static CrestronGenericBaseDevice GetDmRmcControllerForCpu2Chassis(string key, string name, string typeName,
	        uint ipid, Switch chassis, uint num, IKeyed parentDev)
	    {
	        Func<string, string, uint, DMOutput, CrestronGenericBaseDevice> handler;
	        if (ChassisDict.TryGetValue(typeName.ToLower(), out handler))
	        {
	            return handler(key, name, ipid, chassis.Outputs[num]);
	        }
	        Debug.Console(0, "Cannot create DM-RMC of type '{0}' with parent device {1}", typeName, parentDev.Key);
	        return null;
	    }

	    private static CrestronGenericBaseDevice GetDmRmcControllerForCpu3Chassis(string key, string name, string typeName,
	        Switch chassis, uint num, IKeyed parentDev)
	    {
	        Func<string, string, DMOutput, CrestronGenericBaseDevice> cpu3Handler;
	        if (ChassisCpu3Dict.TryGetValue(typeName.ToLower(), out cpu3Handler))
	        {
	            return cpu3Handler(key, name, chassis.Outputs[num]);
	        }
	        Debug.Console(0, "Cannot create DM-RMC of type '{0}' with parent device {1}", typeName, parentDev.Key);
	        return null;
	    }

        private static CrestronGenericBaseDevice GetDmRmcControllerForDmps(string key, string name, string typeName,
            uint ipid, DmpsRoutingController controller, uint num)
        {
            Func<string, string, uint, DMOutput, CrestronGenericBaseDevice> dmpsHandler;
            if (ChassisDict.TryGetValue(typeName.ToLower(), out dmpsHandler))
            {
                var output = controller.Dmps.SwitcherOutputs[num] as DMOutput;

                if (output != null)
                {
                    return dmpsHandler(key, name, ipid, output);
                }
                Debug.Console(0, Debug.ErrorLogLevel.Error,
                    "Cannot attach DM-RMC of type '{0}' to output {1} on DMPS chassis. Output is not a DM Output.",
                    typeName, num);
                return null;
            }

            Debug.Console(0, Debug.ErrorLogLevel.Error, "Cannot create DM-RMC of type '{0}' to output {1} on DMPS chassis", typeName, num);
            return null;
        }

	    private static CrestronGenericBaseDevice GetDmRmcControllerForDmps4k(string key, string name, string typeName,
	        DmpsRoutingController controller, uint num)
	    {
	        Func<string, string, DMOutput, CrestronGenericBaseDevice> dmps4kHandler;
	        if (ChassisCpu3Dict.TryGetValue(typeName.ToLower(), out dmps4kHandler))
	        {
	            var output = controller.Dmps.SwitcherOutputs[num] as DMOutput;

	            if (output != null)
	            {
	                return dmps4kHandler(key, name, output);
	            }
	            Debug.Console(0, Debug.ErrorLogLevel.Error,
	                "Cannot attach DM-RMC of type '{0}' to output {1} on DMPS-4K chassis. Output is not a DM Output.",
	                typeName, num);
	            return null;
	        }

            Debug.Console(0, Debug.ErrorLogLevel.Error, "Cannot create DM-RMC of type '{0}' to output {1} on DMPS-4K chassis", typeName, num);
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

        
	}

    public class DmRmcControllerFactory : EssentialsDeviceFactory<DmRmcControllerBase>
    {
        public DmRmcControllerFactory()
        {
            TypeNames = new List<string>
            { "hdbasetrx", "dmrmc4k100c1g", "dmrmc100c", "dmrmc100s", "dmrmc4k100c", "dmrmc150s",
                "dmrmc200c", "dmrmc200s", "dmrmc200s2", "dmrmcscalerc", "dmrmcscalers", "dmrmcscalers2", "dmrmc4kscalerc", "dmrmc4kscalercdsp",
                "dmrmc4kz100c", "dmrmc4kzscalerc" };
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