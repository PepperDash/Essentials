using System;
using System.Collections.Generic;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using System.Text;

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
    public class CommBridge : EssentialsBridgeableDevice, IBasicCommunication
    {
        private EiscApiAdvanced eisc;

        private IBasicCommunicationJoinMap joinMap;

        public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

        public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;

        public bool IsConnected { get; private set; }

        public CommBridge(string key, string name)
            : base(key, name)
        {

        }

        public void SendBytes(byte[] bytes)
        {
            eisc.Eisc.SetString(joinMap.SendText.JoinNumber, Encoding.ASCII.GetString(bytes, 0, bytes.Length));
        }

        public void SendText(string text)
        {
            eisc.Eisc.SetString(joinMap.SendText.JoinNumber, text);
        }

        public void Connect() {
            eisc.Eisc.SetBool(joinMap.Connect.JoinNumber, true);
        }

        public void Disconnect() {
            eisc.Eisc.SetBool(joinMap.Connect.JoinNumber, false);
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            joinMap = new IBasicCommunicationJoinMap(joinStart);

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

            eisc = bridge;

            trilist.SetBoolSigAction(joinMap.Connected.JoinNumber, (b) => IsConnected = b);

            trilist.SetStringSigAction(joinMap.TextReceived.JoinNumber, (s) => {
                var textHandler = TextReceived;

                if (textHandler != null)
                {
                    textHandler(this, new GenericCommMethodReceiveTextArgs(s));
                }

                var bytesHandler = BytesReceived;

                if(bytesHandler != null)
                {
                    bytesHandler(this, new GenericCommMethodReceiveBytesArgs(Encoding.ASCII.GetBytes(s)));
                }
            });
        }
    }
}