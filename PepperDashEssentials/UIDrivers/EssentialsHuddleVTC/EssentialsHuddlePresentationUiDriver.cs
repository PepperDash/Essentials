//using System;
//using System.Linq;
//using System.Collections.Generic;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
//using Crestron.SimplSharpPro.UI;

//using PepperDash.Core;
//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Core.SmartObjects;
//using PepperDash.Essentials.Core.PageManagers;
//using PepperDash.Essentials.Room.Config;

//namespace PepperDash.Essentials
//{
//    public class EssentialsHuddleVtc1PresentationUiDriver : PanelDriverBase
//    {
//        /// <summary>
//        /// 
//        /// </summary>
//        EssentialsHuddleVtc1Room CurrentRoom;


//        public EssentialsHuddleVtc1PresentationUiDriver(BasicTriListWithSmartObject triList, 
//            EssentialsHuddleVtc1Room room)
//            : base(triList)
//        {
//            CurrentRoom = room;
//        }

//        /// <summary>
//        /// Smart Object 3200
//        /// </summary>
//        SubpageReferenceList SourceStagingSrl;

//        /// <summary>
//        /// The AV page mangagers that have been used, to keep them alive for later
//        /// </summary>
//        Dictionary<object, PageManager> PageManagers = new Dictionary<object, PageManager>();

//        /// <summary>
//        /// Current page manager running for a source
//        /// </summary>
//        PageManager CurrentSourcePageManager;


//    }
//}