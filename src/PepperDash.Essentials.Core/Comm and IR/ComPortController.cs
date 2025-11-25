using System;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Core.Logging;
using Serilog.Events;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Represents a ComPortController
	/// </summary>
	public class ComPortController : Device, IBasicCommunicationWithStreamDebugging
	{
		/// <summary>
		/// Gets or sets the StreamDebugging
		/// </summary>
		public CommunicationStreamDebugging StreamDebugging { get; private set; }

		/// <summary>
		/// Event fired when bytes are received
		/// </summary>
		public event EventHandler<GenericCommMethodReceiveBytesArgs> BytesReceived;

		/// <summary>
		/// Event fired when text is received
		/// </summary>
		public event EventHandler<GenericCommMethodReceiveTextArgs> TextReceived;

		/// <summary>
		/// Gets or sets the IsConnected
		/// </summary>
		public bool IsConnected { get { return true; } }

		ComPort Port;
		ComPort.ComPortSpec Spec;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key"></param>
		/// <param name="postActivationFunc"></param>
		/// <param name="spec"></param>
		/// <param name="config"></param>
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

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">Device key</param>
		/// <param name="port">COM port instance</param>
		/// <param name="spec">COM port specification</param>
		public ComPortController(string key, ComPort port, ComPort.ComPortSpec spec)
			: base(key)
		{
			if (port == null)
			{
				Debug.LogMessage(LogEventLevel.Information, this, "ERROR: Invalid com port, continuing but comms will not function");
				return;
			}

			Port = port;
			Spec = spec;
			//IsConnected = new BoolFeedback(CommonBoolCue.IsConnected, () => true);

			RegisterAndConfigureComPort();
		}

		private void RegisterAndConfigureComPort()
		{
			if (Port == null)
			{
				this.LogInformation($"Configured {Port.Parent.GetType().Name}-comport-{Port.ID} for {Key} does not exist.");
				return;
			}


			if (Port.Parent is CrestronControlSystem || Port.Parent is CenIoCom102)
			{
				var result = Port.Register();
				if (result != eDeviceRegistrationUnRegistrationResponse.Success)
				{
					this.LogError($"Cannot register {Key} using {Port.Parent.GetType().Name}-comport-{Port.ID} (result == {result})");
					return;
				}
				this.LogInformation($"Successfully registered {Key} using {Port.Parent.GetType().Name}-comport-{Port.ID} (result == {result})");
			}

			var specResult = Port.SetComPortSpec(Spec);
			if (specResult != 0)
			{
				this.LogError($"Cannot set comspec for {Key} using {Port.Parent.GetType().Name}-comport-{Port.ID} (result == {specResult})");
				return;
			}
			this.LogInformation($"Successfully set comspec for {Key} using {Port.Parent.GetType().Name}-comport-{Port.ID} (result == {specResult})");

			Port.SerialDataReceived += Port_SerialDataReceived;
		}

		/// <summary>
		/// Destructor
		/// </summary>
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
			var eventSubscribed = false;

			var bytesHandler = BytesReceived;
			if (bytesHandler != null)
			{
				var bytes = Encoding.GetEncoding(28591).GetBytes(s);
				this.PrintReceivedBytes(bytes);
				bytesHandler(this, new GenericCommMethodReceiveBytesArgs(bytes));
				eventSubscribed = true;
			}
			var textHandler = TextReceived;
			if (textHandler != null)
			{
				this.PrintReceivedText(s);
				textHandler(this, new GenericCommMethodReceiveTextArgs(s));
				eventSubscribed = true;
			}

			if (!eventSubscribed) Debug.LogMessage(LogEventLevel.Warning, this, "Received data but no handler is registered");
		}

		/// <summary>
		/// Deactivate method
		/// </summary>
		/// <inheritdoc />
		public override bool Deactivate()
		{
			return Port.UnRegister() == eDeviceRegistrationUnRegistrationResponse.Success;
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
			Port.Send(text);
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

			Port.Send(text);
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