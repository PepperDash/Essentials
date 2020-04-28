using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public class CecPortController : Device, IBasicCommunication
    {
        public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;
        public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

        public bool IsConnected { get { return true; } }

        ICec Port;

        public CecPortController(string key, Func<EssentialsControlPropertiesConfig, ICec> postActivationFunc,
            EssentialsControlPropertiesConfig config):base(key)
        {
            AddPostActivationAction(() =>
            {
                Port = postActivationFunc(config);

                Port.StreamCec.CecChange += StreamCec_CecChange;
            });            
        }

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
                if(cecDevice.ErrorFeedback.BoolValue)
                    Debug.Console(2, this, "CEC NAK Error");
        }

        void OnDataReceived(string s)
        {
            var bytesHandler = BytesReceived;
            if (bytesHandler != null)
            {
                var bytes = Encoding.GetEncoding(28591).GetBytes(s);
                bytesHandler(this, new GenericCommMethodReceiveBytesArgs(bytes));
            }
            var textHandler = TextReceived;
            if (textHandler != null)
                textHandler(this, new GenericCommMethodReceiveTextArgs(s));
        }

        #region IBasicCommunication Members

        public void SendText(string text)
        {
            if (Port == null)
                return;
            Port.StreamCec.Send.StringValue = text;
        }

        public void SendBytes(byte[] bytes)
        {
            if (Port == null)
                return;
            var text = Encoding.GetEncoding(28591).GetString(bytes, 0, bytes.Length);
            Port.StreamCec.Send.StringValue = text;
        }

        public void Connect()
        {
        }

        public void Disconnect()
        {
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
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