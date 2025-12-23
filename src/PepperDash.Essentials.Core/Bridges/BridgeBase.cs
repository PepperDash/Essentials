

using System;
using System.Collections.Generic;
using System.Reflection;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

//using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.Core.Bridges
{
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
    /// Represents a EiscApiAdvanced
    /// </summary>
    public class EiscApiAdvanced : BridgeApi, ICommunicationMonitor
    {
        public EiscApiPropertiesConfig PropertiesConfig { get; private set; }

        public Dictionary<string, JoinMapBaseAdvanced> JoinMaps { get; private set; }

        public BasicTriList Eisc { get; private set; }

        public EiscApiAdvanced(DeviceConfig dc, BasicTriList eisc) :
            base(dc.Key)
        {
            JoinMaps = new Dictionary<string, JoinMapBaseAdvanced>();

            PropertiesConfig = dc.Properties.ToObject<EiscApiPropertiesConfig>();
            //PropertiesConfig = JsonConvert.DeserializeObject<EiscApiPropertiesConfig>(dc.Properties.ToString());

            Eisc = eisc;

            Eisc.SigChange += Eisc_SigChange;

            CommunicationMonitor = new CrestronGenericBaseCommunicationMonitor(this, Eisc, 120000, 300000);

            AddPostActivationAction(LinkDevices);
            AddPostActivationAction(LinkRooms);
            AddPostActivationAction(RegisterEisc);
        }

        /// <summary>
        /// CustomActivate method
        /// </summary>
        /// <inheritdoc />
        public override bool CustomActivate()
        {
            CommunicationMonitor.Start();
            return base.CustomActivate();
        }

        /// <summary>
        /// Deactivate method
        /// </summary>
        public override bool Deactivate()
        {
            CommunicationMonitor.Stop();
            return base.Deactivate();
        }

        private void LinkDevices()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Linking Devices...");

            if (PropertiesConfig.Devices == null)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "No devices linked to this bridge");
                return;
            }

            foreach (var d in PropertiesConfig.Devices)
            {
                var device = DeviceManager.GetDeviceForKey(d.DeviceKey);

                if (device == null)
                {
                    continue;
                }

                Debug.LogMessage(LogEventLevel.Debug, this, "Linking Device: '{0}'", device.Key);

                if (device is IBridgeAdvanced bridge)
                {
                    bridge.LinkToApi(Eisc, d.JoinStart, d.JoinMapKey, this);
                    continue;
                }

                Debug.LogMessage(LogEventLevel.Information, this,
                        "{0} is not compatible with this bridge type. Please use 'eiscapi' instead, or updae the device.",
                        device.Key);
            }
        }

        private void RegisterEisc()
        {
            if (Eisc.Registered)
            {
                return;
            }

            var registerResult = Eisc.Register();

            if (registerResult != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                Debug.LogMessage(LogEventLevel.Verbose, this, "Registration result: {0}", registerResult);
                return;
            }

            Debug.LogMessage(LogEventLevel.Debug, this, "EISC registration successful");
        }

        /// <summary>
        /// LinkRooms method
        /// </summary>
        public void LinkRooms()
        {
            Debug.LogMessage(LogEventLevel.Debug, this, "Linking Rooms...");

            if (PropertiesConfig.Rooms == null)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "No rooms linked to this bridge.");
                return;
            }

            foreach (var room in PropertiesConfig.Rooms)
            {
                var rm = DeviceManager.GetDeviceForKey(room.RoomKey) as IBridgeAdvanced;

                if (rm == null)
                {
                    Debug.LogMessage(LogEventLevel.Debug, this,
                        "Room {0} does not implement IBridgeAdvanced. Skipping...", room.RoomKey);
                    continue;
                }

                rm.LinkToApi(Eisc, room.JoinStart, room.JoinMapKey, this);
            }
        }

        /// <summary>
        /// Adds a join map
        /// </summary>
        /// <param name="deviceKey"></param>
        /// <param name="joinMap"></param>
        /// <summary>
        /// AddJoinMap method
        /// </summary>
        public void AddJoinMap(string deviceKey, JoinMapBaseAdvanced joinMap)
        {
            if (!JoinMaps.ContainsKey(deviceKey))
            {
                JoinMaps.Add(deviceKey, joinMap);
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Verbose, this, "Unable to add join map with key '{0}'.  Key already exists in JoinMaps dictionary", deviceKey);
            }
        }

        /// <summary>
        /// PrintJoinMaps method
        /// </summary>
        /// <inheritdoc />
        public virtual void PrintJoinMaps()
        {
            CrestronConsole.ConsoleCommandResponse("Join Maps for EISC IPID: {0}\r\n", Eisc.ID.ToString("X"));

            foreach (var joinMap in JoinMaps)
            {
                CrestronConsole.ConsoleCommandResponse("Join map for device '{0}':", joinMap.Key);
                joinMap.Value.PrintJoinMapInfo();
            }
        }
        /// <summary>
        /// MarkdownForBridge method
        /// </summary>
        /// <inheritdoc />
        public virtual void MarkdownForBridge(string bridgeKey)
        {
            Debug.LogMessage(LogEventLevel.Information, this, "Writing Joinmaps to files for EISC IPID: {0}", Eisc.ID.ToString("X"));

            foreach (var joinMap in JoinMaps)
            {
                Debug.LogMessage(LogEventLevel.Information, "Generating markdown for device '{0}':", joinMap.Key);
                joinMap.Value.MarkdownJoinMapInfo(joinMap.Key, bridgeKey);
            }
        }

        /// <summary>
        /// Prints the join map for a device by key
        /// </summary>
        /// <param name="deviceKey"></param>
        /// <summary>
        /// PrintJoinMapForDevice method
        /// </summary>
        public void PrintJoinMapForDevice(string deviceKey)
        {
            var joinMap = JoinMaps[deviceKey];

            if (joinMap == null)
            {
                Debug.LogMessage(LogEventLevel.Information, this, "Unable to find joinMap for device with key: '{0}'", deviceKey);
                return;
            }

            Debug.LogMessage(LogEventLevel.Information, "Join map for device '{0}' on EISC '{1}':", deviceKey, Key);
            joinMap.PrintJoinMapInfo();
        }
        /// <summary>
        /// Prints the join map for a device by key
        /// </summary>
        /// <param name="deviceKey"></param>
        /// <summary>
        /// MarkdownJoinMapForDevice method
        /// </summary>
        public void MarkdownJoinMapForDevice(string deviceKey, string bridgeKey)
        {
            var joinMap = JoinMaps[deviceKey];

            if (joinMap == null)
            {
                Debug.LogMessage(LogEventLevel.Information, this, "Unable to find joinMap for device with key: '{0}'", deviceKey);
                return;
            }

            Debug.LogMessage(LogEventLevel.Information, "Join map for device '{0}' on EISC '{1}':", deviceKey, Key);
            joinMap.MarkdownJoinMapInfo(deviceKey, bridgeKey);
        }

        /// <summary>
        /// Used for debugging to trigger an action based on a join number and type
        /// </summary>
        /// <param name="join"></param>
        /// <param name="type"></param>
        /// <param name="state"></param>
        /// <summary>
        /// ExecuteJoinAction method
        /// </summary>
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
                                Debug.LogMessage(LogEventLevel.Verbose, this, "Executing Action: {0}", uo.ToString());
                                uo(Convert.ToBoolean(state));
                            }
                            else
                                Debug.LogMessage(LogEventLevel.Verbose, this, "User Action is null.  Nothing to Execute");
                            break;
                        }
                    case "analog":
                        {
                            var uo = Eisc.BooleanOutput[join].UserObject as Action<ushort>;
                            if (uo != null)
                            {
                                Debug.LogMessage(LogEventLevel.Verbose, this, "Executing Action: {0}", uo.ToString());
                                uo(Convert.ToUInt16(state));
                            }
                            else
                                Debug.LogMessage(LogEventLevel.Verbose, this, "User Action is null.  Nothing to Execute"); break;
                        }
                    case "serial":
                        {
                            var uo = Eisc.BooleanOutput[join].UserObject as Action<string>;
                            if (uo != null)
                            {
                                Debug.LogMessage(LogEventLevel.Verbose, this, "Executing Action: {0}", uo.ToString());
                                uo(Convert.ToString(state));
                            }
                            else
                                Debug.LogMessage(LogEventLevel.Verbose, this, "User Action is null.  Nothing to Execute");
                            break;
                        }
                    default:
                        {
                            Debug.LogMessage(LogEventLevel.Verbose, "Unknown join type.  Use digital/serial/analog");
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Error: {0}", e);
            }

        }

        /// <summary>
        /// Handles incoming sig changes
        /// </summary>
        /// <param name="currentDevice"></param>
        /// <param name="args"></param>
        protected void Eisc_SigChange(object currentDevice, SigEventArgs args)
        {
            try
            {
                Debug.LogMessage(LogEventLevel.Verbose, this, "EiscApiAdvanced change: {0} {1}={2}", args.Sig.Type, args.Sig.Number, args.Sig.StringValue);
                var uo = args.Sig.UserObject;

                if (uo == null) return;

                Debug.LogMessage(LogEventLevel.Debug, this, "Executing Action: {0}", uo.ToString());
                if (uo is Action<bool>)
                    (uo as Action<bool>)(args.Sig.BoolValue);
                else if (uo is Action<ushort>)
                    (uo as Action<ushort>)(args.Sig.UShortValue);
                else if (uo is Action<string>)
                    (uo as Action<string>)(args.Sig.StringValue);
            }
            catch (Exception e)
            {
                Debug.LogMessage(LogEventLevel.Verbose, this, "Error in Eisc_SigChange handler: {0}", e);
            }
        }

        #region Implementation of ICommunicationMonitor

        /// <summary>
        /// Gets or sets the CommunicationMonitor
        /// </summary>
        public StatusMonitorBase CommunicationMonitor { get; private set; }

        #endregion
    }

    /// <summary>
    /// Represents a EiscApiPropertiesConfig
    /// </summary>
    public class EiscApiPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the Control
        /// </summary>
        [JsonProperty("control")]
        public EssentialsControlPropertiesConfig Control { get; set; }

        /// <summary>
        /// Gets or sets the Devices
        /// </summary>
        [JsonProperty("devices")]
        public List<ApiDevicePropertiesConfig> Devices { get; set; }

        /// <summary>
        /// Gets or sets the Rooms
        /// </summary>
        [JsonProperty("rooms")]
        public List<ApiRoomPropertiesConfig> Rooms { get; set; }


        /// <summary>
        /// Represents a ApiDevicePropertiesConfig
        /// </summary>
        public class ApiDevicePropertiesConfig
        {
            /// <summary>
            /// Gets or sets the DeviceKey
            /// </summary>
            [JsonProperty("deviceKey")]
            public string DeviceKey { get; set; }

            /// <summary>
            /// Gets or sets the JoinStart
            /// </summary>
            [JsonProperty("joinStart")]
            public uint JoinStart { get; set; }

            /// <summary>
            /// Gets or sets the JoinMapKey
            /// </summary>
            [JsonProperty("joinMapKey")]
            public string JoinMapKey { get; set; }
        }

        /// <summary>
        /// Represents a ApiRoomPropertiesConfig
        /// </summary>
        public class ApiRoomPropertiesConfig
        {
            /// <summary>
            /// Gets or sets the RoomKey
            /// </summary>
            [JsonProperty("roomKey")]
            public string RoomKey { get; set; }

            /// <summary>
            /// Gets or sets the JoinStart
            /// </summary>
            [JsonProperty("joinStart")]
            public uint JoinStart { get; set; }

            /// <summary>
            /// Gets or sets the JoinMapKey
            /// </summary>
            [JsonProperty("joinMapKey")]
            public string JoinMapKey { get; set; }
        }

    }

    /// <summary>
    /// Represents a EiscApiAdvancedFactory
    /// </summary>
    public class EiscApiAdvancedFactory : EssentialsDeviceFactory<EiscApiAdvanced>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EiscApiAdvancedFactory()
        {
            TypeNames = new List<string> { "eiscapiadv", "eiscapiadvanced", "eiscapiadvancedserver", "eiscapiadvancedclient", "vceiscapiadv", "vceiscapiadvanced", "eiscapiadvudp", "eiscapiadvancedudp" };
        }

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogDebug("Attempting to create new EiscApiAdvanced Device");

            var controlProperties = CommFactory.GetControlPropertiesConfig(dc);

            BasicTriList eisc;

            switch (dc.Type.ToLower())
            {
                case "eiscapiadvudp":
                case "eiscapiadvancedudp":
                    {
                        eisc = new EthernetIntersystemCommunications(controlProperties.IpIdInt,
                            controlProperties.TcpSshProperties.Address, Global.ControlSystem);
                        break;
                    }
                case "eiscapiadv":
                case "eiscapiadvanced":
                    {
                        eisc = new ThreeSeriesTcpIpEthernetIntersystemCommunications(controlProperties.IpIdInt,
                            controlProperties.TcpSshProperties.Address, Global.ControlSystem);
                        break;
                    }
                case "eiscapiadvancedserver":
                    {
                        eisc = new EISCServer(controlProperties.IpIdInt, Global.ControlSystem);
                        break;
                    }
                case "eiscapiadvancedclient":
                    {
                        eisc = new EISCClient(controlProperties.IpIdInt, controlProperties.TcpSshProperties.Address, Global.ControlSystem);
                        break;
                    }
                case "vceiscapiadv":
                case "vceiscapiadvanced":
                    {
                        if (string.IsNullOrEmpty(controlProperties.RoomId))
                        {
                            Debug.LogMessage(LogEventLevel.Information, "Unable to build VC-4 EISC Client for device {0}. Room ID is missing or empty", dc.Key);
                            eisc = null;
                            break;
                        }
                        eisc = new VirtualControlEISCClient(controlProperties.IpIdInt, controlProperties.RoomId,
                            Global.ControlSystem);
                        break;
                    }
                default:
                    eisc = null;
                    break;
            }

            if (eisc == null)
            {
                return null;
            }

            return new EiscApiAdvanced(dc, eisc);
        }
    }

}