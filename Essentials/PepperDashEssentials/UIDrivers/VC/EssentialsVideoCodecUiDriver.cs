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
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.UIDrivers.VC
{


#warning FOR SPARK - (GFX also) we need a staging bar for in call state where there is no camera button
    /// <summary>
    /// This fella will likely need to interact with the room's source, although that is routed via the spark...
    /// Probably needs event or FB to feed AV driver - to show two-mute volume when appropriate.
    /// 
    /// </summary>
    public class EssentialsVideoCodecUiDriver : PanelDriverBase
    {
        IAVDriver Parent;

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

        SubpageReferenceList ActiveCallsSRL;

        // These are likely temp until we get a keyboard built
        StringFeedback DialStringFeedback;
        StringBuilder DialStringBuilder = new StringBuilder();
        BoolFeedback DialStringBackspaceVisibleFeedback;

        ModalDialog IncomingCallModal;

        eKeypadMode KeypadMode;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="triList"></param>
        /// <param name="codec"></param>
        public EssentialsVideoCodecUiDriver(BasicTriListWithSmartObject triList, IAVDriver parent, VideoCodecBase codec)
            : base(triList)
        {
            Codec = codec;
            Parent = parent;
            SetupCallStagingPopover();
            SetupDialKeypad();
            ActiveCallsSRL = new SubpageReferenceList(TriList, UISmartObjectJoin.CodecActiveCallsHeaderList, 3, 3, 3);

            codec.CallStatusChange += new EventHandler<CodecCallStatusItemChangeEventArgs>(Codec_CallStatusChange);

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

        }

        /// <summary>
        /// Handles status changes for calls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Codec_CallStatusChange(object sender, CodecCallStatusItemChangeEventArgs e)
        {
            var call = e.CallItem;
            Debug.Console(1, "*#* UI: Codec status {0}: {1} --> {2}", call.Name, e.PreviousStatus, e.NewStatus);
            switch (e.NewStatus)
            {
                case eCodecCallStatus.Connected:
                    // fire at SRL item
                    Debug.Console(1, "*#* UI: Call Connected {0}", call.Name);
                    KeypadMode = eKeypadMode.DTMF;
                    DialStringBuilder.Remove(0, DialStringBuilder.Length);
                    DialStringFeedback.FireUpdate();
                    TriList.SetBool(UIBoolJoin.KeyboardClearVisible, false);
                    break;
                case eCodecCallStatus.Connecting:
                    // fire at SRL item
                    Debug.Console(1, "*#* UI: Call Connecting {0}", call.Name);
                    break;
                case eCodecCallStatus.Dialing:
                    Debug.Console(1, "*#* UI: Call Dialing {0}", call.Name);
                    break;
                case eCodecCallStatus.Disconnected:
                    Debug.Console(1, "*#* UI: Call Disconnecting {0}", call.Name);
                    if (!Codec.IsInCall)
                    {
                        KeypadMode = eKeypadMode.Dial;
                        DialStringBuilder.Remove(0, DialStringBuilder.Length);
                        DialStringFeedback.FireUpdate();
                    }
                    break;
                case eCodecCallStatus.Disconnecting:
                    break;
                case eCodecCallStatus.EarlyMedia:
                    break;
                case eCodecCallStatus.Idle:
                    break;
                case eCodecCallStatus.Incoming:
                    // fire up a modal
                    ShowIncomingModal(call);
                    break;
                case eCodecCallStatus.OnHold:
                    break;
                case eCodecCallStatus.Preserved:
                    break;
                case eCodecCallStatus.RemotePreserved:
                    break;
                case eCodecCallStatus.Ringing:
                    break;
                default:
                    break;
            }
            TriList.UShortInput[UIUshortJoin.VCStagingConnectButtonMode].UShortValue = (ushort)(Codec.IsInCall ? 1 : 0);
            StagingBarInterlock.ShowInterlocked(Codec.IsInCall ? 
                UIBoolJoin.VCStagingActivePopoverVisible : UIBoolJoin.VCStagingInactivePopoverVisible);

            // Set mode of header button
            if (!Codec.IsInCall)
                TriList.SetUshort(UIUshortJoin.CallHeaderButtonMode, 0);
            else if(Codec.ActiveCalls.Any(c => c.Type == eCodecCallType.Video))
                TriList.SetUshort(UIUshortJoin.CallHeaderButtonMode, 2);
            else
                TriList.SetUshort(UIUshortJoin.CallHeaderButtonMode, 1);

            // Update list of calls
            UpdateCallsHeaderList(call);
        }

        /// <summary>
        /// Redraws the calls list on the header
        /// </summary>
        void UpdateCallsHeaderList(CodecActiveCallItem call)
        {
            var activeList = Codec.ActiveCalls.Where(c => c.IsActiveCall).ToList();
            ActiveCallsSRL.Clear();
            ushort i = 1;
            foreach (var c in activeList)
            {
                var item = new SubpageReferenceListItem(1, ActiveCallsSRL);
                ActiveCallsSRL.StringInputSig(i, 1).StringValue = c.Name;
                ActiveCallsSRL.StringInputSig(i, 2).StringValue = c.Number;
                ActiveCallsSRL.StringInputSig(i, 3).StringValue = c.Status.ToString();
                ActiveCallsSRL.UShortInputSig(i, 1).UShortValue = (ushort)(c.Type == eCodecCallType.Video ? 2 : 1);
                var cc = c; // for scope in lambda
                ActiveCallsSRL.GetBoolFeedbackSig(i, 1).SetSigFalseAction(() => Codec.EndCall(cc));
                i++;
            }
                ActiveCallsSRL.Count = (ushort)activeList.Count;
        }
        
        /// <summary>
        /// 
        /// </summary>
        void ShowIncomingModal(CodecActiveCallItem call)
        {
            IncomingCallModal = new ModalDialog(TriList);
            string msg;
            string icon;
            if (call.Type == eCodecCallType.Audio)
            {
                icon = "Phone";
                msg = string.Format("Incoming phone call from: {0}", call.Name);
            }
            else
            {
                icon = "Camera";
                msg = string.Format("Incoming video call from: {0}", call.Name);
            }
            IncomingCallModal.PresentModalDialog(2, "Incoming Call", icon, msg,
                "Ignore", "Accept", false, false, b =>
                    {
                        if (b == 1)
                            Codec.RejectCall(call);
                        else //2
                            Codec.AcceptCall(call);
                        IncomingCallModal = null;
                    });
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
            TriList.SetSigFalseAction(UIBoolJoin.VCStagingKeypadPress, ShowKeypad);
            TriList.SetSigFalseAction(UIBoolJoin.VCStagingRecentsPress, ShowRecents);
            TriList.SetSigFalseAction(UIBoolJoin.VCStagingConnectPress, ConnectPress);
            TriList.SetSigFalseAction(UIBoolJoin.CallEndPress, () =>
                {
                    if (Codec.ActiveCalls.Count > 1)
                    {
                        Debug.Console(1, "#*#*#*# FUCK ME!!!!");
                        Parent.PopupInterlock.ShowInterlocked(UIBoolJoin.HeaderActiveCallsListVisible);
                    }
                    else
                        Codec.EndAllCalls();
                });
            TriList.SetSigFalseAction(UIBoolJoin.CallEndAllConfirmPress, Codec.EndAllCalls);
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

            TriList.SetSigFalseAction(UIBoolJoin.CodecDirectorySearchTextPress, () =>
                {
                    var kb = Parent.Keyboard;
                    kb.OutputFeedback.OutputChange += new EventHandler<EventArgs>(DirectoryKeyboardChange);
                    kb.HideAction += () =>
                    {
                        kb.OutputFeedback.OutputChange -= DirectoryKeyboardChange;
                    };
                    
                });
        }

        /// <summary>
        /// 
        /// </summary>
        void DirectoryKeyboardChange(object sender, EventArgs e)
        {
            
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
            Codec.Dial(DialStringBuilder.ToString());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        void DialKeypadPress(string i)
        {
            if (KeypadMode == eKeypadMode.Dial)
            {
                DialStringBuilder.Append(i);
                DialStringFeedback.FireUpdate();
                TriList.BooleanInput[UIBoolJoin.KeyboardClearVisible].BoolValue =
                    DialStringBuilder.Length > 0;
            }
            else
            {
                Codec.SendDtmf(i);
                DialStringBuilder.Append(i);
                DialStringFeedback.FireUpdate();
                // no delete key in this mode!
            }
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

        enum eKeypadMode
        {
            Dial, DTMF
        }
    }
}