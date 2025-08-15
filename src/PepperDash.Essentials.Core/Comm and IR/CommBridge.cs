using System;
using System.Collections.Generic;
using System.Text;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;


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

        /// <summary>
        /// Event triggered when text is received through the communication bridge.
        /// </summary>
        public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

        /// <summary>
        /// Event triggered when bytes are received through the communication bridge.
        /// </summary>
        public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;

        /// <summary>
        /// Indicates whether the communication bridge is currently connected.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommBridge"/> class.
        /// </summary>
        /// <param name="key">The unique key for the communication bridge.</param>
        /// <param name="name">The display name for the communication bridge.</param>
        public CommBridge(string key, string name)
            : base(key, name)
        {

        }

        /// <summary>
        /// Sends a byte array through the communication bridge.
        /// </summary>
        /// <param name="bytes">The byte array to send.</param>
        public void SendBytes(byte[] bytes)
        {
            if (eisc == null)
            {
                this.LogWarning("EISC is null, cannot send bytes.");
                return;
            }
            eisc.Eisc.SetString(joinMap.SendText.JoinNumber, Encoding.ASCII.GetString(bytes, 0, bytes.Length));
        }

        /// <summary>
        /// Sends a text string through the communication bridge.
        /// </summary>
        /// <param name="text">The text string to send.</param>
        public void SendText(string text)
        {
            if (eisc == null)
            {
                this.LogWarning("EISC is null, cannot send text.");
                return;
            }
            eisc.Eisc.SetString(joinMap.SendText.JoinNumber, text);
        }

        /// <summary>
        /// Initiates a connection through the communication bridge.
        /// </summary>
        public void Connect()
        {
            if (eisc == null)
            {
                this.LogWarning("EISC is null, cannot connect.");
                return;
            }
            eisc.Eisc.SetBool(joinMap.Connect.JoinNumber, true);
        }

        /// <summary>
        /// Terminates the connection through the communication bridge.
        /// </summary>
        public void Disconnect()
        {
            if (eisc == null)
            {
                this.LogWarning("EISC is null, cannot disconnect.");
                return;
            }
            eisc.Eisc.SetBool(joinMap.Connect.JoinNumber, false);
        }

        /// <inheritdoc />
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
                this.LogWarning("Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            this.LogDebug("Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            eisc = bridge;

            trilist.SetBoolSigAction(joinMap.Connected.JoinNumber, (b) => IsConnected = b);

            trilist.SetStringSigAction(joinMap.TextReceived.JoinNumber, (s) =>
            {
                TextReceived?.Invoke(this, new GenericCommMethodReceiveTextArgs(s));
                BytesReceived?.Invoke(this, new GenericCommMethodReceiveBytesArgs(Encoding.ASCII.GetBytes(s)));
            });
        }
    }
}