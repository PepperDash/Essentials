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
    /// Implements IBasicCommunication and sends all communication through an EISC
    /// </summary>
    [Description("Generic communication wrapper class for any IBasicCommunication type")]
    public class GenericCommBridge : ReconfigurableBridgableDevice, IBasicCommunication
    {       
        public GenericComm(DeviceConfig config)
            : base(config)
        {

        }

        public static IKeyed BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            return new GenericComm(dc);
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

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

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

    public class GenericCommBridgeFactory : EssentialsDeviceFactory<GenericCommBridge>
    {
        public GenericCommBridgeFactory()
        {
            TypeNames = new List<string>() { "genericCommBridge" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
            return new GenericComm(dc);
        }
    }
}