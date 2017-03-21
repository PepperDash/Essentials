using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM;

namespace PepperDash.Essentials.DM.Cards
{
	public class Dmc4kCoHdSingleOutputCard : DmSingleOutputCardControllerBase
	{
		public override eDmOutputCardType Type
		{
			get { return eDmOutputCardType.Dmc4kCoHd; }
		}
		public Dmc4kCoHdSingle Card { get; private set; }

        //public RoutingOutputPort DmOut1 { get; private set; }
        //public RoutingOutputPort DmOut2 { get; private set; }
        //public RoutingOutputPort HdmiOut1 { get; private set; }

		public Dmc4kCoHdSingleOutputCard(string key, Dmc4kCoHdSingle card, uint slot)
			: base(key, slot)
		{
			Card = card;
            //DmOut1 = new RoutingOutputPort(DmPortName.DmOut1, eRoutingSignalType.AudioVideo,
            //    eRoutingPortConnectionType.DmCat, null, this);
            //DmOut2 = new RoutingOutputPort(DmPortName.DmOut2, eRoutingSignalType.AudioVideo,
            //    eRoutingPortConnectionType.DmCat, null, this);
            //HdmiOut1 = new RoutingOutputPort(DmPortName.HdmiOut1, eRoutingSignalType.AudioVideo,
            //    eRoutingPortConnectionType.Hdmi, null, this);

            //OutputPorts.AddRange(new[] { DmOut1, DmOut2, HdmiOut1 });
		}
	}
}