using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
	public class EssentialsHuddleSpaceRoom : EssentialsRoomBase, IHasCurrentSourceInfoChange
	{
		public event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;
		public event SourceInfoChangeHandler CurrentSingleSourceChange;

		public EssentialsRoomPropertiesConfig Config { get; private set; }

		public IRoutingSinkWithSwitching DefaultDisplay { get; private set; }
		public IRoutingSinkNoSwitching DefaultAudioDevice { get; private set; }
		public IBasicVolumeControls DefaultVolumeControls { get; private set; }

		public bool ExcludeFromGlobalFunctions { get; set; }

		/// <summary>
		/// The config name of the source list
		/// </summary>
		public string SourceListKey { get; set; }

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

		/// <summary>
		/// 
		/// </summary>
		public BoolFeedback OnFeedback { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		public EssentialsHuddleSpaceRoom(string key, string name, IRoutingSinkWithSwitching defaultDisplay, 
			IRoutingSinkNoSwitching defaultAudio, EssentialsRoomPropertiesConfig config)
			: base(key, name)
		{
			Config = config;
			DefaultDisplay = defaultDisplay;
			DefaultAudioDevice = defaultAudio;
			if (defaultAudio is IBasicVolumeControls)
				DefaultVolumeControls = defaultAudio as IBasicVolumeControls;
			else if (defaultAudio is IHasVolumeDevice)
				DefaultVolumeControls = (defaultAudio as IHasVolumeDevice).VolumeDevice;
            CurrentVolumeControls = DefaultVolumeControls;

			OnFeedback = new BoolFeedback(() =>
				{ return CurrentSourceInfo != null 
					&& CurrentSourceInfo.Type == eSourceListItemType.Route; });
			SourceListKey = "default";
			EnablePowerOnToLastSource = true;
		}

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
					CurrentVolumeControls = volDev;

					// store the name and UI info for routes
					if (item.SourceKey != null)
						CurrentSourceInfo = item;
					// And finally, set the "control".  This will trigger event
					//CurrentControlDevice = DeviceManager.GetDeviceForKey(item.SourceKey) as Device;

					OnFeedback.FireUpdate();

					// report back when done
					if (successCallback != null)
						successCallback();

#warning Need to again handle special commands in here.

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