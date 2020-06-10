using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	public class ComPortController : Device, IBasicCommunicationWithStreamDebugging
	{
        public CommunicationStreamDebugging StreamDebugging { get; private set; }

		public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;
		public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

		public bool IsConnected { get { return true; } }

		ComPort Port;
		ComPort.ComPortSpec Spec;

	    public ComPortController(string key, Func<EssentialsControlPropertiesConfig, ComPort> postActivationFunc,
	        ComPort.ComPortSpec spec, EssentialsControlPropertiesConfig config) : base(key)
	    {
            StreamDebugging = new CommunicationStreamDebugging(key);

	        Spec = spec;

            AddPostActivationAction(() =>
            {
                Port = postActivationFunc(config);

                RegisterAndConfigureComPort();
            });
	    }

		public ComPortController(string key, ComPort port, ComPort.ComPortSpec spec)
			: base(key)
		{
			if (port == null)
			{
				Debug.Console(0, this,  "ERROR: Invalid com port, continuing but comms will not function");
				return;
			}

			Port = port;
			Spec = spec;
			//IsConnected = new BoolFeedback(CommonBoolCue.IsConnected, () => true);

			RegisterAndConfigureComPort();
		}

	    private void RegisterAndConfigureComPort()
	    {
            if (Port.Parent is CrestronControlSystem)
            {
                var result = Port.Register();
                if (result != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    Debug.Console(0, this, "ERROR: Cannot register Com port: {0}", result);
                    return; // false
                }
            }

	        var specResult = Port.SetComPortSpec(Spec);
	        if (specResult != 0)
	        {
	            Debug.Console(0, this, "WARNING: Cannot set comspec");
	            return;
	        }
	        Port.SerialDataReceived += Port_SerialDataReceived;
	    }

	    ~ComPortController()
		{
			Port.SerialDataReceived -= Port_SerialDataReceived;
		}

		void Port_SerialDataReceived(ComPort ReceivingComPort, ComPortSerialDataEventArgs args)
		{
            OnDataReceived(args.SerialData);
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
            {
                if (StreamDebugging.RxStreamDebuggingIsEnabled)
                    Debug.Console(0, this, "Recevied: '{0}'", s);

                textHandler(this, new GenericCommMethodReceiveTextArgs(s));
            }
        }

		public override bool Deactivate()
		{
			return Port.UnRegister() == eDeviceRegistrationUnRegistrationResponse.Success;
		}

		#region IBasicCommunication Members

		public void SendText(string text)
		{
			if (Port == null)
				return;

            if (StreamDebugging.TxStreamDebuggingIsEnabled)
                Debug.Console(0, this, "Sending {0} characters of text: '{1}'", text.Length, text);
            Port.Send(text);
		}

		public void SendBytes(byte[] bytes)
		{
			if (Port == null)
				return;
			var text = Encoding.GetEncoding(28591).GetString(bytes, 0, bytes.Length);
            if (StreamDebugging.TxStreamDebuggingIsEnabled)
                Debug.Console(0, this, "Sending {0} bytes: '{1}'", bytes.Length, ComTextHelper.GetEscapedText(bytes));

			Port.Send(text);
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