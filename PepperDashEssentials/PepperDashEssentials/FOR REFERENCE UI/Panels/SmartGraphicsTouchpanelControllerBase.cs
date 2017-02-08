//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;


//namespace PepperDash.Essentials.Core
//{
//    public abstract class SmartGraphicsTouchpanelControllerBase : CrestronGenericBaseDevice
//    {
//        public BasicTriListWithSmartObject TriList { get; protected set; }
//        public bool UsesSplashPage { get; set; }
//        public string SplashMessage { get; set; }
//        public bool ShowDate
//        {
//            set { TriList.BooleanInput[UiCue.ShowDate.Number].BoolValue = value; }
//        }
//        public bool ShowTime
//        {
//            set { TriList.BooleanInput[UiCue.ShowTime.Number].BoolValue = value; }
//        }

//        //public abstract List<CueActionPair> FunctionList { get; }


//        protected eMainModeType MainMode;
//        protected SourceListItem CurrentPresentationControlDevice;

//        /// <summary>
//        /// Defines the signal offset for the presentation device.  Defaults to 100
//        /// </summary>
//        public uint PresentationControlDeviceJoinOffset { get { return 100; } }

//        public enum eMainModeType
//        {
//            Presentation, Splash, Tech
//        }

//        protected string SgdFilePath;
//        public EssentialsRoom CurrentRoom { get; protected set; }
//        protected Dictionary<SourceListItem, DevicePageControllerBase> LoadedPageControllers
//            = new Dictionary<SourceListItem, DevicePageControllerBase>();

