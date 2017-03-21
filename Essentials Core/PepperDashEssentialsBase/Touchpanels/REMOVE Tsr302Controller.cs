//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
//using Crestron.SimplSharpPro.UI;

//using PepperDash.Core;


//namespace PepperDash.Essentials.Core
//{
//[Obsolete("Replaced, initially with CrestronTsr302Controller in Resissentials")] 
//    public class Tsr302Controller : SmartGraphicsTouchpanelControllerBase
//    {
//        //public override List<CueActionPair> FunctionList
//        //{
//        //    get
//        //    {
//        //        return new List<CueActionPair>
//        //            {

//        //            };
//        //    }
//        //}

//        public Tsr302 Remote { get; private set; }

//        SourceListSubpageReferenceList SourceSelectSRL;
//        DevicePageControllerBase CurrentPresentationSourcePageController;
//        CTimer VolumeFeedbackTimer;


//        public Tsr302Controller(string key, string name, Tsr302 device, string sgdFilePath) :
//            base(key, name, device, sgdFilePath)
//        {				
//            // Base takes care of TriList
//            Remote = device;
//            Remote.Home.UserObject = new BoolCueActionPair(b => { if (!b) PressHome(); });
//            Remote.VolumeUp.UserObject = new BoolCueActionPair(b => { if (!b) PressHome(); });
//            Remote.ButtonStateChange += Remote_ButtonStateChange;
//        }

//        public override bool CustomActivate()
//        {
//            var baseSuccess = base.CustomActivate();
//            if (!baseSuccess) return false;

//            SourceSelectSRL = new SourceListSubpageReferenceList(this.TriList, n =>
//            { if (CurrentRoom != null) CurrentRoom.SelectSource(n); });


//            return true;
//        }

//        protected override void SwapAudioDeviceControls(IVolumeFunctions oldDev, IVolumeFunctions newDev)
//        {
//            // stop presses
//            if (oldDev != null)
//            {
//                ReleaseAudioPresses();
//                if (oldDev is IVolumeTwoWay)
//                {
//                    (newDev as IVolumeTwoWay).VolumeLevelFeedback.OutputChange -= VolumeLevelOutput_OutputChange;
//                    (oldDev as IVolumeTwoWay).VolumeLevelFeedback
//                        .UnlinkInputSig(TriList.UShortInput[CommonIntCue.MainVolumeLevel.Number]);
//                }
//            }

//            if (newDev != null)
//            {
//                Remote.VolumeDown.UserObject = newDev.VolumeDownCueActionPair;
//                Remote.VolumeUp.UserObject = newDev.VolumeUpCueActionPair;
//                Remote.Mute.UserObject = newDev.MuteToggleCueActionPair;
//                if (newDev is IVolumeTwoWay)
//                {
//                    var vOut = (newDev as IVolumeTwoWay).VolumeLevelFeedback;
//                    vOut.OutputChange += VolumeLevelOutput_OutputChange;
//                    TriList.UShortInput[CommonIntCue.MainVolumeLevel.Number].UShortValue = vOut.UShortValue;
//                }
//            }
//            else
//            {
//                Remote.VolumeDown.UserObject = null;
//                Remote.VolumeUp.UserObject = null;
//                Remote.Mute.UserObject = null;
//            }

//            base.SwapAudioDeviceControls(oldDev, newDev);
//        }

//        void PressHome()
//        {

//        }

//        void VolumeLevelOutput_OutputChange(object sender, EventArgs e)
//        {
//            // Set level and show popup on timer
//            TriList.UShortInput[CommonIntCue.MainVolumeLevel.Number].UShortValue =
//                (sender as IntFeedback).UShortValue;

//            if (VolumeFeedbackTimer == null)
//            {
//                TriList.BooleanInput[CommonBoolCue.ShowVolumeSlider.Number].BoolValue = true;
//                VolumeFeedbackTimer = new CTimer(o => {
//                    TriList.BooleanInput[CommonBoolCue.ShowVolumeSlider.Number].BoolValue = false;
//                }, 1000);
//            }

//        }

//        void ReleaseAudioPresses()
//        {
//            if (Remote.VolumeDown.UserObject is BoolCueActionPair && Remote.VolumeDown.State == eButtonState.Pressed)
//                (Remote.VolumeDown.UserObject as BoolCueActionPair).Invoke(false);
//            if (Remote.VolumeUp.UserObject is BoolCueActionPair && Remote.VolumeUp.State == eButtonState.Pressed)
//                (Remote.VolumeUp.UserObject as BoolCueActionPair).Invoke(false);
//            if (Remote.Mute.UserObject is BoolCueActionPair && Remote.Mute.State == eButtonState.Pressed)
//                (Remote.Mute.UserObject as BoolCueActionPair).Invoke(false);
//        }

//        /// <summary>
//        /// Handler.  Run UO's stored in buttons
//        /// </summary>
//        void Remote_ButtonStateChange(GenericBase device, ButtonEventArgs args)
//        {
//            Debug.Console(2, this, "{0}={1}", args.Button.Name, args.Button.State);
//            var uo = args.Button.UserObject as BoolCueActionPair;
//            if (uo != null)
//                uo.Invoke(args.NewButtonState == eButtonState.Pressed);
//        }
//    }
//}