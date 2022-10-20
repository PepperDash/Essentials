﻿using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;

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

            var bridge = DeviceManager.GetDeviceForKey(bridgeKey) as EiscApiAdvanced;

            if (bridge == null)
            {
                Debug.Console(0, "Unable to find advanced bridge with key: '{0}'", bridgeKey);
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
        public static void JoinmapMarkdown(string command)
        {
            var targets = command.Split(' ');

            var bridgeKey = targets[0].Trim();

            var bridge = DeviceManager.GetDeviceForKey(bridgeKey) as EiscApiAdvanced;

            if (bridge == null)
            {
                Debug.Console(0, "Unable to find advanced bridge with key: '{0}'", bridgeKey);
                return;
            }

            if (targets.Length > 1)
            {
                var deviceKey = targets[1].Trim();

                if (string.IsNullOrEmpty(deviceKey)) return;
                bridge.MarkdownJoinMapForDevice(deviceKey, bridgeKey);
            }
            else
            {
                bridge.MarkdownForBridge(bridgeKey);

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
    public class EiscApiAdvanced : BridgeApi, ICommunicationMonitor
    {
        public EiscApiPropertiesConfig PropertiesConfig { get; private set; }

        protected Dictionary<string, JoinMapBaseAdvanced> JoinMaps { get; private set; }

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

        public override bool CustomActivate()
        {
            CommunicationMonitor.Start();
            return base.CustomActivate();
        }

        public override bool Deactivate()
        {
            CommunicationMonitor.Stop();
            return base.Deactivate();
        }

        private void LinkDevices()
        {
            Debug.Console(1, this, "Linking Devices...");

            if (PropertiesConfig.Devices == null)
            {
                Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "No devices linked to this bridge");
                return;
            }

            foreach (var d in PropertiesConfig.Devices)
            {
                var device = DeviceManager.GetDeviceForKey(d.DeviceKey);

                if (device == null)
                {
                    continue;
                }

                Debug.Console(1, this, "Linking Device: '{0}'", device.Key);

                if (!typeof (IBridgeAdvanced).IsAssignableFrom(device.GetType().GetCType()))
                {
                    Debug.Console(0, this, Debug.ErrorLogLevel.Notice,
                        "{0} is not compatible with this bridge type. Please use 'eiscapi' instead, or updae the device.",
                        device.Key);
                    continue;
                }

                var bridge = device as IBridgeAdvanced;
                if (bridge != null)
                {
                    bridge.LinkToApi(Eisc, d.JoinStart, d.JoinMapKey, this);
                }
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
                Debug.Console(2, this, Debug.ErrorLogLevel.Error, "Registration result: {0}", registerResult);
                return;
            }

            Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "EISC registration successful");
        }

        public void LinkRooms()
        {
            Debug.Console(1, this, "Linking Rooms...");

            if (PropertiesConfig.Rooms == null)
            {
                Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "No rooms linked to this bridge.");
                return;
            }

            foreach (var room in PropertiesConfig.Rooms)
            {
                var rm = DeviceManager.GetDeviceForKey(room.RoomKey) as IBridgeAdvanced;

                if (rm == null)
                {
                    Debug.Console(1, this, Debug.ErrorLogLevel.Notice,
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
        public virtual void PrintJoinMaps()
        {
            Debug.Console(0, this, "Join Maps for EISC IPID: {0}", Eisc.ID.ToString("X"));

            foreach (var joinMap in JoinMaps)
            {
                Debug.Console(0, "Join map for device '{0}':", joinMap.Key);
                joinMap.Value.PrintJoinMapInfo();
            }
        }
        /// <summary>
        /// Generates markdown for all join maps on this bridge
        /// </summary>
        public virtual void MarkdownForBridge(string bridgeKey)
        {
            Debug.Console(0, this, "Writing Joinmaps to files for EISC IPID: {0}", Eisc.ID.ToString("X"));

            foreach (var joinMap in JoinMaps)
            {
                Debug.Console(0, "Generating markdown for device '{0}':", joinMap.Key);
                joinMap.Value.MarkdownJoinMapInfo(joinMap.Key, bridgeKey);
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
        /// Prints the join map for a device by key
        /// </summary>
        /// <param name="deviceKey"></param>
        public void MarkdownJoinMapForDevice(string deviceKey, string bridgeKey)
        {
            var joinMap = JoinMaps[deviceKey];

            if (joinMap == null)
            {
                Debug.Console(0, this, "Unable to find joinMap for device with key: '{0}'", deviceKey);
                return;
            }

            Debug.Console(0, "Join map for device '{0}' on EISC '{1}':", deviceKey, Key);
            joinMap.MarkdownJoinMapInfo(deviceKey, bridgeKey);
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
                                Debug.Console(2, this, "Executing Action: {0}", uo.ToString());
                                uo(Convert.ToBoolean(state));
                            }
                            else
                                Debug.Console(2, this, "User Action is null.  Nothing to Execute");
                            break;
                        }
                    case "analog":
                        {
                            var uo = Eisc.BooleanOutput[join].UserObject as Action<ushort>;
                            if (uo != null)
                            {
                                Debug.Console(2, this, "Executing Action: {0}", uo.ToString());
                                uo(Convert.ToUInt16(state));
                            }
                            else
                                Debug.Console(2, this, "User Action is null.  Nothing to Execute"); break;
                        }
                    case "serial":
                        {
                            var uo = Eisc.BooleanOutput[join].UserObject as Action<string>;
                            if (uo != null)
                            {
                                Debug.Console(2, this, "Executing Action: {0}", uo.ToString());
                                uo(Convert.ToString(state));
                            }
                            else
                                Debug.Console(2, this, "User Action is null.  Nothing to Execute");
                            break;
                        }
                    default:
                        {
                            Debug.Console(2, "Unknown join type.  Use digital/serial/analog");
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
        protected void Eisc_SigChange(object currentDevice, SigEventArgs args)
        {
            try
            {
                if (Debug.Level >= 1)
                    Debug.Console(1, this, "EiscApiAdvanced change: {0} {1}={2}", args.Sig.Type, args.Sig.Number, args.Sig.StringValue);
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

        #region Implementation of ICommunicationMonitor

        public StatusMonitorBase CommunicationMonitor { get; private set; }

        #endregion
    }

    public class EiscApiPropertiesConfig
    {
        [JsonProperty("control")]
        public EssentialsControlPropertiesConfig Control { get; set; }

        [JsonProperty("devices")]
        public List<ApiDevicePropertiesConfig> Devices { get; set; }

        [JsonProperty("rooms")]
        public List<ApiRoomPropertiesConfig> Rooms { get; set; } 


        public class ApiDevicePropertiesConfig
        {
            [JsonProperty("deviceKey")]
            public string DeviceKey { get; set; }

            [JsonProperty("joinStart")]
            public uint JoinStart { get; set; }

            [JsonProperty("joinMapKey")]
            public string JoinMapKey { get; set; }
        }

        public class ApiRoomPropertiesConfig
        {
            [JsonProperty("roomKey")]
            public string RoomKey { get; set; }

            [JsonProperty("joinStart")]
            public uint JoinStart { get; set; }

            [JsonProperty("joinMapKey")]
            public string JoinMapKey { get; set; }
        }

    }

    public class EiscApiAdvancedFactory : EssentialsDeviceFactory<EiscApiAdvanced>
    {
        public EiscApiAdvancedFactory()
        {
            TypeNames = new List<string> { "eiscapiadv", "eiscapiadvanced", "eiscapiadvancedserver", "eiscapiadvancedclient",  "vceiscapiadv", "vceiscapiadvanced" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new EiscApiAdvanced Device");

            var controlProperties = CommFactory.GetControlPropertiesConfig(dc);

            switch (dc.Type.ToLower())
            {
                case "eiscapiadv":
                case "eiscapiadvanced":
                {
                    var eisc = new ThreeSeriesTcpIpEthernetIntersystemCommunications(controlProperties.IpIdInt,
                        controlProperties.TcpSshProperties.Address, Global.ControlSystem);
                    return new EiscApiAdvanced(dc, eisc);
                }
                case "eiscapiadvancedserver":
                {
                    var eisc = new EISCServer(controlProperties.IpIdInt, Global.ControlSystem);
                    return new EiscApiAdvanced(dc, eisc);
                }
                case "eiscapiadvancedclient":
                {
                    var eisc = new EISCClient(controlProperties.IpIdInt, controlProperties.TcpSshProperties.Address, Global.ControlSystem);
                    return new EiscApiAdvanced(dc, eisc);
                }
                case "vceiscapiadv":
                case "vceiscapiadvanced":
                {
                    var eisc = new VirtualControlEISCClient(controlProperties.IpIdInt, InitialParametersClass.RoomId,
                        Global.ControlSystem);
                    return new EiscApiAdvanced(dc, eisc);
                }
                default:
                    return null;
            }
        }
    }

}