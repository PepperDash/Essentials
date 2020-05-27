using System;
using System.Collections.Generic;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Config;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Serves as a generic wrapper class for all styles of IBasicCommuncation ports
    /// </summary>
    [Description("Generic communication wrapper class for any IBasicCommunication type")]
    public class GenericComm : ReconfigurableBridgableDevice
    {
        EssentialsControlPropertiesConfig PropertiesConfig;

        public IBasicCommunication CommPort { get; private set; }

        public GenericComm(DeviceConfig config)
            : base(config)
        {
            PropertiesConfig = CommFactory.GetControlPropertiesConfig(config);

            var commPort = CommFactory.CreateCommForDevice(config);

            //Fixing decision to require '-comPorts' in delcaration for DGE in order to get a device with comports included
            if (commPort == null)
            {
                config.Key = config.Key + "-comPorts";
                commPort = CommFactory.CreateCommForDevice(config);
            }

            CommPort = commPort;

        }

        public static IKeyed BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            return new GenericComm(dc);
        }

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
                Debug.Console(2, this, "Error deserializing port config: {0}", e);
            }
        }

        protected override void CustomSetConfig(DeviceConfig config)
        {
            PropertiesConfig = CommFactory.GetControlPropertiesConfig(config);

            ConfigWriter.UpdateDeviceConfig(config);
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new IBasicCommunicationJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<IBasicCommunicationJoinMap>(joinMapSerialized);

            bridge.AddJoinMap(Key, joinMap);

            if (CommPort == null)
            {
                Debug.Console(1, this, "Unable to link device '{0}'.  CommPort is null", Key);
                return;
            }

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            // this is a permanent event handler. This cannot be -= from event
            CommPort.TextReceived += (s, a) =>
            {
                Debug.Console(2, this, "RX: {0}", a.Text);
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

    public class GenericCommFactory : EssentialsDeviceFactory<GenericComm>
    {
        public GenericCommFactory()
        {
            TypeNames = new List<string>() { "genericComm" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            return new GenericComm(dc);
        }
    }
}