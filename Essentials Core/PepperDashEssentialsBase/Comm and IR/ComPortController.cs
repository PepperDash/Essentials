using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	public class ComPortController : Device, IBasicCommunication
	{
		public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;
		public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

		public bool IsConnected { get { return true; } }

		ComPort Port;
		ComPort.ComPortSpec Spec;

		public ComPortController(string key, ComPort port, ComPort.ComPortSpec spec)
			: base(key)
		{
			Port = port;
			Spec = spec;
			//IsConnected = new BoolFeedback(CommonBoolCue.IsConnected, () => true);

			if (Port.Parent is CrestronControlSystem)
			{
				var result = Port.Register();
				if (result != eDeviceRegistrationUnRegistrationResponse.Success)
				{
					Debug.Console(0, this, "WARNING: Cannot register Com port: {0}", result);
					return; // false
				}
			}
			var specResult = Port.SetComPortSpec(Spec);
			if (specResult != 0)
			{
				Debug.Console(0, this, "WARNING: Cannot set comspec");
				return; // false
			}
			Port.SerialDataReceived += new ComPortDataReceivedEvent(Port_SerialDataReceived);
		}

		~ComPortController()
		{
			Port.SerialDataReceived -= Port_SerialDataReceived;
		}

		void Port_SerialDataReceived(ComPort ReceivingComPort, ComPortSerialDataEventArgs args)
		{
			var bytesHandler = BytesReceived;
			if (bytesHandler != null)
			{
				var bytes = Encoding.GetEncoding(28591).GetBytes(args.SerialData);
				bytesHandler(this, new GenericCommMethodReceiveBytesArgs(bytes));
			}
			var textHandler = TextReceived;
			if (textHandler != null)
				textHandler(this, new GenericCommMethodReceiveTextArgs(args.SerialData));
		}

		public override bool Deactivate()
		{
			return Port.UnRegister() == eDeviceRegistrationUnRegistrationResponse.Success;
		}

		#region IBasicCommunication Members

		public void SendText(string text)
		{
			Port.Send(text);
		}

		public void SendBytes(byte[] bytes)
		{
			var text = Encoding.GetEncoding(28591).GetString(bytes, 0, bytes.Length);
			Port.Send(text);
		}

		//public BoolFeedback IsConnected { get; private set; }

		public void Connect()
		{		
		}

		public void Disconnect()
		{
		}

		#endregion
	}
}