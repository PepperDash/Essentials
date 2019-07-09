using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
	public class ComPortController : Device, IBasicCommunication
	{
		public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;
		public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

		ComPort Port;
		ComPort.ComPortSpec Spec;
		
		public ComPortController(string key, IComPorts ComDevice, uint comPortNum, ComPort.ComPortSpec spec)
			: base(key)
		{
			Port = ComDevice.ComPorts[comPortNum];
			Spec = spec;

			Debug.Console(2, "Creating com port '{0}'", key);
			Debug.Console(2, "Com port spec:\r{0}", JsonConvert.SerializeObject(spec));
		}

		/// <summary>
		/// Creates a ComPort if the parameters are correct. Returns and logs errors if not
		/// </summary>
		public static ComPortController GetComPortController(string key, 
			IComPorts comDevice, uint comPortNum, ComPort.ComPortSpec spec)
		{
			Debug.Console(1, "Creating com port '{0}'", key);
			if (comDevice == null)
				throw new ArgumentNullException("comDevice");
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");
			if (comPortNum > comDevice.NumberOfComPorts)
			{
				Debug.Console(0, "[{0}] Com port {1} out of range on {2}",
					key, comPortNum, comDevice.GetType().Name);
				return null;
			}
			var port = new ComPortController(key, comDevice, comPortNum, spec);
			return port;
		}

		/// <summary>
		/// Registers port and sends ComSpec
		/// </summary>
		/// <returns>false if either register or comspec fails</returns>
		public override bool CustomActivate()
		{
			var result = Port.Register();
			if (result != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Debug.Console(0, this, "Cannot register Com port: {0}", result);
				return false;
			}
			var specResult = Port.SetComPortSpec(Spec);
			if (specResult != 0)
			{
				Debug.Console(0, this, "Cannot set comspec");
				return false;
			}
			Port.SerialDataReceived += new ComPortDataReceivedEvent(Port_SerialDataReceived);
			return true;
		}

		void Port_SerialDataReceived(ComPort ReceivingComPort, ComPortSerialDataEventArgs args)
		{
			if (BytesReceived != null)
			{
				var bytes = Encoding.GetEncoding(28591).GetBytes(args.SerialData);
				BytesReceived(this, new GenericCommMethodReceiveBytesArgs(bytes));
			}
			if(TextReceived != null)
				TextReceived(this, new GenericCommMethodReceiveTextArgs(args.SerialData));
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

		#endregion
	}
}