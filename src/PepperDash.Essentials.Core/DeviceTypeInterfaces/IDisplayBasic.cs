using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Devices.DeviceTypeInterfaces;

	public interface IDisplayBasic
	{
		void InputHdmi1();
		void InputHdmi2();
		void InputHdmi3();
		void InputHdmi4();
		void InputDisplayPort1();
		void InputDvi1();
		void InputVideo1();
		void InputVga1();
		void InputVga2();
		void InputRgb1();
	}