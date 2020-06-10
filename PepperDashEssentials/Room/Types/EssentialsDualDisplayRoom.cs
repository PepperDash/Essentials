using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Room.Config;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDash.Essentials.Devices.Common.AudioCodec;

namespace PepperDash.Essentials
{
    public class EssentialsDualDisplayRoom : EssentialsNDisplayRoomBase, IHasCurrentVolumeControls,
        IRunRouteAction, IPrivacy, IRunDefaultCallRoute, IHasVideoCodec, IHasAudioCodec, IHasInCallFeedback
    {
        public event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;

        public EssentialsDualDisplayRoomPropertiesConfig PropertiesConfig { get; private set; }

        //************************
        // Call-related stuff

        public BoolFeedback InCallFeedback { get; private set; }

        /// <summary>
        /// States: 0 for on hook, 1 for video, 2 for audio, 3 for telekenesis
        /// </summary>
        public IntFeedback CallTypeFeedback { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public BoolFeedback PrivacyModeIsOnFeedback { get; private set; }

        /// <summary>
        /// When something in the room is sharing with the far end or through other means
        /// </summary>
        public BoolFeedback IsSharingFeedback { get; private set; }

        public IRoutingSinkWithSwitching LeftDisplay { get; private set; }
        public IRoutingSinkWithSwitching RightDisplay { get; private set; }


        protected override Func<bool> OnFeedbackFunc
        {
            get
            {
                return () =>
                {
                    var leftDisp = LeftDisplay as DisplayBase;
                    var rightDisp = RightDisplay as DisplayBase;
                    var val = leftDisp != null && leftDisp.CurrentSourceInfo != null
                        && leftDisp.CurrentSourceInfo.Type == eSourceListItemType.Route
                        && rightDisp != null && rightDisp.CurrentSourceInfo != null
                        && rightDisp.CurrentSourceInfo.Type == eSourceListItemType.Route;
                    return val;
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override Func<bool> IsWarmingFeedbackFunc
        {
            get
            {
                return () =>
                {
                    var leftDisp = LeftDisplay as DisplayBase;
                    var rightDisp = RightDisplay as DisplayBase;
                    if (leftDisp != null && RightDisplay != null)
                        return leftDisp.IsWarmingUpFeedback.BoolValue || rightDisp.IsWarmingUpFeedback.BoolValue;
                    else
                        return false;
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override Func<bool> IsCoolingFeedbackFunc
        {
            get
            {
                return () =>
                {
                    var leftDisp = LeftDisplay as DisplayBase;
                    var rightDisp = RightDisplay as DisplayBase;
                    if (leftDisp != null && RightDisplay != null)
                        return leftDisp.IsCoolingDownFeedback.BoolValue || rightDisp.IsCoolingDownFeedback.BoolValue;
                    else
                        return false;
                };
            }
        }

        public IBasicVolumeControls DefaultAudioDevice { get; private set; }
        public IBasicVolumeControls DefaultVolumeControls { get; private set; }

        public VideoCodecBase VideoCodec { get; private set; }

        public AudioCodecBase AudioCodec { get; private set; }

        public bool ExcludeFromGlobalFunctions { get; set; }

        public string DefaultSourceItem { get; set; }

        public ushort DefaultVolume { get; set; }

        /// <summary>
        /// If room is off, enables power on to last source. Default true
        /// </summary>
        public bool EnablePowerOnToLastSource { get; set; }
        string LastSourceKey;

        /// <summary>
        /// Sets the volume control device, and attaches/removes InUseTrackers with "audio"
        /// tag to device.
        /// </summary>
        public IBasicVolumeControls CurrentVolumeControls
        {
            get { return _CurrentAudioDevice; }
            set
            {
                if (value == _CurrentAudioDevice) return;

                var oldDev = _CurrentAudioDevice;
                // derigister this room from the device, if it can
                if (oldDev is IInUseTracking)
                    (oldDev as IInUseTracking).InUseTracker.RemoveUser(this, "audio");
                var handler = CurrentVolumeDeviceChange;
                if (handler != null)
                    CurrentVolumeDeviceChange(this, new VolumeDeviceChangeEventArgs(oldDev, value, ChangeType.WillChange));
                _CurrentAudioDevice = value;
                if (handler != null)
                    CurrentVolumeDeviceChange(this, new VolumeDeviceChangeEventArgs(oldDev, value, ChangeType.DidChange));
                // register this room with new device, if it can
                if (_CurrentAudioDevice is IInUseTracking)
                    (_CurrentAudioDevice as IInUseTracking).InUseTracker.AddUser(this, "audio");
            }
        }
        IBasicVolumeControls _CurrentAudioDevice;

        /// <summary>
        /// "codecOsd"
        /// </summary>
        public string DefaultCodecRouteString { get { return "codecOsd"; } }

        /// <summary>
        /// Temporary implementation. Returns the schedule-ready object or null if none. Fow now,
        /// always returns the VideoCodec if it is capable
        /// </summary>
        public IHasScheduleAwareness ScheduleSource { get { return VideoCodec as IHasScheduleAwareness; } }

        CCriticalSection SourceSelectLock = new CCriticalSection();

        public EssentialsDualDisplayRoom(DeviceConfig config)
            : base(config)
        {
            try
            {
                PropertiesConfig = JsonConvert.DeserializeObject<EssentialsDualDisplayRoomPropertiesConfig>
                    (config.Properties.ToString());

                var leftDisp = PropertiesConfig.Displays[eSourceListItemDestinationTypes.leftDisplay];
                if (leftDisp != null)
                {
                    if (!string.IsNullOrEmpty(leftDisp.Key))
                    {
                        LeftDisplay = DeviceManager.GetDeviceForKey(leftDisp.Key) as IRoutingSinkWithSwitching;
                        Displays.Add(eSourceListItemDestinationTypes.leftDisplay, LeftDisplay);
                    }
                    else
                        Debug.Console(0, this, "Unable to get LeftDisplay for Room");
                }

                var rightDisp = PropertiesConfig.Displays[eSourceListItemDestinationTypes.rightDisplay];
                if (rightDisp != null)
                {
                    if (!string.IsNullOrEmpty(rightDisp.Key))
                    {
                        LeftDisplay = DeviceManager.GetDeviceForKey(rightDisp.Key) as IRoutingSinkWithSwitching;
                        Displays.Add(eSourceListItemDestinationTypes.rightDisplay, RightDisplay);
                    }
                    else
                        Debug.Console(0, this, "Unable to get LeftDisplay for Room");
                }

                VideoCodec = DeviceManager.GetDeviceForKey(PropertiesConfig.VideoCodecKey) as
                   PepperDash.Essentials.Devices.Common.VideoCodec.VideoCodecBase;
                if (VideoCodec == null)
                    throw new ArgumentNullException("codec cannot be null");

                AudioCodec = DeviceManager.GetDeviceForKey(PropertiesConfig.AudioCodecKey) as
                    PepperDash.Essentials.Devices.Common.AudioCodec.AudioCodecBase;
                if (AudioCodec == null)
                    Debug.Console(0, this, "No Audio Codec Found");

                DefaultAudioDevice = DeviceManager.GetDeviceForKey(PropertiesConfig.DefaultAudioKey) as IBasicVolumeControls;

                Initialize();
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error building room \n{0}", e);
            }
        }

        void Initialize()
        {
            if (DefaultAudioDevice is IBasicVolumeControls)
                DefaultVolumeControls = DefaultAudioDevice as IBasicVolumeControls;
            else if (DefaultAudioDevice is IHasVolumeDevice)
                DefaultVolumeControls = (DefaultAudioDevice as IHasVolumeDevice).VolumeDevice;
            CurrentVolumeControls = DefaultVolumeControls;


            var leftDisp = LeftDisplay as DisplayBase;
            if (leftDisp != null)
                InitializeDisplay(leftDisp);

            var rightDisp = RightDisplay as DisplayBase;
            if (rightDisp != null)
                InitializeDisplay(rightDisp);

            // Get Microphone Privacy object, if any
            this.MicrophonePrivacy = EssentialsRoomConfigHelper.GetMicrophonePrivacy(PropertiesConfig, this);

            Debug.Console(2, this, "Microphone Privacy Config evaluated.");

            // Get emergency object, if any
            this.Emergency = EssentialsRoomConfigHelper.GetEmergency(PropertiesConfig, this);

            Debug.Console(2, this, "Emergency Config evaluated.");

            // Combines call feedback from both codecs if available
            InCallFeedback = new BoolFeedback(() =>
            {
                bool inAudioCall = false;
                bool inVideoCall = false;

                if (AudioCodec != null)
                    inAudioCall = AudioCodec.IsInCall;

                if (VideoCodec != null)
                    inVideoCall = VideoCodec.IsInCall;

                if (inAudioCall || inVideoCall)
                    return true;
                else
                    return false;
            });

            VideoCodec.CallStatusChange += (o, a) => this.InCallFeedback.FireUpdate();

            if (AudioCodec != null)
                AudioCodec.CallStatusChange += (o, a) => this.InCallFeedback.FireUpdate();

            IsSharingFeedback = new BoolFeedback(() => VideoCodec.SharingContentIsOnFeedback.BoolValue);
            VideoCodec.SharingContentIsOnFeedback.OutputChange += (o, a) => this.IsSharingFeedback.FireUpdate();

            // link privacy to VC (for now?)
            PrivacyModeIsOnFeedback = new BoolFeedback(() => VideoCodec.PrivacyModeIsOnFeedback.BoolValue);
            VideoCodec.PrivacyModeIsOnFeedback.OutputChange += (o, a) => this.PrivacyModeIsOnFeedback.FireUpdate();

            CallTypeFeedback = new IntFeedback(() => 0);

            SourceListKey = "default";
            EnablePowerOnToLastSource = true;
        }

        void InitializeDisplay(DisplayBase disp)
        {
            if (disp != null)
            {
                // Link power, warming, cooling to display
                disp.PowerIsOnFeedback.OutputChange += (o, a) =>
                    {
                        if (disp.PowerIsOnFeedback.BoolValue != OnFeedback.BoolValue)
                        {
                            if (!disp.PowerIsOnFeedback.BoolValue)
                                disp.CurrentSourceInfo = null;
                            OnFeedback.FireUpdate();
                        }
                        if (disp.PowerIsOnFeedback.BoolValue)
                        {
                            SetDefaultLevels();
                        }
                    };

                disp.IsWarmingUpFeedback.OutputChange += (o, a) =>
                {
                    IsWarmingUpFeedback.FireUpdate();
                    if (!IsWarmingUpFeedback.BoolValue)
                        (CurrentVolumeControls as IBasicVolumeWithFeedback).SetVolume(DefaultVolume);
                };
                disp.IsCoolingDownFeedback.OutputChange += (o, a) =>
                {
                    IsCoolingDownFeedback.FireUpdate();
                };
            }
        }

        protected override void CustomSetConfig(DeviceConfig config)
        {
            var newPropertiesConfig = JsonConvert.DeserializeObject<EssentialsDualDisplayRoomPropertiesConfig>(config.Properties.ToString());

            if (newPropertiesConfig != null)
                PropertiesConfig = newPropertiesConfig;

            ConfigWriter.UpdateRoomConfig(config);
        }

        public override bool CustomActivate()
        {
            // Add Occupancy object from config
            if (PropertiesConfig.Occupancy != null)
                this.SetRoomOccupancy(DeviceManager.GetDeviceForKey(PropertiesConfig.Occupancy.DeviceKey) as
                    IOccupancyStatusProvider, PropertiesConfig.Occupancy.TimeoutMinutes);

            this.LogoUrl = PropertiesConfig.Logo.GetUrl();
            this.SourceListKey = PropertiesConfig.SourceListKey;
            this.DefaultSourceItem = PropertiesConfig.DefaultSourceItem;
            this.DefaultVolume = (ushort)(PropertiesConfig.Volumes.Master.Level * 65535 / 100);

            return base.CustomActivate();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void EndShutdown()
        {
            VideoCodec.EndAllCalls();

            SetDefaultLevels();

            RunDefaultPresentRoute();

            CrestronEnvironment.Sleep(1000);

            RunRouteAction("roomOff", SourceListKey);
        }

        /// <summary>
        /// Routes the default source item, if any. Returns true when default route exists
        /// </summary>
        public override bool RunDefaultPresentRoute()
        {
            if (DefaultSourceItem != null)
                RunRouteAction(DefaultSourceItem, SourceListKey);

            return DefaultSourceItem != null;
        }

        /// <summary>
        /// Sets up the room when started into call mode without presenting a source
        /// </summary>
        /// <returns></returns>
        public bool RunDefaultCallRoute()
        {
            RunRouteAction(DefaultCodecRouteString, SourceListKey);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeKey"></param>
        public void RunRouteAction(string routeKey, string sourceListKey)
        {
            RunRouteAction(routeKey,  sourceListKey, null);
        }

        /// <summary>
        /// Gets a source from config list SourceListKey and dynamically build and executes the
        /// route or commands
        /// </summary>
        /// <param name="name"></param>
        public void RunRouteAction(string routeKey, string sourceListKey, Action successCallback)
        {
            // Run this on a separate thread
            new CTimer(o =>
            {
                // try to prevent multiple simultaneous selections
                SourceSelectLock.TryEnter();

                try
                {

                    Debug.Console(1, this, "Run route action '{0}'", routeKey);
                    var dict = ConfigReader.ConfigObject.GetSourceListForKey(sourceListKey);
                    if (dict == null)
                    {
                        Debug.Console(1, this, "WARNING: Config source list '{0}' not found", sourceListKey);
                        return;
                    }

                    // Try to get the list item by it's string key
                    if (!dict.ContainsKey(routeKey))
                    {
                        Debug.Console(1, this, "WARNING: No item '{0}' found on config list '{1}'",
                            routeKey, SourceListKey);
                        return;
                    }

                    // End usage timer on last source
                    if (!string.IsNullOrEmpty(LastSourceKey))
                    {
                        var usageLastSource = dict[LastSourceKey].SourceDevice as IUsageTracking;
                        if (usageLastSource != null && usageLastSource.UsageTracker != null)
                        {
                            try
                            {
                                // There MAY have been failures in here.  Protect
                                usageLastSource.UsageTracker.EndDeviceUsage();
                            }
                            catch (Exception e)
                            {
                                Debug.Console(1, this, "*#* EXCEPTION in end usage tracking:\r{0}", e);
                            }
                        }
                    }

                    // Let's run it
                    var item = dict[routeKey];
                    if (routeKey.ToLower() != "roomoff")
                    {

                        LastSourceKey = routeKey;
                    }
                    //else
                    //    CurrentSourceInfoKey = null;

                    // hand off the individual routes to this helper
                    foreach (var route in item.RouteList)
                        DoRouteItem(route, item, routeKey);

                    // Start usage timer on routed source
                    var usageNewSource = item.SourceDevice as IUsageTracking;
                    if (usageNewSource != null && usageNewSource.UsageTracker != null) // Have to make sure there is a usage tracker!
                    {
                        (item.SourceDevice as IUsageTracking).UsageTracker.StartDeviceUsage();
                    }

                    // See if this can be moved into common, base-class method -------------


                    // Set volume control, using default if non provided
                    IBasicVolumeControls volDev = null;
                    // Handle special cases for volume control
                    if (string.IsNullOrEmpty(item.VolumeControlKey)
                        || item.VolumeControlKey.Equals("$defaultAudio", StringComparison.OrdinalIgnoreCase))
                        volDev = DefaultVolumeControls;

                    // Or a specific device, probably rarely used.
                    else
                    {
                        var dev = DeviceManager.GetDeviceForKey(item.VolumeControlKey);
                        if (dev is IBasicVolumeControls)
                            volDev = dev as IBasicVolumeControls;
                        else if (dev is IHasVolumeDevice)
                            volDev = (dev as IHasVolumeDevice).VolumeDevice;
                    }

                    if (volDev != CurrentVolumeControls)
                    {
                        // zero the volume on the device we are leaving.  
                        // Set the volume to default on device we are entering
                        if (ZeroVolumeWhenSwtichingVolumeDevices && CurrentVolumeControls is IBasicVolumeWithFeedback)
                        {
                            var vd = CurrentVolumeControls as IBasicVolumeWithFeedback;
                            SavedVolumeLevels[vd] = (uint)vd.VolumeLevelFeedback.IntValue;
                            vd.SetVolume(0);
                        }

                        CurrentVolumeControls = volDev;
                        if (ZeroVolumeWhenSwtichingVolumeDevices && CurrentVolumeControls is IBasicVolumeWithFeedback)
                        {
                            var vd = CurrentVolumeControls as IBasicVolumeWithFeedback;
                            ushort vol = (SavedVolumeLevels.ContainsKey(vd) ? (ushort)SavedVolumeLevels[vd] : DefaultVolume);
                            vd.SetVolume(vol);
                        }
                    }
                    // -----------------------------------------------------------------------



                    // store the name and UI info for routes
                    if (item.SourceKey == "$off")
                    {
                        LeftDisplay.CurrentSourceInfoKey = routeKey;
                        LeftDisplay.CurrentSourceInfo = null;
                        RightDisplay.CurrentSourceInfoKey = routeKey;
                        RightDisplay.CurrentSourceInfo = null;
                    }
                    //else if (item.SourceKey != null)
                    //{
                    //    if(item.RouteList
                    //    CurrentSourceInfoKey = routeKey;
                    //    CurrentSourceInfo = item;
                    //}

                    OnFeedback.FireUpdate();

                    // report back when done
                    if (successCallback != null)
                        successCallback();
                }
                catch (Exception e)
                {
                    Debug.Console(1, this, "ERROR in routing: {0}", e);
                }

                SourceSelectLock.Leave();
            }, 0); // end of CTimer
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="route"></param>
        void DoRouteItem(SourceRouteListItem route, SourceListItem sourceItem, string sourceItemKey)
        {
            // if there is a $defaultAll on route, run two separate
            if (route.DestinationKey.Equals("$defaultAll", StringComparison.OrdinalIgnoreCase))
            {
                // Going to assume a single-path route for now
                var tempVideo = new SourceRouteListItem
                {
                    DestinationKey = "$defaultDisplay",
                    SourceKey = route.SourceKey,
                    Type = eRoutingSignalType.Video
                };
                DoRoute(tempVideo, sourceItem, sourceItemKey);
            }
            else
                DoRoute(route, sourceItem, sourceItemKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        bool DoRoute(SourceRouteListItem route, SourceListItem sourceItem, string sourceItemKey)
        {
            IRoutingSink dest = null;

            if (route.DestinationKey.Equals("$defaultaudio", StringComparison.OrdinalIgnoreCase))
                dest = DefaultAudioDevice as IRoutingSinkNoSwitching;
            else if (route.DestinationKey.Equals(LeftDisplay.Key, StringComparison.OrdinalIgnoreCase))
                dest = LeftDisplay;
            else if (route.DestinationKey.Equals(RightDisplay.Key, StringComparison.OrdinalIgnoreCase))
                dest = RightDisplay;
            else
                dest = DeviceManager.GetDeviceForKey(route.DestinationKey) as IRoutingSinkNoSwitching;

            if (dest == null)
            {
                Debug.Console(1, this, "Cannot route, unknown destination '{0}'", route.DestinationKey);
                return false;
            }

            if (route.SourceKey.Equals("$off", StringComparison.OrdinalIgnoreCase))
            {
                dest.ReleaseRoute();



                if (dest is IPower)
                    (dest as IPower).PowerOff();
            }
            else
            {
                var source = DeviceManager.GetDeviceForKey(route.SourceKey) as IRoutingOutputs;
                if (source == null)
                {
                    Debug.Console(1, this, "Cannot route unknown source '{0}' to {1}", route.SourceKey, route.DestinationKey);
                    return false;
                }
                dest.ReleaseAndMakeRoute(source, route.Type);

                dest.CurrentSourceInfoKey = sourceItemKey;
                dest.CurrentSourceInfo = sourceItem;
            }
            return true;
        }

        public override void RoomVacatedForTimeoutPeriod(object o)
        {
            //Implement this
        }

        /// <summary>
        /// Does what it says
        /// </summary>
        public override void SetDefaultLevels()
        {
            Debug.Console(1, this, "Restoring default levels");
            var vc = CurrentVolumeControls as IBasicVolumeWithFeedback;
            if (vc != null)
                vc.SetVolume(DefaultVolume);
        }
        /// <summary>
        /// Will power the room on with the last-used source
        /// </summary>
        public override void PowerOnToDefaultOrLastSource()
        {
            if (!EnablePowerOnToLastSource || LastSourceKey == null)
                return;
            RunRouteAction(LastSourceKey, SourceListKey);
        }

        /// <summary>
        /// Runs "roomOff" action on all rooms not set to ExcludeFromGlobalFunctions
        /// </summary>
        public static void AllRoomsOff()
        {
            var allRooms = DeviceManager.AllDevices.Where(d =>
                d is EssentialsHuddleSpaceRoom && !(d as EssentialsHuddleSpaceRoom).ExcludeFromGlobalFunctions);
            foreach (var room in allRooms)
                (room as EssentialsHuddleSpaceRoom).RunRouteAction("roomOff", (room as EssentialsHuddleSpaceRoom).SourceListKey);
        }

        #region IPrivacy Members


        public void PrivacyModeOff()
        {
            VideoCodec.PrivacyModeOff();
        }

        public void PrivacyModeOn()
        {
            VideoCodec.PrivacyModeOn();
        }

        public void PrivacyModeToggle()
        {
            VideoCodec.PrivacyModeToggle();
        }

        #endregion
    }
}