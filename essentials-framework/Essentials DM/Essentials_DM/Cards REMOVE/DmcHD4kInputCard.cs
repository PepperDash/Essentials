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
//    /// <summary>
//    /// 
//    /// </summary>
//    public class Dmc4kHdController : DmInputCardControllerBase
//    {
//        public Dmc4kHd Card { get; private set; }
//        public override eDmInputCardType Type
//        {
//            get { return eDmInputCardType.Dmc4kHd; }
//        }

//        public RoutingInputPortWithVideoStatuses HdmiIn { get; private set; }
//        public RoutingOutputPort HdmiLoopOut { get; private set; }
//        public RoutingOutputPort AudioLoopOut { get; private set; }

//        public Dmc4kHdController(string key, Dmc4kHd card, uint slot)
//            : base(key, slot)
//        {
//            Card = card;
//            HdmiIn = new RoutingInputPortWithVideoStatuses(DmPortName.HdmiIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
//                eRoutingPortConnectionType.Hdmi, null, this,
//                VideoStatusHelper.GetHdmiInputStatusFuncs(Card.HdmiInput));

//            HdmiLoopOut = new RoutingOutputPort(DmPortName.HdmiLoopOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
//                eRoutingPortConnectionType.Hdmi, null, this);
//            AudioLoopOut = new RoutingOutputPort(DmPortName.AudioLoopOut, eRoutingSignalType.Audio,
//                eRoutingPortConnectionType.Hdmi, null, this);

//            InputPorts.Add(HdmiIn);
//            OutputPorts.AddRange(new[] { HdmiLoopOut, AudioLoopOut });
//        }
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    public class Dmc4kHdDspController : DmInputCardControllerBase
//    {
//        public Dmc4kHdDsp Card { get; private set; }
//        public override eDmInputCardType Type
//        {
//            get { return eDmInputCardType.Dmc4kHdDsp; }
//        }

//        //public RoutingInputPortWithVideoStatuses HdmiIn { get; private set; }
//        //public RoutingOutputPort HdmiLoopOut { get; private set; }
//        //public RoutingOutputPort AudioLoopOut { get; private set; }

//        public Dmc4kHdDspController(string key, Dmc4kHdDsp card, uint slot)
//            : base(key, slot)
//        {
//            Card = card;
//            //HdmiIn = new RoutingInputPortWithVideoStatuses(DmPortName.HdmiIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
//            //    eRoutingPortConnectionType.Hdmi, null, this,
//            //    VideoStatusHelper.GetHdmiInputStatusFuncs(Card.HdmiInput));

//            //HdmiLoopOut = new RoutingOutputPort(DmPortName.HdmiLoopOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
//            //    eRoutingPortConnectionType.Hdmi, null, this);
//            //AudioLoopOut = new RoutingOutputPort(DmPortName.AudioLoopOut, eRoutingSignalType.Audio,
//            //    eRoutingPortConnectionType.Hdmi, null, this);

//            //InputPorts.Add(HdmiIn);
//            //OutputPorts.AddRange(new[] { HdmiLoopOut, AudioLoopOut });
//        }
//    }
//}

