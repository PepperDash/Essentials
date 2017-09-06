//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
//using Crestron.SimplSharpPro.UI;

//using PepperDash.Essentials.Core;

//namespace PepperDash.Essentials
//{
//    public class DualDisplaySourceSRLController : SubpageReferenceList
//    {
//        public DualDisplaySourceSRLController(BasicTriListWithSmartObject triList,
//            uint smartObjectId, EssentialsPresentationRoom room)
//            : base(triList, smartObjectId, 3, 3, 3)
//        {
//            var srcList = room.s items.Values.ToList().OrderBy(s => s.Order);
//            foreach (var item in srcList)
//            {
//                GetBoolFeedbackSig(index, 1).UserObject = new Action<bool>(routeAction);

//            }
//        }
//    }
//}