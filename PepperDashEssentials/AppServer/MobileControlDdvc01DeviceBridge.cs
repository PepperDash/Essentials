using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.EthernetCommunication;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl
{
	/// <summary>
	/// Represents a generic device connection through to and EISC for DDVC01
	/// </summary>
	public class MobileControlDdvc01DeviceBridge : Device, IChannel, INumericKeypad 
	{
		/// <summary>
		/// EISC used to talk to Simpl
		/// </summary>
		ThreeSeriesTcpIpEthernetIntersystemCommunications EISC;

		public MobileControlDdvc01DeviceBridge(string key, string name, ThreeSeriesTcpIpEthernetIntersystemCommunications eisc)
			: base(key, name)
		{
			EISC = eisc;
		}


		#region IChannel Members

		public void ChannelUp(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void ChannelDown(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void LastChannel(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void Guide(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void Info(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void Exit(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		#endregion

		#region INumericKeypad Members

		public void Digit0(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void Digit1(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void Digit2(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void Digit3(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void Digit4(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void Digit5(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void Digit6(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void Digit7(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void Digit8(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public void Digit9(bool pressRelease)
		{
			EISC.SetBool(1111, pressRelease);
		}

		public bool HasKeypadAccessoryButton1
		{
			get { throw new NotImplementedException(); }
		}

		public string KeypadAccessoryButton1Label
		{
			get { throw new NotImplementedException(); }
		}

		public void KeypadAccessoryButton1(bool pressRelease)
		{
			throw new NotImplementedException();
		}

		public bool HasKeypadAccessoryButton2
		{
			get { throw new NotImplementedException(); }
		}

		public string KeypadAccessoryButton2Label
		{
			get { throw new NotImplementedException(); }
		}

		public void KeypadAccessoryButton2(bool pressRelease)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}