using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core;
using Crestron.SimplSharpPro.ThreeSeriesCards;

namespace PepperDash.Essentials.Core.CrestronIO.Cards
{
	public class C3Com3Controller : CrestronGenericBaseDevice, IComPorts
    {
		C3com3 Card;
		public C3Com3Controller(string key, string name, C3com3 card)
			: base(key, key, card)
		{
			Card = card;
		}

		public CrestronCollection<ComPort> ComPorts
		{
			get { return Card.ComPorts; }
		}

		public int NumberOfComPorts
		{
			get { return Card.NumberOfComPorts; }
		}

	}
}