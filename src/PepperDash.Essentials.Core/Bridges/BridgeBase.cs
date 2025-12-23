

using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;


namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Base class for bridge API variants
    /// </summary>
    [Obsolete("Will be removed in v3.0.0")]
    public abstract class BridgeApi : EssentialsDevice
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">Device key</param>
        protected BridgeApi(string key) :
            base(key)
        {

        }
    }

    /// <summary>
    /// Class to link devices and rooms to an EISC Instance
    /// </summary>
    public class EiscApiAdvanced : BridgeApi, ICommunicationMonitor
    {
        /// <summary>
        /// Gets the PropertiesConfig
        /// </summary>
        public EiscApiPropertiesConfig PropertiesConfig { get; private set; }

        /// <summary>
        /// Gets the JoinMaps dictionary
        /// </summary>
        public Dictionary<string, JoinMapBaseAdvanced> JoinMaps { get; private set; }

        /// <summary>
        /// Gets the EISC instance
        /// </summary>
        public BasicTriList Eisc { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dc">Device configuration</param>
        /// <param name="eisc">EISC instance</param>
        public EiscApiAdvanced(DeviceConfig dc, BasicTriList eisc) :
            base(dc.Key)
        {
            JoinMaps = new Dictionary<string, JoinMapBaseAdvanced>();

            PropertiesConfig = dc.Properties.ToObject<EiscApiPropertiesConfig>();

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
                this.LogDebug("No devices linked to this bridge");
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

                this.LogWarning("{deviceKey} is not compatible with this bridge type. Please update the device.", device.Key);
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
                this.LogVerbose("Registration result: {registerResult}", registerResult);
                return;
            }

            this.LogDebug("EISC registration successful");
        }

        /// <summary>
        /// Link rooms to this EISC. Rooms MUST implement IBridgeAdvanced
        /// </summary>
        public void LinkRooms()
        {
            this.LogDebug("Linking Rooms...");

            if (PropertiesConfig.Rooms == null)
            {
                this.LogDebug("No rooms linked to this bridge.");
                return;
            }

            foreach (var room in PropertiesConfig.Rooms)
            {
                if (!(DeviceManager.GetDeviceForKey(room.RoomKey) is IBridgeAdvanced rm))
                {
                    this.LogDebug("Room {roomKey} does not implement IBridgeAdvanced. Skipping...", room.RoomKey);
                    continue;
                }

                rm.LinkToApi(Eisc, room.JoinStart, room.JoinMapKey, this);
            }
        }

        /// <summary>
        /// Adds a join map
        /// </summary>
        /// <param name="deviceKey">The key of the device to add the join map for</param>
        /// <param name="joinMap">The join map to add</param>        
        public void AddJoinMap(string deviceKey, JoinMapBaseAdvanced joinMap)
        {
            if (!JoinMaps.ContainsKey(deviceKey))
            {
                JoinMaps.Add(deviceKey, joinMap);
            }
            else
            {
                this.LogWarning("Unable to add join map with key '{deviceKey}'.  Key already exists in JoinMaps dictionary", deviceKey);
            }
        }

        /// <summary>
        /// PrintJoinMaps method
        /// </summary>        
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
        public virtual void MarkdownForBridge(string bridgeKey)
        {
            this.LogInformation("Writing Joinmaps to files for EISC IPID: {eiscId}", Eisc.ID.ToString("X"));

            foreach (var joinMap in JoinMaps)
            {
                this.LogInformation("Generating markdown for device '{deviceKey}':", joinMap.Key);
                joinMap.Value.MarkdownJoinMapInfo(joinMap.Key, bridgeKey);
            }
        }

        /// <summary>
        /// Prints the join map for a device by key
        /// </summary>
        /// <param name="deviceKey">The key of the device to print the join map for</param>        
        public void PrintJoinMapForDevice(string deviceKey)
        {
            var joinMap = JoinMaps[deviceKey];

            if (joinMap == null)
            {
                this.LogInformation("Unable to find joinMap for device with key: '{deviceKey}'", deviceKey);
                return;
            }

            this.LogInformation("Join map for device '{deviceKey}' on EISC '{eiscKey}':", deviceKey, Key);
            joinMap.PrintJoinMapInfo();
        }
        /// <summary>
        /// Prints the join map for a device by key in Markdown format
        /// </summary>
        /// <param name="deviceKey">The key of the device to print the join map for</param>
        /// <param name="bridgeKey">The key of the bridge to use for the Markdown output</param>        
        public void MarkdownJoinMapForDevice(string deviceKey, string bridgeKey)
        {
            var joinMap = JoinMaps[deviceKey];

            if (joinMap == null)
            {
                this.LogInformation("Unable to find joinMap for device with key: '{deviceKey}'", deviceKey);
                return;
            }

            this.LogInformation("Join map for device '{deviceKey}' on EISC '{eiscKey}':", deviceKey, Key);
            joinMap.MarkdownJoinMapInfo(deviceKey, bridgeKey);
        }

        /// <summary>
        /// Used for debugging to trigger an action based on a join number and type
        /// </summary>
        /// <param name="join">The join number to execute the action for</param>
        /// <param name="type">The type of join (digital, analog, serial)</param>
        /// <param name="state">The state to pass to the action</param>        
        public void ExecuteJoinAction(uint join, string type, object state)
        {
            try
            {
                switch (type.ToLower())
                {
                    case "digital":
                        {
                            if (Eisc.BooleanOutput[join].UserObject is Action<bool> userObject)
                            {
                                this.LogVerbose("Executing Boolean Action");
                                userObject(Convert.ToBoolean(state));
                            }
                            else
                                this.LogVerbose("User Object is null.  Nothing to Execute");
                            break;
                        }
                    case "analog":
                        {
                            if (Eisc.BooleanOutput[join].UserObject is Action<ushort> userObject)
                            {
                                this.LogVerbose("Executing Analog Action");
                                userObject(Convert.ToUInt16(state));
                            }
                            else
                                this.LogVerbose("User Object is null.  Nothing to Execute"); break;
                        }
                    case "serial":
                        {
                            if (Eisc.BooleanOutput[join].UserObject is Action<string> userObject)
                            {
                                this.LogVerbose("Executing Serial Action");
                                userObject(Convert.ToString(state));
                            }
                            else
                                this.LogVerbose("User Object is null.  Nothing to Execute");
                            break;
                        }
                    default:
                        {
                            this.LogVerbose("Unknown join type.  Use digital/serial/analog");
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                this.LogError("ExecuteJoinAction error: {message}", e.Message);
                this.LogDebug(e, "Stack Trace: ");
            }

        }

        /// <summary>
        /// Handle incoming sig changes
        /// </summary>
        /// <param name="currentDevice">BasicTriList device that triggered the event</param>
        /// <param name="args">Event arguments containing the signal information</param>
        protected void Eisc_SigChange(object currentDevice, SigEventArgs args)
        {
            try
            {
                this.LogVerbose("EiscApiAdvanced change: {0} {1}={2}", args.Sig.Type, args.Sig.Number, args.Sig.StringValue);
                var uo = args.Sig.UserObject;

                if (uo == null) return;

                this.LogDebug("Executing Action: {0}", uo.ToString());
                if (uo is Action<bool>)
                    (uo as Action<bool>)(args.Sig.BoolValue);
                else if (uo is Action<ushort>)
                    (uo as Action<ushort>)(args.Sig.UShortValue);
                else if (uo is Action<string>)
                    (uo as Action<string>)(args.Sig.StringValue);
            }
            catch (Exception e)
            {
                this.LogError("Eisc_SigChange handler error: {message}", e.Message);
                this.LogDebug(e, "Stack Trace: ");
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
    /// Factory class for EiscApiAdvanced devices
    /// </summary>
    /// <remarks>
    /// Supported types:
    /// eiscapiadv - Create a standard EISC client over TCP/IP
    /// eiscapiadvanced - Create a standard EISC client over TCP/IP
    /// eiscapiadvancedserver - Create an EISC server
    /// eiscapiadvancedclient - Create an EISC client
    /// vceiscapiadv - Create a VC-4 EISC client
    /// vceiscapiadvanced - Create a VC-4 EISC client
    /// eiscapiadvudp - Create a standard EISC client over UDP
    /// eiscapiadvancedudp - Create a standard EISC client over UDP
    /// </remarks>
    public class EiscApiAdvancedFactory : EssentialsDeviceFactory<EiscApiAdvanced>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EiscApiAdvancedFactory()
        {
            TypeNames = new List<string> { "eiscapiadv", "eiscapiadvanced", "eiscapiadvancedserver", "eiscapiadvancedclient", "vceiscapiadv", "vceiscapiadvanced", "eiscapiadvudp", "eiscapiadvancedudp" };
        }

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
                            Debug.LogInformation("Unable to build VC-4 EISC Client for device {0}. Room ID is missing or empty", dc.Key);
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