using System;
using System.Collections.Generic;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.EthernetCommunication;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Bridges;

//using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Helper methods for bridges
    /// </summary>
    public static class BridgeHelper
    {
        public static void PrintJoinMap(string command)
        {
            var targets = command.Split(' ');

            var bridgeKey = targets[0].Trim();

            var bridge = DeviceManager.GetDeviceForKey(bridgeKey) as EiscApi;

            if (bridge == null)
            {
                Debug.Console(0, "Unable to find bridge with key: '{0}'", bridgeKey);
                return;
            }

            if (targets.Length > 1)
            {
                var deviceKey = targets[1].Trim();

                if (string.IsNullOrEmpty(deviceKey)) return;
                bridge.PrintJoinMapForDevice(deviceKey);
            }
            else
            {
                bridge.PrintJoinMaps();
            }
        }
    }


    /// <summary>
    /// Base class for all bridge class variants
    /// </summary>
    public class BridgeBase : EssentialsDevice
    {
        public BridgeApi Api { get; protected set; }

        public BridgeBase(string key) :
            base(key)
        {

        }
    }

    /// <summary>
    /// Base class for bridge API variants
    /// </summary>
    public abstract class BridgeApi : EssentialsDevice
    {
        protected BridgeApi(string key) :
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

        protected Dictionary<string, JoinMapBaseAdvanced> JoinMaps { get; private set; }

        public ThreeSeriesTcpIpEthernetIntersystemCommunications Eisc { get; private set; }

        public EiscApi(DeviceConfig dc) :
            base(dc.Key)
        {
            JoinMaps = new Dictionary<string, JoinMapBaseAdvanced>();

            PropertiesConfig = dc.Properties.ToObject<EiscApiPropertiesConfig>();
            //PropertiesConfig = JsonConvert.DeserializeObject<EiscApiPropertiesConfig>(dc.Properties.ToString());

            Eisc = new ThreeSeriesTcpIpEthernetIntersystemCommunications(PropertiesConfig.Control.IpIdInt, PropertiesConfig.Control.TcpSshProperties.Address, Global.ControlSystem);

            Eisc.SigChange += Eisc_SigChange;

            Eisc.Register();

            AddPostActivationAction( () =>
            {
                Debug.Console(1, this, "Linking Devices...");

                foreach (var d in PropertiesConfig.Devices)
                {
                    var device = DeviceManager.GetDeviceForKey(d.DeviceKey);

                    if (device == null) continue;

                    Debug.Console(1, this, "Linking Device: '{0}'", device.Key);
                    if (device is IBridge)      // Check for this first to allow bridges in plugins to override existing bridges that apply to the same type.
                    {
                        Debug.Console(2, this, "'{0}' is IBridge", device.Key);
                        (device as IBridge).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    }
                    else if (device is IBridgeAdvanced)
                    {
                        Debug.Console(2, this, "'{0}' is IBridgeAdvanced", device.Key);
                        (device as IBridgeAdvanced).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey, this);
                    }
                    //if (device.GetType().GetCType().IsAssignableFrom(typeof (IBridge)))
                    //{
                    //    var bridge = device as IBridge;

                    //    if (bridge == null)
                    //        continue;
                    //    Debug.Console(2, this, "Linking device {0} as IBridge");
                    //    bridge.LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}

                    //if (!device.GetType().GetCType().IsAssignableFrom(typeof (IBridgeAdvanced))) continue;

                    //var bridgeAdvanced = device as IBridgeAdvanced;

                    //if (bridgeAdvanced == null) continue;
                    //Debug.Console(2, this, "Linking device {0} as IBridgeAdvanced");
                    //bridgeAdvanced.LinkToApi(Eisc, d.JoinStart, d.JoinMapKey, this);
                }
                Debug.Console(1, this, "Devices Linked.");
                //
                    //else if (device is PepperDash.Essentials.Core.Monitoring.SystemMonitorController)
                    //{
                    //    (device as PepperDash.Essentials.Core.Monitoring.SystemMonitorController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is GenericComm)
                    //{
                    //    (device as GenericComm).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is CameraBase)
                    //{
                    //    (device as CameraBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey, this);
                    //    continue;
                    //}
                    //else if (device is PepperDash.Essentials.Core.DisplayBase)
                    //{
                    //    (device as DisplayBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is DmChassisController)
                    //{
                    //    (device as DmChassisController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is DmBladeChassisController)
                    //{
                    //    (device as DmBladeChassisController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is DmpsRoutingController)
                    //{
                    //    (device as DmpsRoutingController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is DmpsAudioOutputController)
                    //{
                    //    (device as DmpsAudioOutputController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is DmTxControllerBase)
                    //{
                    //    (device as DmTxControllerBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is DmRmcControllerBase)
                    //{
                    //    (device as DmRmcControllerBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is GenericRelayDevice)
                    //{
                    //    (device as GenericRelayDevice).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is IRSetTopBoxBase)
                    //{
                    //    (device as IRSetTopBoxBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is IDigitalInput)
                    //{
                    //    (device as IDigitalInput).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is AppleTV)
                    //{
                    //    (device as AppleTV).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is HdMdxxxCEController)
                    //{
                    //    (device as HdMdxxxCEController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is LightingBase)
                    //{
                    //    (device as LightingBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is DigitalLogger)
                    //{
                    //    (device as DigitalLogger).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is PepperDash.Essentials.Devices.Common.Occupancy.GlsOccupancySensorBaseController)
                    //{
                    //    (device as PepperDash.Essentials.Devices.Common.Occupancy.GlsOccupancySensorBaseController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is StatusSignController)
                    //{
                    //    (device as StatusSignController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                    //else if (device is C2nRthsController)
                    //{
                    //    (device as C2nRthsController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    //    continue;
                    //}
                //}

                
            });
        }

        /// <summary>
        /// Adds a join map
        /// </summary>
        /// <param name="deviceKey"></param>
        /// <param name="joinMap"></param>
        public void AddJoinMap(string deviceKey, JoinMapBaseAdvanced joinMap)
        {
            if (!JoinMaps.ContainsKey(deviceKey))
            {
                JoinMaps.Add(deviceKey, joinMap);
            }
            else
            {
                Debug.Console(2, this, "Unable to add join map with key '{0}'.  Key already exists in JoinMaps dictionary", deviceKey);
            }
        }

        /// <summary>
        /// Prints all the join maps on this bridge
        /// </summary>
        public void PrintJoinMaps()
        {
            Debug.Console(0, this, "Join Maps for EISC IPID: {0}", Eisc.ID.ToString("X"));

            foreach (var joinMap in JoinMaps)
            {
                Debug.Console(0, "Join map for device '{0}':", joinMap.Key);
                joinMap.Value.PrintJoinMapInfo();
            }
        }

        /// <summary>
        /// Prints the join map for a device by key
        /// </summary>
        /// <param name="deviceKey"></param>
        public void PrintJoinMapForDevice(string deviceKey)
        {
            var joinMap = JoinMaps[deviceKey];

            if (joinMap == null)
            {
                Debug.Console(0, this, "Unable to find joinMap for device with key: '{0}'", deviceKey);
                return;
            }

            Debug.Console(0, "Join map for device '{0}' on EISC '{1}':", deviceKey, Key); 
            joinMap.PrintJoinMapInfo();
        }

        /// <summary>
        /// Used for debugging to trigger an action based on a join number and type
        /// </summary>
        /// <param name="join"></param>
        /// <param name="type"></param>
        /// <param name="state"></param>
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
        void Eisc_SigChange(object currentDevice, SigEventArgs args)
        {
            try
            {
                if (Debug.Level >= 1)
                    Debug.Console(1, this, "EiscApi change: {0} {1}={2}", args.Sig.Type, args.Sig.Number, args.Sig.StringValue);
                var uo = args.Sig.UserObject;

                if (uo == null) return;

                Debug.Console(1, this, "Executing Action: {0}", uo.ToString());
                if (uo is Action<bool>)
                    (uo as Action<bool>)(args.Sig.BoolValue);
                else if (uo is Action<ushort>)
                    (uo as Action<ushort>)(args.Sig.UShortValue);
                else if (uo is Action<string>)
                    (uo as Action<string>)(args.Sig.StringValue);
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