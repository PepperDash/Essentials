//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;

//using PepperDash.Core;
//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Core.Presets;

//namespace PepperDash.Essentials
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public class PageControllerLargeDvd : DevicePageControllerBase
//    {
//        IDiscPlayerControls Device;

//        public PageControllerLargeDvd(BasicTriListWithSmartObject tl, IDiscPlayerControls device)
//            : base(tl)
//        {

//            Device = device;
//            FixedObjectSigs = new List<BoolInputSig>
//            {
//                tl.BooleanInput[10093],		// well
//                tl.BooleanInput[10411],		// DVD Dpad
//                tl.BooleanInput[10412]		// everything else
//            };
//        }

//        protected override void CustomSetVisible(bool state)
//        {
//            // Hook up smart objects if applicable
//            if (Device != null)
//            {
//#warning rewire this
//                //var uos = (Device as IHasCueActionList).CueActionList;
//                //SmartObjectHelper.LinkDpadWithUserObjects(TriList, 10411, uos, state);
//            }
//        }
//    }
//}