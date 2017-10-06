using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Room.Config;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials
{
	public class EssentialsHuddleVtc1Room : EssentialsRoomBase, IHasCurrentSourceInfoChange, IPrivacy
	{
		public event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;
		public event SourceInfoChangeHandler CurrentSingleSourceChange;


        //************************
        // Call-related stuff

        public BoolFeedback InCallFeedback { get; private set; }

        /// <summary>
        /// Make this more specific
        /// </summary>
        public List<CodecActiveCallItem> ActiveCalls { get; private set; }

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
                        && disp != null
                        && disp.PowerIsOnFeedback.BoolValue;
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

        public EssentialsHuddleVtc1PropertiesConfig Config { get; private set; }

		public IRoutingSinkWithSwitching DefaultDisplay { get; private set; }
		public IBasicVolumeControls DefaultAudioDevice { get; private set; }
		public IBasicVolumeControls DefaultVolumeControls { get; private set; }

        public VideoCodecBase VideoCodec { get; private set; }

		public bool ExcludeFromGlobalFunctions { get; set; }

		/// <summary>
		/// The config name of the source list
		/// </summary>
		public string SourceListKey { get; set; }
        
        public string DefaultSourceItem { get; set; }

        public ushort DefaultVolume { get; set; }

		/// <summary>
		/// If room is off, enables power on to last source. Default true
		/// </summary>
		public bool EnablePowerOnToLastSource { get; set; }
		string LastSourceKey;

		/// <summary>
		/// 
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
			private set
			{
				if (value == _CurrentSourceInfo) return;

				var handler = CurrentSingleSourceChange;
				// remove from in-use tracker, if so equipped
				if(_CurrentSourceInfo != null && _CurrentSourceInfo.SourceDevice is IInUseTracking)
					(_CurrentSourceInfo.SourceDevice as IInUseTracking).InUseTracker.RemoveUser(this, "control");

				if (handler != null)
					handler(this, _CurrentSourceInfo, ChangeType.WillChange);

				_CurrentSourceInfo = value;

				// add to in-use tracking
				if (_CurrentSourceInfo != null && _CurrentSourceInfo.SourceDevice is IInUseTracking)
					(_CurrentSourceInfo.SourceDevice as IInUseTracking).InUseTracker.AddUser(this, "control");
				if (handler != null)
					handler(this, _CurrentSourceInfo, ChangeType.DidChange);
			}
		}
		SourceListItem _CurrentSourceInfo;

        public string CurrentSourceInfoKey { get; private set; }

        /// <summary>
        /// "codecOsd"
        /// </summary>
        public string DefaultCodecRouteString { get { return "codecOsd"; } } 

        /// <summary>
        /// Temporary implementation. Returns the schedule-ready object or null if none. Fow now,
        /// always returns the VideoCodec if it is capable
        /// </summary>
        public IHasScheduleAwareness ScheduleSource { get { return VideoCodec as IHasScheduleAwareness; } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
        public EssentialsHuddleVtc1Room(string key, string name, IRoutingSinkWithSwitching defaultDisplay, 
			IBasicVolumeControls defaultAudio, VideoCodecBase codec, EssentialsHuddleVtc1PropertiesConfig config)
			: base(key, name)
		{
            if (codec == null)
                throw new ArgumentNullException("codec cannot be null");
			Config = config;
			DefaultDisplay = defaultDisplay;
            VideoCodec = codec;
			DefaultAudioDevice = defaultAudio;

			if (defaultAudio is IBasicVolumeControls)
				DefaultVolumeControls = defaultAudio as IBasicVolumeControls;
			else if (defaultAudio is IHasVolumeDevice)
				DefaultVolumeControls = (defaultAudio as IHasVolumeDevice).VolumeDevice;
            CurrentVolumeControls = DefaultVolumeControls;

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
                        (DefaultDisplay as IBasicVolumeWithFeedback).SetVolume(DefaultVolume);
                };
                disp.IsCoolingDownFeedback.OutputChange += (o, a) => 
                {
                    IsCoolingDownFeedback.FireUpdate(); 
                    if (IsCoolingDownFeedback.BoolValue)
                        (DefaultDisplay as IBasicVolumeWithFeedback).SetVolume(DefaultVolume);
                };
            }

            InCallFeedback = new BoolFeedback(() => VideoCodec.IsInCall);
            IsSharingFeedback = new BoolFeedback(() => VideoCodec.SharingSourceFeedback.StringValue != null); 

            // link privacy to VC (for now?)
            PrivacyModeIsOnFeedback = new BoolFeedback(() => VideoCodec.PrivacyModeIsOnFeedback.BoolValue);
            VideoCodec.PrivacyModeIsOnFeedback.OutputChange += (o, a) => this.PrivacyModeIsOnFeedback.FireUpdate();

            CallTypeFeedback = new IntFeedback(() => 0);
          
			SourceListKey = "default";
			EnablePowerOnToLastSource = true;
   		}


        /// <summary>
        /// 
        /// </summary>
        public override void Shutdown()
        {
            RunRouteAction("roomOff");
            VideoCodec.EndAllCalls();
        }

        /// <summary>
        /// Routes the default source item, if any. Returns true when default route exists
        /// </summary>
        public bool RunDefaultPresentRoute()
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
			RunRouteAction(routeKey, null);
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
                try
                {

                    Debug.Console(1, this, "Run route action '{0}'", routeKey);
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
			IRoutingSinkNoSwitching dest = null;

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
        public void PowerOnToDefaultOrLastSource()
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