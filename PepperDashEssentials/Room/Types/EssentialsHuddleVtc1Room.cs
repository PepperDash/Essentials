using System;
using System.Linq;
using Crestron.SimplSharp;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Room.Config;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDash.Essentials.Devices.Common.AudioCodec;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

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
		private string _lastSourceKey;

		/// <summary>
		/// The SourceListItem last run - containing names and icons 
		/// </summary>
		public SourceListItem CurrentSourceInfo
		{
			get { return _currentSourceInfo; }
			set
			{
				if (value == _currentSourceInfo) return;

				var handler = CurrentSourceChange;
				// remove from in-use tracker, if so equipped
				if(_currentSourceInfo != null && _currentSourceInfo.SourceDevice is IInUseTracking)
					(_currentSourceInfo.SourceDevice as IInUseTracking).InUseTracker.RemoveUser(this, "control");

				if (handler != null)
					handler(_currentSourceInfo, ChangeType.WillChange);

				_currentSourceInfo = value;

				// add to in-use tracking
				if (_currentSourceInfo != null && _currentSourceInfo.SourceDevice is IInUseTracking)
					(_currentSourceInfo.SourceDevice as IInUseTracking).InUseTracker.AddUser(this, "control");
				if (handler != null)
					handler(_CurrentSourceInfo, ChangeType.DidChange);

                var vc = VideoCodec as IHasExternalSourceSwitching;
                if (vc != null)
                {
                    vc.SetSelectedSource(CurrentSourceInfoKey);
                }
			}
		}
		private SourceListItem _currentSourceInfo;

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

        private readonly CCriticalSection _sourceSelectLock = new CCriticalSection();

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
                    AudioCodecBase;
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
                if (DefaultAudioDevice != null)
                    DefaultVolumeControls = DefaultAudioDevice;
                else if (DefaultAudioDevice is IHasVolumeDevice)
                    DefaultVolumeControls = (DefaultAudioDevice as IHasVolumeDevice).VolumeDevice;
                CurrentVolumeControls = DefaultVolumeControls;


                // Combines call feedback from both codecs if available
                InCallFeedback = new BoolFeedback(() =>
                {
                    var inAudioCall = false;
                    var inVideoCall = false;

                    if (AudioCodec != null)
                        inAudioCall = AudioCodec.IsInCall;

                    if (VideoCodec != null)
                        inVideoCall = VideoCodec.IsInCall;

                    return inAudioCall || inVideoCall;
                });

                // Get Microphone Privacy object, if any  MUST HAPPEN AFTER setting InCallFeedback
                MicrophonePrivacy = EssentialsRoomConfigHelper.GetMicrophonePrivacy(PropertiesConfig, this);

                Debug.Console(2, this, "Microphone Privacy Config evaluated.");

                // Get emergency object, if any
                Emergency = EssentialsRoomConfigHelper.GetEmergency(PropertiesConfig, this);

                Debug.Console(2, this, "Emergency Config evaluated.");


                VideoCodec.CallStatusChange += (o, a) => this.InCallFeedback.FireUpdate();
				VideoCodec.IsReadyChange += (o, a) => { this.SetCodecExternalSources(); SetCodecBranding(); }; 

                if (AudioCodec != null)
                    AudioCodec.CallStatusChange += (o, a) => InCallFeedback.FireUpdate();

                IsSharingFeedback = new BoolFeedback(() => VideoCodec.SharingContentIsOnFeedback.BoolValue);
                VideoCodec.SharingContentIsOnFeedback.OutputChange += (o, a) => IsSharingFeedback.FireUpdate();

                // link privacy to VC (for now?)
                PrivacyModeIsOnFeedback = new BoolFeedback(() => VideoCodec.PrivacyModeIsOnFeedback.BoolValue);
                VideoCodec.PrivacyModeIsOnFeedback.OutputChange += (o, a) => PrivacyModeIsOnFeedback.FireUpdate();

                CallTypeFeedback = new IntFeedback(() => 0);

                SourceListKey = "default";
                EnablePowerOnToLastSource = true;

                var disp = DefaultDisplay as DisplayBase;
                if (disp == null)
                {
                    return;
                }

                OnFeedbackFunc = () => CurrentSourceInfo != null
                                       && CurrentSourceInfo.Type == eSourceListItemType.Route;

                InitializeDisplay(disp);
                
            }
            catch (Exception e)
            {
                Debug.Console(0, this, "Error Initializing Room: {0}", e);
            }
   		}

        #region Overrides of EssentialsRoomBase

        protected override void PowerIsOnFeedbackOnOutputChange(object sender, FeedbackEventArgs args)
        {
            var disp = sender as DisplayBase;

            if (disp == null) return;

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
        }

        protected override void IsCoolingDownFeedbackOnOutputChange(object sender, FeedbackEventArgs args)
        {
            IsCoolingDownFeedback.FireUpdate();
        }

        protected override void IsWarmingUpFeedbackOnOutputChange(object sender, FeedbackEventArgs args)
        {
            IsWarmingUpFeedback.FireUpdate();

            if (IsWarmingUpFeedback.BoolValue)
            {
                return;
            }

            var basicVolumeWithFeedback = CurrentVolumeControls as IBasicVolumeWithFeedback;
            if (basicVolumeWithFeedback != null)
            {
                basicVolumeWithFeedback.SetVolume(DefaultVolume);
            }
        }


        #endregion

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
                SetRoomOccupancy(DeviceManager.GetDeviceForKey(PropertiesConfig.Occupancy.DeviceKey) as
                    IOccupancyStatusProvider, PropertiesConfig.Occupancy.TimeoutMinutes);
            }

            this.LogoUrlLightBkgnd = PropertiesConfig.LogoLight.GetLogoUrlLight();
            this.LogoUrlDarkBkgnd = PropertiesConfig.LogoDark.GetLogoUrlDark();

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
            VideoCodec.StopSharing();
            VideoCodec.StandbyActivate();
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
		public override void RunRouteAction(string routeKey)
		{
            RunRouteAction(routeKey, () => { });
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeKey"></param>
        /// <param name="souceListKey"></param>
        public void RunRouteAction(string routeKey, string souceListKey)
        {
            if (string.IsNullOrEmpty(sourceListKey))
            {
                Debug.Console(1, this, "No sourceListKey present.  RunRouteAction assumes default source list.");
                RunRouteAction(routeKey, new Action(() => { }));
            }
            else
            {
                Debug.Console(1, this, "sourceListKey present but not yet implemented");
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeKey"></param>
        /// <param name="souceListKey"></param>
        /// <param name="successCallback"></param>
        public void RunRouteAction(string routeKey, string sourceListKey, Action successCallback)
        {
            if (string.IsNullOrEmpty(sourceListKey))
            {
                RunRouteAction(routeKey, successCallback);
            }
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a source from config list SourceListKey and dynamically build and executes the
        /// route or commands
        /// </summary>
        public void RunRouteAction(string routeKey, Action successCallback)
        {
            // Run this on a separate thread
            //new CTimer
            CrestronInvoke.BeginInvoke(o =>
            {
				// try to prevent multiple simultaneous selections
				_sourceSelectLock.TryEnter();

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
                    if (!string.IsNullOrEmpty(_lastSourceKey))
                    {
                        var usageLastSource = dict[_lastSourceKey].SourceDevice as IUsageTracking;
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
             
                        _lastSourceKey = routeKey;
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
							var vol = (SavedVolumeLevels.ContainsKey(vd) ? (ushort)SavedVolumeLevels[vd] : DefaultVolume);
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

                    if (OnFeedback.BoolValue)
                    {
                        if (VideoCodec.UsageTracker.InUseTracker.InUseFeedback.BoolValue)
                        {
                            Debug.Console(1, this, "Video Codec in use, deactivating standby on codec");
                        }

                        if (VideoCodec.StandbyIsOnFeedback.BoolValue)
                        {
                            VideoCodec.StandbyDeactivate();
                        }
                    }

                    // report back when done
                    if (successCallback != null)
                        successCallback();
                }
                catch (Exception e)
                {
                    Debug.Console(1, this, "ERROR in routing: {0}", e);
                }

				_sourceSelectLock.Leave();
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
		private bool DoRoute(SourceRouteListItem route)
		{
			IRoutingSink dest;

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
                if (dest is IHasPowerControl)
                    (dest as IHasPowerControl).PowerOff();

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
            if (!EnablePowerOnToLastSource || _lastSourceKey == null)
                return;
            RunRouteAction(_lastSourceKey);
        }

		/// <summary>
		/// Runs "roomOff" action on all rooms not set to ExcludeFromGlobalFunctions
		/// </summary>
		public static void AllRoomsOff()
		{
			var allRooms = DeviceManager.AllDevices.Where(d => 
				d is EssentialsHuddleSpaceRoom && !(d as EssentialsHuddleSpaceRoom).ExcludeFromGlobalFunctions);
			foreach (var room in allRooms)
			{
			    var essentialsHuddleSpaceRoom = room as EssentialsHuddleSpaceRoom;
			    if (essentialsHuddleSpaceRoom != null)
			    {
			        essentialsHuddleSpaceRoom.RunRouteAction("roomOff");
			    }
			}
		}


        /// <summary>
        /// Setup the external sources for the Cisco Touch 10 devices that support IHasExternalSourceSwitch
        /// </summary>
        private void SetCodecExternalSources()
        {
            var videoCodecWithExternalSwitching = VideoCodec as IHasExternalSourceSwitching;

            if (videoCodecWithExternalSwitching == null || !videoCodecWithExternalSwitching.ExternalSourceListEnabled)
            {
                return;
            }

            try
            {
                // Get the tie line that the external switcher is connected to
                string codecInputConnectorName = ConfigReader.ConfigObject.TieLines.SingleOrDefault(
                    x => x.DestinationKey == VideoCodec.Key && x.DestinationPort == videoCodecWithExternalSwitching.ExternalSourceInputPort).DestinationPort;

                videoCodecWithExternalSwitching.ClearExternalSources();
                videoCodecWithExternalSwitching.RunRouteAction = RunRouteAction;
                var srcList = ConfigReader.ConfigObject.SourceLists.SingleOrDefault(x => x.Key == SourceListKey).Value.OrderBy(kv => kv.Value.Order); ;

                foreach (var kvp in srcList)
                {
                    var srcConfig = kvp.Value;

                    if (kvp.Key != DefaultCodecRouteString && kvp.Key != "roomOff")
                    {
                        videoCodecWithExternalSwitching.AddExternalSource(codecInputConnectorName, kvp.Key, srcConfig.PreferredName, PepperDash.Essentials.Devices.Common.VideoCodec.Cisco.eExternalSourceType.desktop);
                        videoCodecWithExternalSwitching.SetExternalSourceState(kvp.Key, PepperDash.Essentials.Devices.Common.VideoCodec.Cisco.eExternalSourceMode.Ready);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Console(2, this, "Error setting codec external sources: {0}", e);
            }
        }

        private void SetCodecBranding()
        {
            var vcWithBranding = VideoCodec as IHasBranding;

            if (vcWithBranding == null) return;

            Debug.Console(1, this, "Setting Codec Branding");
            vcWithBranding.InitializeBranding(Key);
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