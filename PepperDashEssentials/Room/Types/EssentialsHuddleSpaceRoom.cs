using System;
using System.Linq;
using Crestron.SimplSharp;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials
{
    public class EssentialsHuddleSpaceRoom : EssentialsRoomBase, IHasCurrentSourceInfoChange, IRunRouteAction, IRunDefaultPresentRoute, IHasCurrentVolumeControls, IHasDefaultDisplay
	{
		//
		public event SourceInfoChangeHandler CurrentSourceChange;

		public EssentialsHuddleRoomPropertiesConfig PropertiesConfig { get; private set; }

		public IRoutingSinkWithSwitching DefaultDisplay { get; private set; }
		public IRoutingSink DefaultAudioDevice { get; private set; }
		public IBasicVolumeControls DefaultVolumeControls { get; private set; }

		public bool ExcludeFromGlobalFunctions { get; set; }

        public string DefaultSourceItem { get; set; }

        public ushort DefaultVolume { get; set; }

		/// <summary>
		/// If room is off, enables power on to last source. Default true
		/// </summary>
		public bool EnablePowerOnToLastSource { get; set; }
		string _lastSourceKey;

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
					handler( _currentSourceInfo, ChangeType.DidChange);
			}
		}
		SourceListItem _currentSourceInfo;

        public string CurrentSourceInfoKey { get; set; }

        public EssentialsHuddleSpaceRoom(DeviceConfig config)
            : base(config)
        {
            try
            {
                PropertiesConfig = JsonConvert.DeserializeObject<EssentialsHuddleRoomPropertiesConfig>
                    (config.Properties.ToString());
                DefaultDisplay = DeviceManager.GetDeviceForKey(PropertiesConfig.DefaultDisplayKey) as IRoutingSinkWithSwitching;


                DefaultAudioDevice = DeviceManager.GetDeviceForKey(PropertiesConfig.DefaultAudioKey) as IRoutingSinkWithSwitching;

                Initialize();
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error building room: \n{0}", e);
            }
        }

		private void Initialize()
		{
            if (DefaultAudioDevice is IBasicVolumeControls)
                DefaultVolumeControls = DefaultAudioDevice as IBasicVolumeControls;
            else if (DefaultAudioDevice is IHasVolumeDevice)
                DefaultVolumeControls = (DefaultAudioDevice as IHasVolumeDevice).VolumeDevice;
            CurrentVolumeControls = DefaultVolumeControls;

            SourceListKey = "default";
            EnablePowerOnToLastSource = true;

            var disp = DefaultDisplay as DisplayBase;
		    if (disp == null) return;

            IsWarmingFeedbackFunc = () => disp.IsWarmingUpFeedback.BoolValue;

            IsCoolingFeedbackFunc = () => disp.IsCoolingDownFeedback.BoolValue;

            OnFeedbackFunc = () => CurrentSourceInfo != null
                                       && CurrentSourceInfo.Type == eSourceListItemType.Route;

            InitializeDisplay(disp);
		}


        protected override void IsCoolingDownFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            IsCoolingDownFeedback.FireUpdate();
        }

        protected override void PowerIsOnFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            var display = sender as DisplayBase;

            if (display == null) return;

            if (display.PowerIsOnFeedback.BoolValue == OnFeedback.BoolValue)
            {
                // Link power, warming, cooling to display
                 var dispTwoWay = disp as IHasPowerControlWithFeedback;
                 if (dispTwoWay != null)
                 {
                     dispTwoWay.PowerIsOnFeedback.OutputChange += (o, a) =>
                         {
                             if (dispTwoWay.PowerIsOnFeedback.BoolValue != OnFeedback.BoolValue)
                             {
                                 if (!dispTwoWay.PowerIsOnFeedback.BoolValue)
                                     CurrentSourceInfo = null;
                                 OnFeedback.FireUpdate();
                             }
                         };
                 }
            }

            if (!display.PowerIsOnFeedback.BoolValue)
            {
                CurrentSourceInfo = null;
            }
            OnFeedback.FireUpdate();
        }

        protected override void IsWarmingUpFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            IsWarmingUpFeedback.FireUpdate();

            if (IsWarmingUpFeedback.BoolValue)
            {
                return;
            }

            var displayVolumeControl = DefaultDisplay as IBasicVolumeWithFeedback;

            if (displayVolumeControl == null)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Error,
                    "Default display {0} is not volume control control provider", DefaultDisplay.Key);
                return;
            }

            displayVolumeControl.SetVolume(DefaultVolume);
        }

        protected override void CustomSetConfig(DeviceConfig config)
        {
            var newPropertiesConfig = JsonConvert.DeserializeObject<EssentialsHuddleRoomPropertiesConfig>(config.Properties.ToString());

            if (newPropertiesConfig != null)
                PropertiesConfig = newPropertiesConfig;

            ConfigWriter.UpdateRoomConfig(config);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void EndShutdown()
        {
            SetDefaultLevels();

            RunDefaultPresentRoute();

            CrestronEnvironment.Sleep(1000);

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Shutting down room");

            RunRouteAction("roomOff");
        }

        /// <summary>
        /// Routes the default source item, if any
        /// </summary>
        public override bool RunDefaultPresentRoute()
        {
            if (DefaultSourceItem == null)
            {
                Debug.Console(0, this, "Unable to run default present route, DefaultSourceItem is null.");
                return false;
            }

            RunRouteAction(DefaultSourceItem);
			return true;
        }

        public override bool CustomActivate()
        {
            // Add Occupancy object from config
            if (PropertiesConfig.Occupancy != null)
                SetRoomOccupancy(DeviceManager.GetDeviceForKey(PropertiesConfig.Occupancy.DeviceKey) as
                    IOccupancyStatusProvider, PropertiesConfig.Occupancy.TimeoutMinutes);

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
        /// <param name="routeKey"></param>
		public void RunRouteAction(string routeKey)
		{
            RunRouteAction(routeKey, () => { });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeKey"></param>
        /// <param name="sourceListKey"></param>
        public void RunRouteAction(string routeKey, string sourceListKey)
        {
            RunRouteAction(routeKey, new Action(() => { }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeKey"></param>
        /// <param name="sourceListKey"></param>
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

                var item = dict[routeKey];
                //Debug.Console(2, this, "Action {0} has {1} steps",
                //    item.SourceKey, item.RouteList.Count);

                // End usage timer on last source
                if (!string.IsNullOrEmpty(_lastSourceKey))
                {
                    var lastSource = dict[_lastSourceKey].SourceDevice;

                    try
                    {
                        if (lastSource is IUsageTracking)
                            (lastSource as IUsageTracking).UsageTracker.EndDeviceUsage();
                    }
                    catch (Exception e)
                    {
                        Debug.Console(1, this, "*#* EXCEPTION in end usage tracking (257):\r{0}", e);
                    }
                }

                // Let's run it
                if (routeKey.ToLower() != "roomoff")
                {
                    _lastSourceKey = routeKey;
                }
                else
                {
                    CurrentSourceInfoKey = null;
                }

                foreach (var route in item.RouteList)
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

                        //var tempAudio = new SourceRouteListItem
                        //{
                        //    DestinationKey = "$defaultAudio",
                        //    SourceKey = route.SourceKey,
                        //    Type = eRoutingSignalType.Audio
                        //};
                        //DoRoute(tempAudio);
                        //continue; -- not sure why this was here
                    }
                    else
                        DoRoute(route);
                }

                // Start usage timer on routed source
                if (item.SourceDevice is IUsageTracking)
                {
                    (item.SourceDevice as IUsageTracking).UsageTracker.StartDeviceUsage();
                }




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
                        SavedVolumeLevels[vd] = (uint) vd.VolumeLevelFeedback.IntValue;
                        vd.SetVolume(0);
                    }
                    CurrentVolumeControls = volDev;
                    if (ZeroVolumeWhenSwtichingVolumeDevices && CurrentVolumeControls is IBasicVolumeWithFeedback)
                    {
                        var vd = CurrentVolumeControls as IBasicVolumeWithFeedback;
                        ushort vol = (SavedVolumeLevels.ContainsKey(vd) ? (ushort) SavedVolumeLevels[vd] : DefaultVolume);
                        vd.SetVolume(vol);
                    }
                }



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
                // And finally, set the "control".  This will trigger event
                //CurrentControlDevice = DeviceManager.GetDeviceForKey(item.SourceKey) as Device;

                OnFeedback.FireUpdate();

                // report back when done
                if (successCallback != null)
                    successCallback();

            }, 0); // end of CTimer
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
		/// 
		/// </summary>
		/// <param name="route"></param>
		/// <returns></returns>
		private bool DoRoute(SourceRouteListItem route)
		{
			IRoutingSink dest;

			if (route.DestinationKey.Equals("$defaultaudio", StringComparison.OrdinalIgnoreCase))
				dest = DefaultAudioDevice;
			else if (route.DestinationKey.Equals("$defaultDisplay", StringComparison.OrdinalIgnoreCase))
				dest = DefaultDisplay;
			else
				dest = DeviceManager.GetDeviceForKey(route.DestinationKey) as IRoutingSink;

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
		/// Runs "roomOff" action on all rooms not set to ExcludeFromGlobalFunctions
		/// </summary>
		public static void AllRoomsOff()
		{
		    var allRooms = DeviceManager.AllDevices.OfType<EssentialsHuddleSpaceRoom>().Where(d =>
		        !d.ExcludeFromGlobalFunctions);
			foreach (var room in allRooms)
				room.RunRouteAction("roomOff");
		}
	}
}