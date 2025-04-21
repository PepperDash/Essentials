﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials
{
    public class EssentialsHuddleSpaceRoom : EssentialsRoomBase, IEssentialsHuddleSpaceRoom
	{
		public event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;
		public event SourceInfoChangeHandler CurrentSourceChange;

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
					handler( _CurrentSourceInfo, ChangeType.DidChange);
			}
		}
		SourceListItem _CurrentSourceInfo;

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

                InitializeRoom();
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error building room: \n{0}", e);
            }
        }

		void InitializeRoom()
		{
            if (DefaultAudioDevice is IBasicVolumeControls)
                DefaultVolumeControls = DefaultAudioDevice as IBasicVolumeControls;
            else if (DefaultAudioDevice is IHasVolumeDevice)
                DefaultVolumeControls = (DefaultAudioDevice as IHasVolumeDevice).VolumeDevice;
            CurrentVolumeControls = DefaultVolumeControls;

            var disp = DefaultDisplay as DisplayBase;
            if (disp != null)
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

                disp.IsWarmingUpFeedback.OutputChange += (o, a) => 
                { 
                    IsWarmingUpFeedback.FireUpdate();
                    if (!IsWarmingUpFeedback.BoolValue)
                        (DefaultDisplay as IBasicVolumeWithFeedback).SetVolume(DefaultVolume);
                };
                disp.IsCoolingDownFeedback.OutputChange += (o, a) => 
                {
                    IsCoolingDownFeedback.FireUpdate(); 
                };
            }

            SetupEnvironmentalControlDevices();

            SetSourceListKey();
          
			EnablePowerOnToLastSource = true;
   		}

        private void SetupEnvironmentalControlDevices()
        {
            if (PropertiesConfig.Environment != null)
            {
                if (PropertiesConfig.Environment.Enabled)
                {
                    foreach (var d in PropertiesConfig.Environment.DeviceKeys)
                    {
                        var envDevice = DeviceManager.GetDeviceForKey(d) as EssentialsDevice;
                        EnvironmentalControlDevices.Add(envDevice);
                    }
                }
            }
        }

        private void SetSourceListKey()
        {
            if (!string.IsNullOrEmpty(PropertiesConfig.SourceListKey))
            {
                SetSourceListKey(PropertiesConfig.SourceListKey);
            }
            else
            {
                SetSourceListKey(Key);
            }

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
                this.SetRoomOccupancy(DeviceManager.GetDeviceForKey(PropertiesConfig.Occupancy.DeviceKey) as
                    IOccupancyStatusProvider, PropertiesConfig.Occupancy.TimeoutMinutes);

            this.LogoUrlLightBkgnd = PropertiesConfig.LogoLight.GetLogoUrlLight();
            this.LogoUrlDarkBkgnd = PropertiesConfig.LogoDark.GetLogoUrlDark();
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
            RunRouteAction(routeKey, new Action(() => { }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeKey"></param>
        /// <param name="souceListKey"></param>
        /// <param name="successCallback"></param>
        public void RunRouteAction(string routeKey, string sourceListKey)
        {
            RunRouteAction(routeKey, new Action(() => { }));
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
		/// <param name="name"></param>
		public void RunRouteAction(string routeKey, Action successCallback)
		{
			// Run this on a separate thread
			new CTimer(o =>
				{
                    Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Run route action '{0}'", routeKey);
                    var dict = ConfigReader.ConfigObject.GetSourceListForKey(SourceListKey);
					if(dict == null)
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
                    if (!string.IsNullOrEmpty(LastSourceKey))
                    {
                        var lastSource = dict[LastSourceKey].SourceDevice;

                        try
                        {
                            if (lastSource != null && lastSource is IUsageTracking)
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
                        LastSourceKey = routeKey;
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
			if (!EnablePowerOnToLastSource || LastSourceKey == null)
				return;
			RunRouteAction(LastSourceKey);
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
		bool DoRoute(SourceRouteListItem route)
		{
			IRoutingSink dest = null;

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
			var allRooms = DeviceManager.AllDevices.Where(d => 
				d is EssentialsHuddleSpaceRoom && !(d as EssentialsHuddleSpaceRoom).ExcludeFromGlobalFunctions);
			foreach (var room in allRooms)
				(room as EssentialsHuddleSpaceRoom).RunRouteAction("roomOff");
		}
	}
}