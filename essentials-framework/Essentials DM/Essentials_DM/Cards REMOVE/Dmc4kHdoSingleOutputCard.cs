//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
//using Crestron.SimplSharpPro.DM;
//using Crestron.SimplSharpPro.DM.Cards;

//using PepperDash.Core;
//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.DM;

//namespace PepperDash.Essentials.DM.Cards
//{
//    public class Dmc4kHdoSingleOutputCard : DmSingleOutputCardControllerBase
//    {
//        public override eDmOutputCardType Type
//        {
//            get { return eDmOutputCardType.Dmc4kHdo; }
//        }
//        public Dmc4kHdoSingle Card { get; private set; }

//        //public RoutingOutputPort AudioOut1 { get; private set; }
//        //public RoutingOutputPort AudioOut2 { get; private set; }
//        //public RoutingOutputPort HdmiOut1 { get; private set; }
//        //public RoutingOutputPort HdmiOut2 { get; private set; }

//        public Dmc4kHdoSingleOutputCard(string key, Dmc4kHdoSingle card, uint slot)
//            : base(key, slot)
//        {
//            Card = card;
//            //AudioOut1 = new RoutingOutputPort(DmPortName.BalancedAudioOut1, eRoutingSignalType.Audio,
//            //    eRoutingPortConnectionType.LineAudio, null, this);
//            //AudioOut2 = new RoutingOutputPort(DmPortName.BalancedAudioOut2, eRoutingSignalType.Audio,
//            //    eRoutingPortConnectionType.LineAudio, null, this);
//            //HdmiOut1 = new RoutingOutputPort(DmPortName.HdmiOut1, eRoutingSignalType.Audio | eRoutingSignalType.Video,
//            //    eRoutingPortConnectionType.Hdmi, null, this);
//            //HdmiOut2 = new RoutingOutputPort(DmPortName.HdmiOut2, eRoutingSignalType.Audio | eRoutingSignalType.Video,
//            //    eRoutingPortConnectionType.Hdmi, null, this);

//            //OutputPorts.AddRange(new[] { AudioOut1, AudioOut2, HdmiOut1, HdmiOut2 });
//        }
//    }
//}