using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a CecPortController
    /// </summary>
    public class CecPortController : Device, IBasicCommunicationWithStreamDebugging
    {
        /// <summary>
        /// Gets or sets the StreamDebugging
        /// </summary>
        public CommunicationStreamDebugging StreamDebugging { get; private set; }

        /// <summary>
        /// Event raised when bytes are received
        /// </summary>
        public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;

        /// <summary>
        /// Event raised when text is received
        /// </summary>
        public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

        /// <summary>
        /// Gets or sets the IsConnected
        /// </summary>
        public bool IsConnected { get { return true; } }

        ICec Port;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">key of the device</param>
        /// <param name="postActivationFunc">post activation function for the device</param>
        /// <param name="config">configuration for the device</param>
        public CecPortController(string key, Func<EssentialsControlPropertiesConfig, ICec> postActivationFunc,
            EssentialsControlPropertiesConfig config) : base(key)
        {
            StreamDebugging = new CommunicationStreamDebugging(key);

            AddPostActivationAction(() =>
            {
                Port = postActivationFunc(config);

                Port.StreamCec.CecChange += StreamCec_CecChange;
            });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">key of the device</param>
        /// <param name="port">CEC port</param>
        public CecPortController(string key, ICec port)
            : base(key)
        {
            Port = port;

            Port.StreamCec.CecChange += new CecChangeEventHandler(StreamCec_CecChange);
        }

        void StreamCec_CecChange(Cec cecDevice, CecEventArgs args)
        {
            if (args.EventId == CecEventIds.CecMessageReceivedEventId)
                OnDataReceived(cecDevice.Received.StringValue);
            else if (args.EventId == CecEventIds.ErrorFeedbackEventId)
                if (cecDevice.ErrorFeedback.BoolValue)
                    Debug.LogMessage(LogEventLevel.Verbose, this, "CEC NAK Error");
        }

        void OnDataReceived(string s)
        {
            var bytesHandler = BytesReceived;
            if (bytesHandler != null)
            {
                var bytes = Encoding.GetEncoding(28591).GetBytes(s);
                this.PrintReceivedBytes(bytes);
                bytesHandler(this, new GenericCommMethodReceiveBytesArgs(bytes));
            }
            var textHandler = TextReceived;
            if (textHandler != null)
            {
                this.PrintReceivedText(s);
                textHandler(this, new GenericCommMethodReceiveTextArgs(s));
            }
        }

        #region IBasicCommunication Members

        /// <summary>
        /// SendText method
        /// </summary>
        public void SendText(string text)
        {
            if (Port == null)
                return;
            this.PrintSentText(text);
            Port.StreamCec.Send.StringValue = text;
        }

        /// <summary>
        /// SendBytes method
        /// </summary>
        public void SendBytes(byte[] bytes)
        {
            if (Port == null)
                return;
            var text = Encoding.GetEncoding(28591).GetString(bytes, 0, bytes.Length);
            this.PrintSentBytes(bytes);
            Debug.LogMessage(LogEventLevel.Information, this, "Sending {0} bytes: '{1}'", bytes.Length, ComTextHelper.GetEscapedText(bytes));
            Port.StreamCec.Send.StringValue = text;
        }

        /// <summary>
        /// Connect method
        /// </summary>
        public void Connect()
        {
        }

        /// <summary>
        /// Disconnect method
        /// </summary>
        public void Disconnect()
        {
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <summary>
        /// SimulateReceive method
        /// </summary>
        public void SimulateReceive(string s)
        {
            // split out hex chars and build string
            var split = Regex.Split(s, @"(\\[Xx][0-9a-fA-F][0-9a-fA-F])");
            StringBuilder b = new StringBuilder();
            foreach (var t in split)
            {
                if (t.StartsWith(@"\") && t.Length == 4)
                    b.Append((char)(Convert.ToByte(t.Substring(2, 2), 16)));
                else
                    b.Append(t);
            }

            OnDataReceived(b.ToString());
        }
    }
}