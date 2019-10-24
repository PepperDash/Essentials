//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;

//using Newtonsoft.Json;

//using PepperDash.Core;
//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Core.Devices;
//using PepperDash.Essentials.Core.Config;
//using PepperDash.Essentials.Room.Config;
//using PepperDash.Essentials.Devices.Common.Codec;
//using PepperDash.Essentials.Devices.Common.VideoCodec;
//using PepperDash.Essentials.Devices.Common.AudioCodec;

//namespace PepperDash.Essentials.Room.Types
//{
//    public class EssentialsDualDisplayRoom : EssentialsNDisplayRoomBase, IHasCurrentVolumeControls, 
//        IRunRouteAction, IPrivacy, IRunDefaultCallRoute, IHasVideoCodec, IHasAudioCodec
//    {
//        public event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;

//        public EssentialsHuddleVtc1PropertiesConfig PropertiesConfig { get; private set; }

//        //************************
//        // Call-related stuff

//        public BoolFeedback InCallFeedback { get; private set; }

//        /// <summary>
//        /// States: 0 for on hook, 1 for video, 2 for audio, 3 for telekenesis
//        /// </summary>
//        public IntFeedback CallTypeFeedback { get; private set; }

//        /// <summary>
//        /// 
//        /// </summary>
//        public BoolFeedback PrivacyModeIsOnFeedback { get; private set; }

//        /// <summary>
//        /// When something in the room is sharing with the far end or through other means
//        /// </summary>
//        public BoolFeedback IsSharingFeedback { get; private set; }

//        IRoutingSinkWithSwitching LeftDisplay { get; private set; }
//        IRoutingSinkWithSwitching RightDisplay { get; private set; }


//        protected override Func<bool> OnFeedbackFunc
//        {
//            get
//            {
//                return () =>
//                {
//                    var leftDisp = LeftDisplay as DisplayBase;
//                    var rightDisp = RightDisplay as DisplayBase;
//                    var val = leftDisp != null && leftDisp.CurrentSourceInfo != null
//                        && leftDisp.CurrentSourceInfo.Type == eSourceListItemType.Route
//                        && rightDisp != null && rightDisp.CurrentSourceInfo != null
//                        && rightDisp.CurrentSourceInfo.Type == eSourceListItemType.Route;
//                    return val;
//                };
//            }
//        }
//    }
//}