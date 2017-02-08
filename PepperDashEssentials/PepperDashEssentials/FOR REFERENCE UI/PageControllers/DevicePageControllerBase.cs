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
//    public abstract class DevicePageControllerBase
//    {

//        protected BasicTriListWithSmartObject TriList;
//        protected List<BoolInputSig> FixedObjectSigs;

//        public DevicePageControllerBase(BasicTriListWithSmartObject triList)
//        {
//            TriList = triList;
//        }

//        public void SetVisible(bool state)
//        {
//            foreach (var sig in FixedObjectSigs)
//            {
//                Debug.Console(2, "set visible {0}={1}", sig.Number, state);
//                sig.BoolValue = state;
//            }
//            CustomSetVisible(state);
//        }

//        /// <summary>
//        /// Add any specialized show/hide logic here - beyond FixedObjectSigs. Overriding
//        /// methods do not need to call this base method
//        /// </summary>
//        protected virtual void CustomSetVisible(bool state)
//        {
//        }
//    }
//}