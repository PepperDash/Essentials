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

namespace PepperDash.Essentials.Core;

	public class CecPortController : Device, IBasicCommunicationWithStreamDebugging
{
		public CommunicationStreamDebugging StreamDebugging { get; private set; }

    public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;
    public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

    public bool IsConnected { get { return true; } }

    ICec Port;

    bool _cecSubscribed;

    public CecPortController(string key, Func<EssentialsControlPropertiesConfig, ICec> postActivationFunc,
        EssentialsControlPropertiesConfig config):base(key)
    {
			StreamDebugging = new CommunicationStreamDebugging(key);

        AddPostActivationAction(() =>
        {
            Port = postActivationFunc(config);

            TryEnsureCecSubscription();
        });            
    }

    public CecPortController(string key, ICec port)
        : base(key)
    {
        Port = port;

        TryEnsureCecSubscription();
    }

    /// <summary>
    /// Subscribes to the CEC change event once <see cref="ICec.StreamCec"/> is available.
    /// Safe to call repeatedly; the subscription is only wired a single time.
    /// If StreamCec is null during construction, this is retried when send methods invoke
    /// this method later.
    /// </summary>
    void TryEnsureCecSubscription()
    {
        if (_cecSubscribed)
            return;

        if (Port?.StreamCec == null)
        {
            Debug.LogMessage(LogEventLevel.Warning, this, "StreamCec is not available; CEC feedback is deferred until the device is ready");
            return;
        }

        Port.StreamCec.CecChange += new CecChangeEventHandler(StreamCec_CecChange);
        _cecSubscribed = true;
        Debug.LogMessage(LogEventLevel.Information, this, "Subscribed to CEC feedback");
    }

    void StreamCec_CecChange(Cec cecDevice, CecEventArgs args)
    {
        if (args.EventId == CecEventIds.CecMessageReceivedEventId)
            OnDataReceived(cecDevice.Received.StringValue);
        else if (args.EventId == CecEventIds.ErrorFeedbackEventId)
            if(cecDevice.ErrorFeedback.BoolValue)
                Debug.LogMessage(LogEventLevel.Verbose, this, "CEC NAK Error");
    }

    void OnDataReceived(string s)
    {
			var bytesHandler = BytesReceived;
        if (bytesHandler != null)
        {
            var bytes = Encoding.GetEncoding(28591).GetBytes(s);
				if (StreamDebugging.RxStreamDebuggingIsEnabled)
					Debug.LogMessage(LogEventLevel.Information, this, "Received: '{0}'", ComTextHelper.GetEscapedText(bytes));
            bytesHandler(this, new GenericCommMethodReceiveBytesArgs(bytes));
        }
        var textHandler = TextReceived;
			if (textHandler != null)
			{
				if (StreamDebugging.RxStreamDebuggingIsEnabled)
					Debug.LogMessage(LogEventLevel.Information, this, "Received: '{0}'", s);
				textHandler(this, new GenericCommMethodReceiveTextArgs(s));
			}
    }

    #region IBasicCommunication Members

    public void SendText(string text)
    {
        if (Port?.StreamCec == null)
            return;
        TryEnsureCecSubscription();
			if (StreamDebugging.TxStreamDebuggingIsEnabled)
				Debug.LogMessage(LogEventLevel.Information, this, "Sending {0} characters of text: '{1}'", text.Length, text);
        Port.StreamCec.Send.StringValue = text;
    }

    public void SendBytes(byte[] bytes)
    {
        if (Port?.StreamCec == null)
            return;
        TryEnsureCecSubscription();
        var text = Encoding.GetEncoding(28591).GetString(bytes, 0, bytes.Length);
			if (StreamDebugging.TxStreamDebuggingIsEnabled)
				Debug.LogMessage(LogEventLevel.Information, this, "Sending {0} bytes: '{1}'", bytes.Length, ComTextHelper.GetEscapedText(bytes));
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