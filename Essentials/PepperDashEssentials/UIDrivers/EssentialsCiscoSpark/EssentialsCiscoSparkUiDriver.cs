using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.UIDrivers.EssentialsCiscoSpark
{

    /// <summary>
    /// This fella will likely need to interact with the room's source, although that is routed via the spark...
    /// Probably needs event or FB to feed AV driver - to show two-mute volume when appropriate.
    /// 
    /// </summary>
    public class EssentialsCiscoSparkUiDriver : PanelDriverBase
    {
        object Codec;

        /// <summary>
        /// 
        /// </summary>
        SubpageReferenceList CallStagingSrl;

        /// <summary>
        /// 
        /// </summary>
        SmartObjectDynamicList CallQuickDialList;

        /// <summary>
        /// 
        /// </summary>
        SubpageReferenceList DirectorySrl; // ***************** SRL ???


        /// <summary>
        /// To drive UI elements outside of this driver that may be dependent on this.
        /// </summary>
        BoolFeedback InCall;
        BoolFeedback LocalPrivacyIsMuted;


        public EssentialsCiscoSparkUiDriver(BasicTriListWithSmartObject triList, object codec)
            : base(triList)
        {
            Codec = codec;
            SetupCallStagingSrl();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Show()
        {
            base.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Hide()
        {
            base.Hide();
        }

        /// <summary>
        /// Builds the call stage
        /// </summary>
        void SetupCallStagingSrl()
        {
            CallStagingSrl = new SubpageReferenceList(TriList, UISmartObjectJoin.CallStagingSrl, 3, 3, 3);
            var c = CallStagingSrl;
            c.AddItem(new SubpageReferenceListButtonAndModeItem(1, c, 1, b => { if (!b) { } })); //************ Camera
            c.AddItem(new SubpageReferenceListButtonAndModeItem(2, c, 2, b => { if (!b) { } })); //************ Directory
            c.AddItem(new SubpageReferenceListButtonAndModeItem(3, c, 3, b => { if (!b) { } })); //************ Keypad
            c.AddItem(new SubpageReferenceListButtonAndModeItem(4, c, 4, b => { if (!b) { } })); //************ End Call
            c.Count = 3;
        }

        /// <summary>
        /// 
        /// </summary>
        void ShowCameraControls()
        {

        }

        void ShowKeypad()
        {

        }

        void ShowDirectory()
        {

        }

        void CallHasStarted()
        {
            // Header icon
            // Add end call button to stage
            // Volume bar needs to have mic mute
        }

        void CallHasEnded()
        {
            // Header icon
            // Remove end call
            // Volume bar no mic mute (or hidden if no source?)
        }



        public class BoolJoin
        {
            public const uint CameraControlsVisible = 3001;

            public const uint KeypadVisbile = 3002;

            public const uint DirectoryVisible = 3003;

           
        }
    }
}