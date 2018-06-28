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
//    public class PageControllerLaptop : DevicePageControllerBase
//    {
//        public PageControllerLaptop(BasicTriListWithSmartObject tl)
//            : base(tl)
//        {
//            FixedObjectSigs = new List<BoolInputSig> 
//            {
//                tl.BooleanInput[10092], // well
//                tl.BooleanInput[11001] // Laptop info
//            };
//        }
//    }
//}