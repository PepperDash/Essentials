using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.UIDrivers.VC
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
        SmartObjectDynamicList DirectorySrl; // ***************** SRL ???


        /// <summary>
        /// To drive UI elements outside of this driver that may be dependent on this.
        /// </summary>
        BoolFeedback InCall;
        BoolFeedback LocalPrivacyIsMuted;

        /// <summary>
        /// For the subpages above the bar
        /// </summary>
        JoinedSigInterlock VCControlsInterlock;

        /// <summary>
        /// For the different staging bars: Active, inactive
        /// </summary>
        JoinedSigInterlock StagingBarInterlock;

        /// <summary>
        /// For the staging button feedbacks
        /// </summary>
        JoinedSigInterlock StagingButtonFeedbackInterlock;

        SmartObjectNumeric DialKeypad;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="triList"></param>
        /// <param name="codec"></param>
        public EssentialsCiscoSparkUiDriver(BasicTriListWithSmartObject triList, object codec)
            : base(triList)
        {
            Codec = codec;
            SetupCallStagingPopover();
            SetupDialKeypad();

            InCall = new BoolFeedback(() => false);
            LocalPrivacyIsMuted = new BoolFeedback(() => false);

            //DirectorySrl = new SubpageReferenceList(triList, UISmartObjectJoin.VCDirectoryList, 3, 3, 3);

            VCControlsInterlock = new JoinedSigInterlock(triList);
            VCControlsInterlock.SetButDontShow(UIBoolJoin.VCRecentsVisible);

            StagingBarInterlock = new JoinedSigInterlock(triList);
            StagingBarInterlock.SetButDontShow(UIBoolJoin.VCStagingInactivePopoverVisible);

            StagingButtonFeedbackInterlock = new JoinedSigInterlock(triList);
            StagingButtonFeedbackInterlock.ShowInterlocked(UIBoolJoin.VCRecentsVisible);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Show()
        {
            VCControlsInterlock.Show();
            StagingBarInterlock.Show();
            base.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Hide()
        {
            VCControlsInterlock.Hide();
            StagingBarInterlock.Hide();
            base.Hide();
        }

        /// <summary>
        /// Builds the call stage
        /// </summary>
        void SetupCallStagingPopover()
        {
            TriList.SetSigFalseAction(UIBoolJoin.VCStagingDirectoryPress, ShowDirectory);
            TriList.SetSigFalseAction(UIBoolJoin.VCStagingConnectPress, () => { });
            TriList.SetSigFalseAction(UIBoolJoin.VCStagingKeypadPress, ShowKeypad);
            TriList.SetSigFalseAction(UIBoolJoin.VCStagingRecentsPress, ShowRecents);
        }

        /// <summary>
        /// 
        /// </summary>
        void SetupDialKeypad()
        {
            if(TriList.SmartObjects.Contains(UISmartObjectJoin.VCDialKeypad))
            {
                DialKeypad = new SmartObjectNumeric(TriList.SmartObjects[UISmartObjectJoin.VCDialKeypad], true);
                DialKeypad.Digit0.SetBoolSigAction(b => ___DialPlaceholder___(0));
                DialKeypad.Digit1.SetBoolSigAction(b => ___DialPlaceholder___(1));
                DialKeypad.Digit2.SetBoolSigAction(b => ___DialPlaceholder___(2));
                DialKeypad.Digit3.SetBoolSigAction(b => ___DialPlaceholder___(3));
                DialKeypad.Digit4.SetBoolSigAction(b => ___DialPlaceholder___(4));
                DialKeypad.Digit5.SetBoolSigAction(b => ___DialPlaceholder___(5));
                DialKeypad.Digit6.SetBoolSigAction(b => ___DialPlaceholder___(6));
                DialKeypad.Digit7.SetBoolSigAction(b => ___DialPlaceholder___(7));
                DialKeypad.Digit8.SetBoolSigAction(b => ___DialPlaceholder___(8));
                DialKeypad.Digit9.SetBoolSigAction(b => ___DialPlaceholder___(9));
                DialKeypad.Misc1SigName = "*";
                DialKeypad.Misc1.SetBoolSigAction(b => { });
                DialKeypad.Misc2SigName = "#";
                DialKeypad.Misc2.SetBoolSigAction(b => { });
            }
            else
                Debug.Console(0, "Trilist {0:x2}, VC dial keypad object {1} not found. Check SGD file or VTP",
                    TriList.ID, UISmartObjectJoin.VCDialKeypad);
        }

        /// <summary>
        /// 
        /// </summary>
        void ShowCameraControls()
        {
            VCControlsInterlock.ShowInterlocked(UIBoolJoin.VCCameraVisible);
            StagingButtonFeedbackInterlock.ShowInterlocked(UIBoolJoin.VCStagingCameraPress);
        }

        void ShowKeypad()
        {
            VCControlsInterlock.ShowInterlocked(UIBoolJoin.VCKeypadVisible);
            StagingButtonFeedbackInterlock.ShowInterlocked(UIBoolJoin.VCStagingKeypadPress);
        }

        void ShowDirectory()
        {
            // populate directory
            VCControlsInterlock.ShowInterlocked(UIBoolJoin.VCDirectoryVisible);
            StagingButtonFeedbackInterlock.ShowInterlocked(UIBoolJoin.VCStagingDirectoryPress);
        }

        void ShowRecents()
        {
            //populate recents
            VCControlsInterlock.ShowInterlocked(UIBoolJoin.VCDirectoryVisible);
            StagingButtonFeedbackInterlock.ShowInterlocked(UIBoolJoin.VCStagingRecentsPress);
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

        void ___DialPlaceholder___(int i)
        {
            throw new NotImplementedException();
        }
    }
}