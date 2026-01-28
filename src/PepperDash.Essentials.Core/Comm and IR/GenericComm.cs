

using System;
using System.Collections.Generic;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Serves as a generic wrapper class for all styles of IBasicCommuncation ports
    /// </summary>
    [Description("Generic communication wrapper class for any IBasicCommunication type")]
    public class GenericComm : ReconfigurableBridgableDevice
    {
        EssentialsControlPropertiesConfig PropertiesConfig;

        /// <summary>
        /// Gets the CommPort
        /// </summary>
        public IBasicCommunication CommPort { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">the config of the device</param>
        public GenericComm(DeviceConfig config)
            : base(config)
        {

            PropertiesConfig = CommFactory.GetControlPropertiesConfig(config);

            var commPort = CommFactory.CreateCommForDevice(config);

            //Fixing decision to require '-comPorts' in declaration for DGE in order to get a device with comports included
            if (commPort == null)
            {
                config.Key = config.Key + "-comPorts";
                commPort = CommFactory.CreateCommForDevice(config);
            }

            CommPort = commPort;

        }

        /// <summary>
        /// BuildDevice method
        /// </summary>
        public static IKeyed BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Generic Comm Device");
            return new GenericComm(dc);
        }

        /// <summary>
        /// SetPortConfig method
        /// </summary>
        public void SetPortConfig(string portConfig)
        {
            // TODO: Deserialize new EssentialsControlPropertiesConfig and handle as necessary
            try
            {
                PropertiesConfig = JsonConvert.DeserializeObject<EssentialsControlPropertiesConfig>
                    (portConfig);
            }
            catch (Exception e)
            {
                Debug.LogMessage(LogEventLevel.Verbose, this, "Error deserializing port config: {0}", e);
            }
        }

        /// <summary>
        /// CustomSetConfig method
        /// </summary>
        /// <param name="config">the new device configuration</param>
        protected override void CustomSetConfig(DeviceConfig config)
        {
            PropertiesConfig = CommFactory.GetControlPropertiesConfig(config);

            ConfigWriter.UpdateDeviceConfig(config);
        }

        /// <summary>
        /// LinkToApi method
        /// </summary>
        /// <inheritdoc />
        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new IBasicCommunicationJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<IBasicCommunicationJoinMap>(joinMapSerialized);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Information, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            if (CommPort == null)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Unable to link device '{0}'.  CommPort is null", Key);
                return;
            }

            Debug.LogMessage(LogEventLevel.Debug, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            // this is a permanent event handler. This cannot be -= from event
            CommPort.TextReceived += (s, a) =>
            {
                trilist.SetString(joinMap.TextReceived.JoinNumber, a.Text);
            };
            trilist.SetStringSigAction(joinMap.SendText.JoinNumber, s => CommPort.SendText(s));
            trilist.SetStringSigAction(joinMap.SetPortConfig.JoinNumber, SetPortConfig);


            var sComm = CommPort as ISocketStatus;
            if (sComm == null) return;
            sComm.ConnectionChange += (s, a) =>
            {
                trilist.SetUshort(joinMap.Status.JoinNumber, (ushort)(a.Client.ClientStatus));
                trilist.SetBool(joinMap.Connected.JoinNumber, a.Client.ClientStatus ==
                                                   SocketStatus.SOCKET_STATUS_CONNECTED);
            };

            trilist.SetBoolSigAction(joinMap.Connect.JoinNumber, b =>
            {
                if (b)
                {
                    sComm.Connect();
                }
                else
                {
                    sComm.Disconnect();
                }
            });
        }
    }

    /// <summary>
    /// Represents a GenericCommFactory
    /// </summary>
    public class GenericCommFactory : EssentialsDeviceFactory<GenericComm>
    {
        /// <summary>
        /// Initializes a new instance of the GenericCommFactory class.
        /// </summary>
        public GenericCommFactory()
        {
            TypeNames = new List<string>() { "genericComm" };
        }

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Generic Comm Device");
            return new GenericComm(dc);
        }
    }
}