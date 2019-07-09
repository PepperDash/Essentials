using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common
{
	/// <summary>
	/// 
	/// </summary>
	public class DigitalLoggerPropertiesConfig
	{
		public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

		public ControlPropertiesConfig Control { get; set; }
		public string userName { get; set; }
		public string password { get; set; }
		public string address { get; set; }
	}

}