//        static object RoomChangeLock = new object();

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        public SmartGraphicsTouchpanelControllerBase(string key, string name, BasicTriListWithSmartObject triList,
//            string sgdFilePath)
//            : base(key, name, triList)
//        {
//            TriList = triList;
//            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
//            if (string.IsNullOrEmpty(sgdFilePath)) throw new ArgumentNullException("sgdFilePath");
//            SgdFilePath = sgdFilePath;
//            TriList.LoadSmartObjects(SgdFilePath);
//            UsesSplashPage = true;
//            SplashMessage = "Welcome";
//            TriList.SigChange += Tsw_AnySigChange;
//            foreach (var kvp in TriList.SmartObjects)
//                kvp.Value.SigChange += this.Tsw_AnySigChange;
//        }

//#warning wire UI manipulating presses up here, typically in child classes...
//        //public override bool CustomActivate()
//        //{
//        //    var baseSuccess = base.CustomActivate();
//        //    if (!baseSuccess) return false;


//        //    // Wiring up the buttons with UOs 
//        //    foreach (var uo in this.FunctionList)
//        //    {
//        //        if (uo.Cue.Number == 0) continue;
//        //        //if (uo is BoolCueActionPair)
//        //        //    TriList.BooleanOutput[uo.Cue.Number].UserObject = uo;
//        //        //else if (uo is UShortCueActionPair)
//        //        //    TriList.UShortOutput[uo.Cue.Number].UserObject = uo;
//        //        //else if (uo is StringCueActionPair)
//        //        //    TriList.StringOutput[uo.Cue.Number].UserObject = uo;
//        //    }

//        //    return true;
//        //}

//        //public void SetCurrentRoom(EssentialsRoom room)
//        //{
//        //    if (CurrentRoom != null)
//        //        HideRoomUI();
//        //    CurrentRoom = room;
//        //    ShowRoomUI();
//        //}

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="room"></param>
//        public void SetCurrentRoom(EssentialsRoom room)
//        {
//            if (CurrentRoom == room) return;

//            IBasicVolumeControls oldAudio = null;
//            //Disconnect current room and audio device
//            if (CurrentRoom != null)
//            {
//                HideRoomUI();
//                CurrentRoom.AudioDeviceWillChange -= CurrentRoom_AudioDeviceWillChange;
//                CurrentRoom.PresentationSourceChange -= CurrentRoom_PresentationSourceChange;
//                oldAudio = CurrentRoom.CurrentAudioDevice;
//            }

//            CurrentRoom = room;
//            IBasicVolumeControls newAudio = null;
//            if (CurrentRoom != null)
//            {
//                CurrentRoom.AudioDeviceWillChange += this.CurrentRoom_AudioDeviceWillChange;
//                CurrentRoom.PresentationSourceChange += this.CurrentRoom_PresentationSourceChange;
//                SetControlSource(CurrentRoom.CurrentPresentationSourceInfo);
//                newAudio = CurrentRoom.CurrentAudioDevice;
//                ShowRoomUI();
//            }

//            SwapAudioDeviceControls(oldAudio, newAudio);
//        }

//        /// <summary>
//        /// Detaches and attaches an IVolumeFunctions device to the appropriate TP TriList signals.
//        /// This will also add IVolumeNumeric if the device implements it.
//        /// Overriding classes should call this. Overriding classes are responsible for
//        /// linking up to hard keys, etc.
//        /// </summary>
//        /// <param name="oldDev">May be null</param>
//        /// <param name="newDev">May be null</param>
//        protected virtual void SwapAudioDeviceControls(IBasicVolumeControls oldDev, IBasicVolumeControls newDev)
//        {
//            // Disconnect
//            if (oldDev != null)
//            {
//                TriList.BooleanOutput[CommonBoolCue.VolumeDown.Number].UserObject = null;
//                TriList.BooleanOutput[CommonBoolCue.VolumeUp.Number].UserObject = null;
//                TriList.BooleanOutput[CommonBoolCue.MuteToggle.Number].UserObject = null;
//                TriList.BooleanInput[CommonBoolCue.ShowVolumeButtons.Number].BoolValue = false;
//                TriList.BooleanInput[CommonBoolCue.ShowVolumeSlider.Number].BoolValue = false;
//                if (oldDev is IBasicVolumeWithFeedback)
//                {
//                    var fbDev = oldDev as IBasicVolumeWithFeedback;
//                    TriList.UShortOutput[401].UserObject = null;
//                    fbDev.MuteFeedback.UnlinkInputSig(TriList.BooleanInput[403]);
//                    fbDev.VolumeLevelFeedback.UnlinkInputSig(TriList.UShortInput[401]);
//                }
//            }
//            if (newDev != null)
//            {
//                TriList.BooleanInput[CommonBoolCue.ShowVolumeSlider.Number].BoolValue = true;
//                TriList.SetBoolSigAction(401, newDev.VolumeUp);
//                TriList.SetBoolSigAction(402, newDev.VolumeDown);
//                TriList.SetSigFalseAction(405, newDev.MuteToggle);

//                if (newDev is IBasicVolumeWithFeedback) // Show slider
//                {
//                    var fbDev = newDev as IBasicVolumeWithFeedback;
//                    TriList.BooleanInput[406].BoolValue = false;
//                    TriList.BooleanInput[407].BoolValue = true;
//                    TriList.UShortOutput[401].UserObject = new Action<ushort>(fbDev.SetVolume);
//                    fbDev.VolumeLevelFeedback.LinkInputSig(TriList.UShortInput[401]);
//                }
//                else // Show buttons only
//                {
//                    TriList.BooleanInput[406].BoolValue = true;
//                    TriList.BooleanInput[407].BoolValue = false;
//                }

//            }
//        }


//        /// <summary>
//        /// Does nothing. Override to add functionality when calling SetCurrentRoom
//        /// </summary>
//        protected virtual void HideRoomUI() { }

//        /// <summary>
//        /// Does nothing. Override to add functionality when calling SetCurrentRoom
//        /// </summary>		
//        protected virtual void ShowRoomUI() { }

//        /// <summary>
//        /// Sets up the current presentation device and updates statuses if the device is capable.
//        /// </summary>
//        protected void SetControlSource(SourceListItem newSource)
//        {
//            if (CurrentPresentationControlDevice != null)
//            {
//                HideCurrentPresentationSourceUi();
//#warning Get button methods from RESI, and find a more-well-defined way to wire up feedbacks

//                // Unhook presses and things
//                //if (CurrentPresentationControlDevice is IHasCueActionList)
//                //{
//                //    foreach (var uo in (CurrentPresentationControlDevice as IHasCueActionList).CueActionList)
//                //    {
//                //        if (uo.Cue.Number == 0) continue;
//                //        if (uo is BoolCueActionPair)
//                //        {
//                //            var bSig = TriList.BooleanOutput[uo.Cue.Number];
//                //            // Disconnection should also clear bool sigs in case they are pressed and
//                //            // might be orphaned
//                //            if (bSig.BoolValue)
//                //                (bSig.UserObject as BoolCueActionPair).Invoke(false);
//                //            bSig.UserObject = null;
//                //        }
//                //        else if (uo is UShortCueActionPair)
//                //            TriList.UShortOutput[uo.Cue.Number].UserObject = null;
//                //        else if (uo is StringCueActionPair)
//                //            TriList.StringOutput[uo.Cue.Number].UserObject = null;
//                //    }
//                //}
//                // unhook outputs
//                if (CurrentPresentationControlDevice is IHasFeedback)
//                {
//                    foreach (var fb in (CurrentPresentationControlDevice as IHasFeedback).Feedbacks)
//                    {
//                        if (fb.Cue.Number == 0) continue;
//                        if (fb is BoolFeedback)
//                            (fb as BoolFeedback).UnlinkInputSig(TriList.BooleanInput[fb.Cue.Number]);
//                        else if (fb is IntFeedback)
//                            (fb as IntFeedback).UnlinkInputSig(TriList.UShortInput[fb.Cue.Number]);
//                        else if (fb is StringFeedback)
//                            (fb as StringFeedback).UnlinkInputSig(TriList.StringInput[fb.Cue.Number]);
//                    }
//                }
//            }
//            CurrentPresentationControlDevice = newSource;
//            //connect presses and things
//            //if (newSource is IHasCueActionList) // This has functions, get 'em
//            //{
//            //    foreach (var ao in (newSource as IHasCueActionList).CueActionList)
//            //    {
//            //        if (ao.Cue.Number == 0) continue;
//            //        if (ao is BoolCueActionPair)
//            //            TriList.BooleanOutput[ao.Cue.Number].UserObject = ao;
//            //        else if (ao is UShortCueActionPair)
//            //            TriList.UShortOutput[ao.Cue.Number].UserObject = ao;
//            //        else if (ao is StringCueActionPair)
//            //            TriList.StringOutput[ao.Cue.Number].UserObject = ao;
//            //    }
//            //}
//            // connect outputs (addInputSig should update sig)
//            if (CurrentPresentationControlDevice is IHasFeedback)
//            {
//                foreach (var fb in (CurrentPresentationControlDevice as IHasFeedback).Feedbacks)
//                {
//                    if (fb.Cue.Number == 0) continue;
//                    if (fb is BoolFeedback)
//                        (fb as BoolFeedback).LinkInputSig(TriList.BooleanInput[fb.Cue.Number]);
//                    else if (fb is IntFeedback)
//                        (fb as IntFeedback).LinkInputSig(TriList.UShortInput[fb.Cue.Number]);
//                    else if (fb is StringFeedback)
//                        (fb as StringFeedback).LinkInputSig(TriList.StringInput[fb.Cue.Number]);
//                }
//            }
//            ShowCurrentPresentationSourceUi();
//        }

//        /// <summary>
//        /// Reveals the basic UI for the current device
//        /// </summary>
//        protected virtual void ShowCurrentPresentationSourceUi()
//        {
//        }

//        /// <summary>
//        /// Hides the UI for the current device and calls for a feedback signal cleanup
//        /// </summary>
//        protected virtual void HideCurrentPresentationSourceUi()
//        {
//        }


//        /// <summary>
//        /// 
//        /// </summary>
//        void CurrentRoom_PresentationSourceChange(object sender, EssentialsRoomSourceChangeEventArgs args)
//        {
//            SetControlSource(args.NewSource);
//        }


//        /// <summary>
//        /// 
//        /// </summary>
//        void CurrentRoom_AudioDeviceWillChange(object sender, EssentialsRoomAudioDeviceChangeEventArgs e)
//        {
//            SwapAudioDeviceControls(e.OldDevice, e.NewDevice);
//        }



//        /// <summary>
//        /// Panel event handler
//        /// </summary>
//        void Tsw_AnySigChange(object currentDevice, SigEventArgs args)
//        {
//            // plugged in commands
//            object uo = args.Sig.UserObject;

//            if (uo is Action<bool>)
//                (uo as Action<bool>)(args.Sig.BoolValue);
//            else if (uo is Action<ushort>)
//                (uo as Action<ushort>)(args.Sig.UShortValue);
//            else if (uo is Action<string>)
//                (uo as Action<string>)(args.Sig.StringValue);
//        }
//    }
//}