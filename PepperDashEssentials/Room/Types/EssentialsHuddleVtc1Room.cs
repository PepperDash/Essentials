using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Room.Config;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDash.Essentials.Devices.Common.AudioCodec;

namespace PepperDash.Essentials
{
    public class EssentialsHuddleVtc1Room : EssentialsRoomBase, IHasCurrentSourceInfoChange,
        IPrivacy, IHasCurrentVolumeControls, IRunRouteAction, IRunDefaultCallRoute, IHasVideoCodec, IHasAudioCodec, IHasDefaultDisplay, IHasInCallFeedback
	{
		public event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;
		public event SourceInfoChangeHandler CurrentSourceChange;


        //************************
        // Call-related stuff

        public BoolFeedback InCallFeedback { get; private set; }

        ///// <summary>
        ///// Make this more specific
        ///// </summary>
        //public List<CodecActiveCallItem> ActiveCalls { get; private set; }

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

        //************************


        protected override Func<bool> OnFeedbackFunc
        {
            get
            {
                return () =>
                {
                    var disp = DefaultDisplay as DisplayBase;
                    var val = CurrentSourceInfo != null
                        && CurrentSourceInfo.Type == eSourceListItemType.Route
                        && disp != null;
						//&& disp.PowerIsOnFeedback.BoolValue;
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
                    var disp = DefaultDisplay as DisplayBase;
                    if (disp != null)
                        return disp.IsWarmingUpFeedback.BoolValue;
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
                    var disp = DefaultDisplay as DisplayBase;
                    if (disp != null)
                        return disp.IsCoolingDownFeedback.BoolValue;
                    else
                        return false;
                };
            }
        }

        public EssentialsHuddleVtc1PropertiesConfig PropertiesConfig { get; private set; }

		public IRoutingSinkWithSwitching DefaultDisplay { get; private set; }
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
			get { return _CurrentAudioDevice;  }
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
		/// The SourceListItem last run - containing names and icons 
		/// </summary>
		public SourceListItem CurrentSourceInfo
		{
			get { return _CurrentSourceInfo; }
			set
			{
				if (value == _CurrentSourceInfo) return;

				var handler = CurrentSourceChange;
				// remove from in-use tracker, if so equipped
				if(_CurrentSourceInfo != null && _CurrentSourceInfo.SourceDevice is IInUseTracking)
					(_CurrentSourceInfo.SourceDevice as IInUseTracking).InUseTracker.RemoveUser(this, "control");

				if (handler != null)
					handler(_CurrentSourceInfo, ChangeType.WillChange);

				_CurrentSourceInfo = value;

				// add to in-use tracking
				if (_CurrentSourceInfo != null && _CurrentSourceInfo.SourceDevice is IInUseTracking)
					(_CurrentSourceInfo.SourceDevice as IInUseTracking).InUseTracker.AddUser(this, "control");
				if (handler != null)
					handler(_CurrentSourceInfo, ChangeType.DidChange);
			}
		}
		SourceListItem _CurrentSourceInfo;

        public string CurrentSourceInfoKey { get; set; }

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

        public EssentialsHuddleVtc1Room(DeviceConfig config)
            : base(config)
        {
            try
            {
                PropertiesConfig = JsonConvert.DeserializeObject<EssentialsHuddleVtc1PropertiesConfig>
                    (config.Properties.ToString());
                DefaultDisplay = DeviceManager.GetDeviceForKey(PropertiesConfig.DefaultDisplayKey) as IRoutingSinkWithSwitching;

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
                Debug.Console(1, this, "Error building room: \n{0}", e);
            }
        }

        void Initialize()
		{
            try
            {
                if (DefaultAudioDevice is IBasicVolumeControls)
                    DefaultVolumeControls = DefaultAudioDevice as IBasicVolumeControls;
                else if (DefaultAudioDevice is IHasVolumeDevice)
                    DefaultVolumeControls = (DefaultAudioDevice as IHasVolumeDevice).VolumeDevice;
                CurrentVolumeControls = DefaultVolumeControls;


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

                var disp = DefaultDisplay as DisplayBase;
                if (disp != null)
                {
                    // Link power, warming, cooling to display
                    disp.PowerIsOnFeedback.OutputChange += (o, a) =>
                        {
                            if (disp.PowerIsOnFeedback.BoolValue != OnFeedback.BoolValue)
                            {
                                if (!disp.PowerIsOnFeedback.BoolValue)
                                    CurrentSourceInfo = null;
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



                // Get Microphone Privacy object, if any  MUST HAPPEN AFTER setting InCallFeedback
                this.MicrophonePrivacy = EssentialsRoomConfigHelper.GetMicrophonePrivacy(PropertiesConfig, this);

                Debug.Console(2, this, "Microphone Privacy Config evaluated.");

                // Get emergency object, if any
                this.Emergency = EssentialsRoomConfigHelper.GetEmergency(PropertiesConfig, this);

                Debug.Console(2, this, "Emergency Config evaluated.");


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
            catch (Exception e)
            {
                Debug.Console(0, this, "Error Initializing Room: {0}", e);
            }
   		}

        protected override void CustomSetConfig(DeviceConfig config)
        {
            var newPropertiesConfig = JsonConvert.DeserializeObject<EssentialsHuddleVtc1PropertiesConfig>(config.Properties.ToString());

            if (newPropertiesConfig != null)
                PropertiesConfig = newPropertiesConfig;

            ConfigWriter.UpdateRoomConfig(config);
        }

        public override bool CustomActivate()
        {
            // Add Occupancy object from config
            if (PropertiesConfig.Occupancy != null)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Setting Occupancy Provider for room");
                this.SetRoomOccupancy(DeviceManager.GetDeviceForKey(PropertiesConfig.Occupancy.DeviceKey) as
                    IOccupancyStatusProvider, PropertiesConfig.Occupancy.TimeoutMinutes);
            }

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

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Shutting down room");

            RunRouteAction("roomOff");
        }

        /// <summary>
        /// Routes the default source item, if any. Returns true when default route exists
        /// </summary>
        public override bool RunDefaultPresentRoute()
        {
			if (DefaultSourceItem != null)
                RunRouteAction(DefaultSourceItem);

            return DefaultSourceItem != null;
        }

        /// <summary>
        /// Sets up the room when started into call mode without presenting a source
        /// </summary>
        /// <returns></returns>
        public bool RunDefaultCallRoute()
        {
            RunRouteAction(DefaultCodecRouteString);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeKey"></param>
		public void RunRouteAction(string routeKey)
		{
            RunRouteAction(routeKey, new Action(() => { }));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeKey"></param>
        /// <param name="souceListKey"></param>
        /// <param name="successCallback"></param>
        public void RunRouteAction(string routeKey, string souceListKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeKey"></param>
        /// <param name="souceListKey"></param>
        /// <param name="successCallback"></param>
        public void RunRouteAction(string routeKey, string souceListKey, Action successCallback)
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// Gets a source from config list SourceListKey and dynamically build and executes the
		/// route or commands
		/// </summary>
		/// <param name="name"></param>
        public void RunRouteAction(string routeKey, Action successCallback)
        {
            // Run this on a separate thread
            new CTimer(o =>
            {
				// try to prevent multiple simultaneous selections
				SourceSelectLock.TryEnter();

                try
                {

                    Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Run route action '{0}'", routeKey);
                    var dict = ConfigReader.ConfigObject.GetSourceListForKey(SourceListKey);
                    if (dict == null)
                    {
                        Debug.Console(1, this, "WARNING: Config source list '{0}' not found", SourceListKey);
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
                    else
                        CurrentSourceInfoKey = null;

                    // hand off the individual routes to this helper
                    foreach (var route in item.RouteList)
                        DoRouteItem(route);

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
					else if (item.VolumeControlKey.Equals("$defaultDisplay", StringComparison.OrdinalIgnoreCase))
						volDev = DefaultDisplay as IBasicVolumeControls;
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
                        CurrentSourceInfoKey = routeKey;
                        CurrentSourceInfo = null;
                    }
                    else if (item.SourceKey != null)
                    {
                        CurrentSourceInfoKey = routeKey;
                        CurrentSourceInfo = item;
                    }

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
        void DoRouteItem(SourceRouteListItem route)
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
                DoRoute(tempVideo);
            }
            else
                DoRoute(route);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="route"></param>
		/// <returns></returns>
		bool DoRoute(SourceRouteListItem route)
		{
			IRoutingSink dest = null;

			if (route.DestinationKey.Equals("$defaultaudio", StringComparison.OrdinalIgnoreCase))
                dest = DefaultAudioDevice as IRoutingSinkNoSwitching;
			else if (route.DestinationKey.Equals("$defaultDisplay", StringComparison.OrdinalIgnoreCase))
				dest = DefaultDisplay;
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
            RunRouteAction(LastSourceKey);
        }

		/// <summary>
		/// Runs "roomOff" action on all rooms not set to ExcludeFromGlobalFunctions
		/// </summary>
		public static void AllRoomsOff()
		{
			var allRooms = DeviceManager.AllDevices.Where(d => 
				d is EssentialsHuddleSpaceRoom && !(d as EssentialsHuddleSpaceRoom).ExcludeFromGlobalFunctions);
			foreach (var room in allRooms)
				(room as EssentialsHuddleSpaceRoom).RunRouteAction("roomOff");
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