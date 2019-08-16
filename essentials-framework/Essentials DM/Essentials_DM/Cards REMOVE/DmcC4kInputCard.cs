//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
//using Crestron.SimplSharpPro.DM;
//using Crestron.SimplSharpPro.DM.Cards;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.DM;

//namespace PepperDash.Essentials.DM.Cards
//{
//    public class Dmc4kCController : DmInputCardControllerBase
//    {
//        public override eDmInputCardType Type
//        {
//            get { return eDmInputCardType.Dmc4kC; }
//        }
//        public Dmc4kC Card { get; private set; }

//        //public RoutingInputPortWithVideoStatuses DmIn { get; private set; }
//        //public RoutingOutputPort HdmiLoopOut { get; private set; }
//        //public RoutingOutputPort AudioLoopOut { get; private set; }
		
//        public Dmc4kCController(string key, Dmc4kC card, uint slot)
//            : base(key, slot)
//        {
//            Card = card;
//            //DmIn = new RoutingInputPortWithVideoStatuses(DmPortName.DmIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
//            //    eRoutingPortConnectionType.DmCat, null, this, 
//            //    VideoStatusHelper.GetDmInputStatusFuncs(Card.DmInput));

//            //HdmiLoopOut = new RoutingOutputPort(DmPortName.HdmiLoopOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
//            //    eRoutingPortConnectionType.Hdmi, null, this);
//            //AudioLoopOut = new RoutingOutputPort(DmPortName.AudioLoopOut, eRoutingSignalType.Audio,
//            //    eRoutingPortConnectionType.Hdmi, null, this);

//            //InputPorts.Add(DmIn);
//            //OutputPorts.AddRange(new[] { HdmiLoopOut, AudioLoopOut });
//        }
//    }

//    public class Dmc4kCDspController : DmInputCardControllerBase
//    {
//        public override eDmInputCardType Type
//        {
//            get { return eDmInputCardType.Dmc4kCDsp; }
//        }
//        public Dmc4kCDsp Card { get; private set; }

//        //public RoutingInputPortWithVideoStatuses DmIn { get; private set; }
//        //public RoutingOutputPort HdmiLoopOut { get; private set; }
//        //public RoutingOutputPort AudioLoopOut { get; private set; }

//        public Dmc4kCDspController(string key, Dmc4kCDsp card, uint slot)
//            : base(key, slot)
//        {
//            Card = card;
//            //DmIn = new RoutingInputPortWithVideoStatuses(DmPortName.DmIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
//            //    eRoutingPortConnectionType.DmCat, null, this,
//            //    VideoStatusHelper.GetDmInputStatusFuncs(Card.DmInput));

//            //HdmiLoopOut = new RoutingOutputPort(DmPortName.HdmiLoopOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
//            //    eRoutingPortConnectionType.Hdmi, null, this);
//            //AudioLoopOut = new RoutingOutputPort(DmPortName.AudioLoopOut, eRoutingSignalType.Audio,
//            //    eRoutingPortConnectionType.Hdmi, null, this);

//            //InputPorts.Add(DmIn);
//            //OutputPorts.AddRange(new[] { HdmiLoopOut, AudioLoopOut });
//        }
//    }

//}