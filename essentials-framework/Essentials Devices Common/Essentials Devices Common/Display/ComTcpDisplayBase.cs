using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using PepperDash.Core;
using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.Devices.Displays
{
	public abstract class ComTcpDisplayBase : DisplayBase, IPower
	{
		/// <summary>
		/// Sets the communication method for this - swaps out event handlers and output handlers
		/// </summary>
		public IBasicCommunication CommunicationMethod
		{
			get { return _CommunicationMethod; }
			set
			{
				if (_CommunicationMethod != null)
					_CommunicationMethod.BytesReceived -= this.CommunicationMethod_BytesReceived;
				// Outputs???
				_CommunicationMethod = value;
				if (_CommunicationMethod != null)
					_CommunicationMethod.BytesReceived += this.CommunicationMethod_BytesReceived;
				// Outputs?
			}
		}
		IBasicCommunication _CommunicationMethod;

		public ComTcpDisplayBase(string key, string name)
			: base(key, name)
		{

		}


		protected abstract void CommunicationMethod_BytesReceived(object sender, GenericCommMethodReceiveBytesArgs args);
	}
}