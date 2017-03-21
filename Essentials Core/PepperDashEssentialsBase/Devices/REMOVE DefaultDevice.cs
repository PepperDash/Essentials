//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
//using Crestron.SimplSharpPro.EthernetCommunication;
//using Crestron.SimplSharpPro.UI;

//using PepperDash.Core;


//namespace PepperDash.Essentials.Core
//{

//[Obsolete]
//    public class PresentationDevice : Device, IPresentationSource
//    {
//        public PresentationSourceType Type { get; protected set; }
//        public string IconName { get { return "Blank"; } set { } }
//        public BoolFeedback HasPowerOnFeedback { get; protected set; }

//        PresentationDevice()
//            : base("Default", "Default")
//        {
//            HasPowerOnFeedback = new BoolFeedback(CommonBoolCue.HasPowerFeedback, () => false);
//            Type = PresentationSourceType.None;
//        }

//        /// <summary>
//        /// Returns a "default" presentation device, with no abilities.
//        /// </summary>
//        public static IPresentationSource Default
//        {
//            get
//            {
//                if (_Default == null)
//                    _Default = new PresentationDevice();
//                return _Default;
//            }
//        }
//        static IPresentationSource _Default;
//    }
//}