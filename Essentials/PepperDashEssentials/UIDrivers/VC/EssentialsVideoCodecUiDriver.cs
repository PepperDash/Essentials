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
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.UIDrivers.VC
{

    /// <summary>
    /// This fella will likely need to interact with the room's source, although that is routed via the spark...
    /// Probably needs event or FB to feed AV driver - to show two-mute volume when appropriate.
    /// 
    /// </summary>
    public class EssentialsVideoCodecUiDriver : PanelDriverBase
    {
        /// <summary>
        /// 
        /// </summary>
        VideoCodecBase Codec;

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

        // These are likely temp until we get a keyboard built
        StringFeedback DialStringFeedback;
        StringBuilder DialStringBuilder = new StringBuilder();
        BoolFeedback DialStringBackspaceVisibleFeedback;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="triList"></param>
        /// <param name="codec"></param>
        public EssentialsVideoCodecUiDriver(BasicTriListWithSmartObject triList, VideoCodecBase codec)
            : base(triList)
        {
            Codec = codec;
            SetupCallStagingPopover();
            SetupDialKeypad();

            InCall = new BoolFeedback(() => false);
            LocalPrivacyIsMuted = new BoolFeedback(() => false);

            //DirectorySrl = new SubpageReferenceList(triList, UISmartObjectJoin.VCDirectoryList, 3, 3, 3);

            VCControlsInterlock = new JoinedSigInterlock(triList);
            VCControlsInterlock.SetButDontShow(UIBoolJoin.VCDirectoryVisible);

            StagingBarInterlock = new JoinedSigInterlock(triList);
            StagingBarInterlock.SetButDontShow(UIBoolJoin.VCStagingInactivePopoverVisible);

            StagingButtonFeedbackInterlock = new JoinedSigInterlock(triList);
            StagingButtonFeedbackInterlock.ShowInterlocked(UIBoolJoin.VCRecentsVisible);

            DialStringFeedback = new StringFeedback(() => DialStringBuilder.ToString());
            DialStringFeedback.LinkInputSig(triList.StringInput[UIStringJoin.KeyboardText]);

            DialStringBackspaceVisibleFeedback = new BoolFeedback(() => DialStringBuilder.Length > 0);
            DialStringBackspaceVisibleFeedback
                .LinkInputSig(TriList.BooleanInput[UIBoolJoin.KeyboardClearVisible]);

            Codec.ActiveCallCountFeedback.OutputChange += new EventHandler<EventArgs>(InCallFeedback_OutputChange);
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
            TriList.SetSigFalseAction(UIBoolJoin.VCStagingConnectPress, ConnectPress);
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
                DialKeypad.Digit0.SetSigFalseAction(() => DialKeypadPress("0"));
                DialKeypad.Digit1.SetSigFalseAction(() => DialKeypadPress("1"));
                DialKeypad.Digit2.SetSigFalseAction(() => DialKeypadPress("2"));
                DialKeypad.Digit3.SetSigFalseAction(() => DialKeypadPress("3"));
                DialKeypad.Digit4.SetSigFalseAction(() => DialKeypadPress("4"));
                DialKeypad.Digit5.SetSigFalseAction(() => DialKeypadPress("5"));
                DialKeypad.Digit6.SetSigFalseAction(() => DialKeypadPress("6"));
                DialKeypad.Digit7.SetSigFalseAction(() => DialKeypadPress("7"));
                DialKeypad.Digit8.SetSigFalseAction(() => DialKeypadPress("8"));
                DialKeypad.Digit9.SetSigFalseAction(() => DialKeypadPress("9"));
                DialKeypad.Misc1SigName = "*";
                DialKeypad.Misc1.SetSigFalseAction(() => DialKeypadPress("*"));
                DialKeypad.Misc2SigName = "#";
                DialKeypad.Misc2.SetSigFalseAction(() => DialKeypadPress("#"));
                TriList.SetSigFalseAction(UIBoolJoin.KeyboardClearPress, DialKeypadBackspacePress);
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

        /// <summary>
        ///
        /// </summary>
        void ConnectPress()
        {
            if (Codec.IsInCall)
                Codec.EndCall("end whatever is selected");
            else
                Codec.Dial(DialStringBuilder.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        void InCallFeedback_OutputChange(object sender, EventArgs e)
        {
            var inCall = Codec.IsInCall;
            Debug.Console(1, "*#* Codec Driver InCallFeedback change={0}", InCall);
            TriList.UShortInput[UIUshortJoin.VCStagingConnectButtonMode].UShortValue = (ushort)(inCall ? 1 : 0);
            StagingBarInterlock.ShowInterlocked(
                inCall ? UIBoolJoin.VCStagingActivePopoverVisible : UIBoolJoin.VCStagingInactivePopoverVisible);

            if (Codec.IsInCall) // Call is starting
            {
                // Header icon
                // Volume bar needs to have mic mute
            }
            else // ending
            {
                // Header icon
                // Volume bar no mic mute (or hidden if no source?)
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        void DialKeypadPress(string i)
        {
            DialStringBuilder.Append(i);
            DialStringFeedback.FireUpdate();
            TriList.BooleanInput[UIBoolJoin.KeyboardClearVisible].BoolValue = 
                DialStringBuilder.Length > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        void DialKeypadBackspacePress()
        {
            DialStringBuilder.Remove(DialStringBuilder.Length - 1, 1);
            DialStringFeedback.FireUpdate();
            TriList.BooleanInput[UIBoolJoin.KeyboardClearVisible].BoolValue =
                DialStringBuilder.Length > 0;
            TriList.SetBool(UIBoolJoin.VCStagingConnectEnable, DialStringBuilder.Length > 0);
        }
    }
}