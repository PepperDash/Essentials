using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;
using Crestron.SimplSharpPro.UI;

namespace PepperDash.Essentials.Core
{
	//[Obsolete]
	//public class PresentationDeviceType
	//{
	//    public const ushort Default = 1;
	//    public const ushort CableSetTopBox = 2;
	//    public const ushort SatelliteSetTopBox = 3;
	//    public const ushort Dvd = 4;
	//    public const ushort Bluray = 5;
	//    public const ushort PC = 9;
	//    public const ushort Laptop = 10;
	//}

	public enum PresentationSourceType
	{
		None, Dvd, Laptop, PC, SetTopBox, VCR
	}
}