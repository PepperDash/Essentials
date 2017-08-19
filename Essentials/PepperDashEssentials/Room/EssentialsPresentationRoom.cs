using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
	public class EssentialsPresentationRoom : EssentialsRoomBase, IHasCurrentSourceInfoChange
	{
		public event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;
		public event SourceInfoChangeHandler CurrentSingleSourceChange;
        public event SourceInfoChangeHandler CurrentDisplay1SourceChange;
        public event SourceInfoChangeHandler CurrentDisplay2SourceChange;

        protected override Func<bool> OnFeedbackFunc { get {
			return () => (CurrentSingleSourceInfo != null 
				&& CurrentSingleSourceInfo.Type != eSourceListItemType.Off)
                || (Display1SourceInfo != null 
                && Display1SourceInfo.Type != eSourceListItemType.Off)
                || (Display2SourceInfo != null
                && Display2SourceInfo.Type != eSourceListItemType.Off); } } 

        public EssentialsPresentationRoomPropertiesConfig Config { get; private set; }

        public Dictionary<uint, IRoutingSinkNoSwitching> Displays { get; private set; }

        public IRoutingSinkNoSwitching DefaultAudioDevice { get; private set; }
		public IBasicVolumeControls DefaultVolumeControls { get; private set; }

		/// <summary>
		/// The config name of the source list
		/// </summary>
		public string SourceListKey { get; set; }

		/// <summary>
		/// If room is off, enables power on to last source. Default true
		/// </summary>
		public bool EnablePowerOnToLastSource { get; set; }
		string LastSourceKey;

        public enum eVideoRoutingMode
        {
            SelectSourceSelectDisplay, SourceToAllDisplays
        }

        public eVideoRoutingMode VideoRoutingMode { get; set; }

        public enum eAudioRoutingMode
        {
            AudioFollowsLastVideo, SelectAudioFromDisplay
        }

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
		/// The SourceListItem last run - containing names and icons. The complex setter is 
        /// to add/remove this room to the source's InUseTracking, if it is capable
		/// </summary>
		public SourceListItem CurrentSingleSourceInfo
		{
			get { return _CurrentSingleSourceInfo; }
			private set
			{
				if (value == _CurrentSingleSourceInfo) return;

				var handler = CurrentSingleSourceChange;
				// remove from in-use tracker, if so equipped
				if(_CurrentSingleSourceInfo != null && _CurrentSingleSourceInfo.SourceDevice is IInUseTracking)
					(_CurrentSingleSourceInfo.SourceDevice as IInUseTracking).InUseTracker.RemoveUser(this, "control");

				if (handler != null)
					handler(this, _CurrentSingleSourceInfo, ChangeType.WillChange);

				_CurrentSingleSourceInfo = value;

				// add to in-use tracking
				if (_CurrentSingleSourceInfo != null && _CurrentSingleSourceInfo.SourceDevice is IInUseTracking)
					(_CurrentSingleSourceInfo.SourceDevice as IInUseTracking).InUseTracker.AddUser(this, "control");
				if (handler != null)
					handler(this, _CurrentSingleSourceInfo, ChangeType.DidChange);
			}
		}
		SourceListItem _CurrentSingleSourceInfo;

        public SourceListItem Display1SourceInfo
        {
            get { return _Display1SourceInfo; }
            set
            {
                if (value == _Display1SourceInfo) return;

                var handler = CurrentDisplay1SourceChange;
                if (handler != null)
                    handler(this, _Display1SourceInfo, ChangeType.WillChange);

                _Display1SourceInfo = value;

                if (handler != null)
                    handler(this, _Display1SourceInfo, ChangeType.DidChange);
            }
        }
        SourceListItem _Display1SourceInfo;

        public SourceListItem Display2SourceInfo
        {
            get { return _Display2SourceInfo; }
            set
            {
                if (value == _Display2SourceInfo) return;

                var handler = CurrentDisplay2SourceChange;
                if (handler != null)
                    handler(this, _Display2SourceInfo, ChangeType.WillChange);

                _Display2SourceInfo = value;

                if (handler != null)
                    handler(this, _Display2SourceInfo, ChangeType.DidChange);
            }
        }
        SourceListItem _Display2SourceInfo;

        /// <summary>
        /// If an audio dialer is available for this room
        /// </summary>
        public bool HasAudioDialer { get { return false; } }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
        public EssentialsPresentationRoom(string key, string name,
            Dictionary<uint, IRoutingSinkNoSwitching> displays,
            IBasicVolumeWithFeedback defaultVolume, EssentialsPresentationRoomPropertiesConfig config)
			: base(key, name)
		{
			Config = config;
			Displays = displays;

            DefaultVolumeControls = defaultVolume;
            CurrentVolumeControls = defaultVolume;

            //DefaultAudioDevice = defaultAudio;
            //if (defaultAudio is IBasicVolumeControls)
            //    DefaultVolumeControls = defaultAudio as IBasicVolumeControls;
            //else if (defaultAudio is IHasVolumeDevice)
            //    DefaultVolumeControls = (defaultAudio as IHasVolumeDevice).VolumeDevice;

			
			SourceListKey = "default";
			EnablePowerOnToLastSource = true;
		}

        /// <summary>
        /// Run the same source to all destinations
        /// </summary>
        /// <param name="sourceItem"></param>
        public void RouteSourceToAllDestinations(SourceListItem sourceItem)
        {
            if (Config.Volumes.Master != null)
            {
                var audioDev = DeviceManager.GetDeviceForKey(Config.Volumes.Master.DeviceKey);
                if (audioDev is IBasicVolumeWithFeedback)
                {

                }
            }

            foreach (var display in Displays.Values)
            {
                if (sourceItem != null)
                    DoVideoRoute(sourceItem.SourceKey, display.Key);
                else
                    DoVideoRoute("$off", display.Key);
            }
            Display1SourceInfo = sourceItem;
            Display2SourceInfo = sourceItem;
            CurrentSingleSourceInfo = sourceItem;
            OnFeedback.FireUpdate();
        }

        public void SourceToDisplay1(SourceListItem sourceItem)
        {
            DoVideoRoute(sourceItem.SourceKey, Displays[1].Key);
            Display1SourceInfo = sourceItem;
            OnFeedback.FireUpdate();
        }

        public void SourceToDisplay2(SourceListItem sourceItem)
        {
            DoVideoRoute(sourceItem.SourceKey, Displays[2].Key);
            Display2SourceInfo = sourceItem;
            OnFeedback.FireUpdate();
        }


        /// <summary>
        /// Basic source -> destination routing
        /// </summary>
        void DoVideoRoute(string sourceKey, string destinationKey)
        {
            new CTimer(o =>
                {
                    var dest = DeviceManager.GetDeviceForKey(destinationKey) as IRoutingSinkNoSwitching;
                    if (dest == null)
                    {
                        Debug.Console(1, this, "Cannot route. Destination '{0}' not found", destinationKey);
                        return;
                    }
                    // off is special case
                    if (sourceKey.Equals("$off", StringComparison.OrdinalIgnoreCase))
                    {
                        dest.ReleaseRoute();
                        if (dest is IPower)
                            (dest as IPower).PowerOff();
                        return;
                    }

                    var source = DeviceManager.GetDeviceForKey(sourceKey) as IRoutingOutputs;
                    if (source == null)
                    {
                        Debug.Console(1, this, "Cannot route. Source '{0}' not found", sourceKey);
                        return;
                    }
                    dest.ReleaseAndMakeRoute(source, eRoutingSignalType.Video);
                }, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Shutdown()
        {
            RunRouteAction("roomoff");
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
					Debug.Console(1, this, "Run room action '{0}'", routeKey);
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
					Debug.Console(2, this, "Action {0} has {1} steps",
						item.SourceKey, item.RouteList.Count);

					// Let's run it
					if (routeKey.ToLower() != "roomoff")
						LastSourceKey = routeKey;

					foreach (var route in item.RouteList)
					{
						// if there is a $defaultAll on route, run two separate
						if (route.DestinationKey.Equals("$defaultAll", StringComparison.OrdinalIgnoreCase))
						{
							var tempAudio = new SourceRouteListItem
							{
								DestinationKey = "$defaultDisplay",
								SourceKey = route.SourceKey,
								Type = eRoutingSignalType.Video
							};
							DoRoute(tempAudio);

							var tempVideo = new SourceRouteListItem
							{
								DestinationKey = "$defaultAudio",
								SourceKey = route.SourceKey,
								Type = eRoutingSignalType.Audio
							};
							DoRoute(tempVideo);
							continue;
						}
						else
							DoRoute(route);
					}

					// Set volume control on room, using default if non provided
					IBasicVolumeControls volDev = null;
					// Handle special cases for volume control
					if (string.IsNullOrEmpty(item.VolumeControlKey) 
						|| item.VolumeControlKey.Equals("$defaultAudio", StringComparison.OrdinalIgnoreCase))
						volDev = DefaultVolumeControls;
                    //else if (item.VolumeControlKey.Equals("$defaultDisplay", StringComparison.OrdinalIgnoreCase))
                    //    volDev = DefaultDisplay as IBasicVolumeControls;
					// Or a specific device, probably rarely used.
					else
					{
						var dev = DeviceManager.GetDeviceForKey(item.VolumeControlKey);
						if (dev is IBasicVolumeControls)
							volDev = dev as IBasicVolumeControls;
						else if (dev is IHasVolumeDevice)
							volDev = (dev as IHasVolumeDevice).VolumeDevice;
					}
					CurrentVolumeControls = volDev;

					// store the name and UI info for routes
					if (item.SourceKey != null)
						CurrentSingleSourceInfo = item;
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
		public void PowerOnToDefaultOrLastSource()
		{
			if (!EnablePowerOnToLastSource || LastSourceKey == null)
				return;
			RunRouteAction(LastSourceKey);
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
				dest = DefaultAudioDevice;
            //else if (route.DestinationKey.Equals("$defaultDisplay", StringComparison.OrdinalIgnoreCase))
            //    dest = DefaultDisplay;
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
	}
}