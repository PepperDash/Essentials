using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.EthernetCommunication;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Lighting;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.DM;
//using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.Bridges
{
    /// <summary>
    /// Base class for all bridge class variants
    /// </summary>
    public class BridgeBase : Device
    {
        public BridgeApi Api { get; private set; }

        public BridgeBase(string key) :
            base(key)
        {

        }
            
    }

    /// <summary>
    /// Base class for bridge API variants
    /// </summary>
    public abstract class BridgeApi : Device
    {
        public BridgeApi(string key) :
            base(key)
        {

        }

    }

    /// <summary>
    /// Bridge API using EISC
    /// </summary>
    public class EiscApi : BridgeApi
    {
        public EiscApiPropertiesConfig PropertiesConfig { get; private set; }

        public Dictionary<string, JoinMapBase> JoinMaps { get; set; }

        public ThreeSeriesTcpIpEthernetIntersystemCommunications Eisc { get; private set; }

        public EiscApi(DeviceConfig dc) :
            base(dc.Key)
        {
            PropertiesConfig = JsonConvert.DeserializeObject<EiscApiPropertiesConfig>(dc.Properties.ToString());

            Eisc = new ThreeSeriesTcpIpEthernetIntersystemCommunications(PropertiesConfig.Control.IpIdInt, PropertiesConfig.Control.TcpSshProperties.Address, Global.ControlSystem);

            Eisc.SigChange += new Crestron.SimplSharpPro.DeviceSupport.SigEventHandler(Eisc_SigChange);

            Eisc.Register();

            AddPostActivationAction( () =>
            {
                Debug.Console(1, this, "Linking Devices...");

                foreach (var d in PropertiesConfig.Devices)
                {
                    var device = DeviceManager.GetDeviceForKey(d.DeviceKey);

                    if (device != null)
                    {
                        if (device is IBridge)      // Check for this first to allow bridges in plugins to override existing bridges that apply to the same type.
                        {
                            (device as IBridge).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is PepperDash.Essentials.Core.Monitoring.SystemMonitorController)
                        {
                            (device as PepperDash.Essentials.Core.Monitoring.SystemMonitorController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is GenericComm)
                        {
                            (device as GenericComm).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is CameraBase)
                        {
                            (device as CameraBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
						else if (device is PepperDash.Essentials.Core.DisplayBase)
						{
							(device as DisplayBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
							continue;
						}
                        else if (device is DmChassisController) {
                            (device as DmChassisController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is DmBladeChassisController) {
                            (device as DmBladeChassisController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is DmpsRoutingController)
                        {
                            (device as DmpsRoutingController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is DmpsAudioOutputController)
                        {
                            (device as DmpsAudioOutputController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is DmTxControllerBase)
                        {
                            (device as DmTxControllerBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is DmRmcControllerBase)
                        {
                            (device as DmRmcControllerBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is GenericRelayDevice)
                        {
                            (device as GenericRelayDevice).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is IRSetTopBoxBase)
                        {
                            (device as IRSetTopBoxBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is IDigitalInput)
                        {
                            (device as IDigitalInput).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is AppleTV)
                        {
                            (device as AppleTV).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is HdMdxxxCEController)
                        {
                            (device as HdMdxxxCEController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is LightingBase)
                        {
                            (device as LightingBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is DigitalLogger)
                        {
                            (device as DigitalLogger).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is PepperDash.Essentials.Devices.Common.Occupancy.GlsOccupancySensorBaseController)
                        {
                            (device as PepperDash.Essentials.Devices.Common.Occupancy.GlsOccupancySensorBaseController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is StatusSignController)
                        {
                            (device as StatusSignController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                        else if (device is C2nRthsController)
                        {
                            (device as C2nRthsController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
                    }
                }

                Debug.Console(1, this, "Devices Linked.");
            });
        }

        /// <summary>
        /// Used for debugging to trigger an action based on a join number and type
        /// </summary>
        /// <param name="join"></param>
        /// <param name="type"></param>
        public void ExecuteJoinAction(uint join, string type, object state)
        {
            try
            {
                switch (type.ToLower())
                {
                    case "digital":
                        {
                            var uo = Eisc.BooleanOutput[join].UserObject as Action<bool>;
                            if (uo != null)
                            {
                                Debug.Console(1, this, "Executing Action: {0}", uo.ToString());
                                uo(Convert.ToBoolean(state));
                            }
                            else
                                Debug.Console(1, this, "User Action is null.  Nothing to Execute");
                            break;
                        }
                    case "analog":
                        {
                            var uo = Eisc.BooleanOutput[join].UserObject as Action<ushort>;
                            if (uo != null)
                            {
                                Debug.Console(1, this, "Executing Action: {0}", uo.ToString());
                                uo(Convert.ToUInt16(state));
                            }
                            else
                                Debug.Console(1, this, "User Action is null.  Nothing to Execute"); break;
                        }
                    case "serial":
                        {
                            var uo = Eisc.BooleanOutput[join].UserObject as Action<string>;
                            if (uo != null)
                            {
                                Debug.Console(1, this, "Executing Action: {0}", uo.ToString());
                                uo(Convert.ToString(state));
                            }
                            else
                                Debug.Console(1, this, "User Action is null.  Nothing to Execute");
                            break;
                        }
                    default:
                        {
                            Debug.Console(1, "Unknown join type.  Use digital/serial/analog");
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error: {0}", e);
            }

        }

        /// <summary>
        /// Handles incoming sig changes
        /// </summary>
        /// <param name="currentDevice"></param>
        /// <param name="args"></param>
        void Eisc_SigChange(object currentDevice, Crestron.SimplSharpPro.SigEventArgs args)
        {
            try
            {
                if (Debug.Level >= 1)
                    Debug.Console(1, this, "EiscApi change: {0} {1}={2}", args.Sig.Type, args.Sig.Number, args.Sig.StringValue);
                var uo = args.Sig.UserObject;
                if (uo != null)
                {
                    Debug.Console(1, this, "Executing Action: {0}", uo.ToString());
                    if (uo is Action<bool>)
                        (uo as Action<bool>)(args.Sig.BoolValue);
                    else if (uo is Action<ushort>)
                        (uo as Action<ushort>)(args.Sig.UShortValue);
                    else if (uo is Action<string>)
                        (uo as Action<string>)(args.Sig.StringValue);
                }
            }
            catch (Exception e)
            {
                Debug.Console(2, this, "Error in Eisc_SigChange handler: {0}", e);
            }
        }
    }

    public class EiscApiPropertiesConfig
    {
        [JsonProperty("control")]
        public EssentialsControlPropertiesConfig Control { get; set; }

        [JsonProperty("devices")]
        public List<ApiDevicePropertiesConfig> Devices { get; set; }


        public class ApiDevicePropertiesConfig
        {
            [JsonProperty("deviceKey")]
            public string DeviceKey { get; set; }

            [JsonProperty("joinStart")]
            public uint JoinStart { get; set; }

            [JsonProperty("joinMapKey")]
            public string JoinMapKey { get; set; }
        }

    }


}