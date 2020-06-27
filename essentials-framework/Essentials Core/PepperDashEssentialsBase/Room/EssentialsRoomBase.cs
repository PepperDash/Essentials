using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.Rooms.Config;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class EssentialsRoomBase : ReconfigurableDevice
    {
        protected EssentialsRoomPropertiesConfig BaseConfig;
        protected IBasicVolumeControls CurrentAudioDevice;
        protected Func<bool> IsCoolingFeedbackFunc;
        protected Func<bool> IsWarmingFeedbackFunc;
        protected string LastSourceKey;

        /// <summary>
        /// 
        /// </summary>
        protected Func<bool> OnFeedbackFunc;

        /// <summary>
        /// Seconds after vacancy detected until prompt is displayed
        /// </summary>
        protected int RoomVacancyShutdownPromptSeconds;

        /// <summary>
        /// Seconds after vacancy prompt is displayed until shutdown
        /// </summary>
        protected int RoomVacancyShutdownSeconds;

        protected CCriticalSection RoutingLock = new CCriticalSection();

        protected Dictionary<IBasicVolumeWithFeedback, uint> SavedVolumeLevels =
            new Dictionary<IBasicVolumeWithFeedback, uint>();

        private SourceListItem _currentSourceInfo;

        /// <summary>
        /// If room is off, enables power on to last source. Default true
        /// </summary>
        public bool EnablePowerOnToLastSource { get; set; }

        protected EssentialsRoomBase(DeviceConfig config)
            : base(config)
        {
            BaseConfig = config.Properties.ToObject<EssentialsRoomPropertiesConfig>();

            ZeroVolumeWhenSwtichingVolumeDevices = BaseConfig.ZeroVolumeWhenSwtichingVolumeDevices;
            SetupShutdownPrompt();

            SetupRoomVacancyShutdown();

            OnFeedback = new BoolFeedback(OnFeedbackFunc);

            IsWarmingUpFeedback = new BoolFeedback(IsWarmingFeedbackFunc);
            IsCoolingDownFeedback = new BoolFeedback(IsCoolingFeedbackFunc);

            AddPostActivationAction(() =>
            {
                if (RoomOccupancy != null)
                {
                    OnRoomOccupancyIsSet();
                }
            });
        }

        public IRoutingSinkWithSwitching DefaultDisplay { get; protected set; }
        public IRoutingSink DefaultAudioDevice { get; protected set; }
        public IBasicVolumeControls DefaultVolumeControls { get; protected set; }

        public string DefaultSourceItem { get; set; }

        public ushort DefaultVolume { get; set; }

        /// <summary>
        /// Sets the volume control device, and attaches/removes InUseTrackers with "audio"
        /// tag to device.
        /// </summary>
        public IBasicVolumeControls CurrentVolumeControls
        {
            get { return CurrentAudioDevice; }
            set
            {
                if (value == CurrentAudioDevice)
                {
                    return;
                }

                var handler = CurrentVolumeDeviceChange;

                if (handler != null)
                {
                    handler(this,
                        new VolumeDeviceChangeEventArgs(CurrentAudioDevice, value, ChangeType.WillChange));
                }

                var oldDevice = CurrentAudioDevice as IInUseTracking;
                var newDevice = value as IInUseTracking;

                UpdateInUseTracking(oldDevice, newDevice);

                CurrentAudioDevice = value;

                if (handler == null)
                {
                    return;
                }

                handler(this,
                    new VolumeDeviceChangeEventArgs(CurrentAudioDevice, value, ChangeType.DidChange));
            }
        }

        public bool ExcludeFromGlobalFunctions { get; set; }

        public BoolFeedback OnFeedback { get; private set; }

        public BoolFeedback IsWarmingUpFeedback { get; private set; }
        public BoolFeedback IsCoolingDownFeedback { get; private set; }

        public IOccupancyStatusProvider RoomOccupancy { get; private set; }

        public bool OccupancyStatusProviderIsRemote { get; private set; }

        /// <summary>
        /// The config name of the source list
        /// </summary>
        public string SourceListKey { get; set; }

        /// <summary>
        /// Timer used for informing the UIs of a shutdown
        /// </summary>        
        public SecondsCountdownTimer ShutdownPromptTimer { get; private set; }

        public int ShutdownPromptSeconds { get; set; }
        public int ShutdownVacancySeconds { get; set; }
        public eShutdownType ShutdownType { get; private set; }

        public EssentialsRoomEmergencyBase Emergency { get; set; }

        public Privacy.MicrophonePrivacyController MicrophonePrivacy { get; set; }

        public string LogoUrl { get; set; }

        protected SecondsCountdownTimer RoomVacancyShutdownTimer { get; private set; }

        public eVacancyMode VacancyMode { get; private set; }

        /// <summary>
        /// When volume control devices change, should we zero the one that we are leaving?
        /// </summary>
        public bool ZeroVolumeWhenSwtichingVolumeDevices { get; private set; }

        #region IHasCurrentSourceInfoChange Members

        public event SourceInfoChangeHandler CurrentSourceChange;
        public string CurrentSourceInfoKey { get; set; }

        /// <summary>
        /// The SourceListItem last run - containing names and icons 
        /// </summary>
        public SourceListItem CurrentSourceInfo
        {
            get { return _currentSourceInfo; }
            set
            {
                if (value == _currentSourceInfo)
                {
                    return;
                }

                var handler = CurrentSourceChange;

                if (handler != null)
                {
                    handler(_currentSourceInfo, ChangeType.WillChange);
                }

                var oldSource = _currentSourceInfo as IInUseTracking;
                var newSource = value as IInUseTracking;

                UpdateInUseTracking(oldSource, newSource);

                _currentSourceInfo = value;

                if (handler == null)
                {
                    return;
                }

                handler(_currentSourceInfo, ChangeType.DidChange);
            }
        }

        #endregion

        public event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;

        /// <summary>
        /// Fires when the RoomOccupancy object is set
        /// </summary>
        public event EventHandler<EventArgs> RoomOccupancyIsSet;


        private void SetupRoomVacancyShutdown()
        {
            RoomVacancyShutdownTimer = new SecondsCountdownTimer(Key + "-vacancyOffTimer");

            RoomVacancyShutdownTimer.HasFinished += RoomVacancyShutdownPromptTimer_HasFinished; // Shutdown is triggered

            RoomVacancyShutdownPromptSeconds = 1500; //  25 min to prompt warning
            RoomVacancyShutdownSeconds = 240; //  4 min after prompt will trigger shutdown prompt
            VacancyMode = eVacancyMode.None;
        }

        private void SetupShutdownPrompt()
        {
            // Setup the ShutdownPromptTimer
            ShutdownPromptTimer = new SecondsCountdownTimer(Key + "-offTimer");
            ShutdownPromptTimer.IsRunningFeedback.OutputChange += (o, a) =>
            {
                if (!ShutdownPromptTimer.IsRunningFeedback.BoolValue)
                {
                    ShutdownType = eShutdownType.None;
                }
            };
            ShutdownPromptTimer.HasFinished += (o, a) => Shutdown(); // Shutdown is triggered 

            ShutdownPromptSeconds = 60;
            ShutdownVacancySeconds = 120;

            ShutdownType = eShutdownType.None;
        }

        protected void InitializeDisplay(DisplayBase display)
        {
            // Link power, warming, cooling to display
            display.PowerIsOnFeedback.OutputChange += PowerIsOnFeedbackOnOutputChange;

            display.IsWarmingUpFeedback.OutputChange += IsWarmingUpFeedbackOnOutputChange;
            display.IsCoolingDownFeedback.OutputChange += IsCoolingDownFeedbackOnOutputChange;
        }

        protected void UpdateInUseTracking(IInUseTracking oldDev, IInUseTracking newDev)
        {
            // derigister this room from the device, if it can
            if (oldDev != null)
            {
                oldDev.InUseTracker.RemoveUser(this, "audio");
            }

            // register this room with new device, if it can
            if (newDev != null)
            {
                newDev.InUseTracker.AddUser(this, "audio");
            }
        }

        protected abstract void PowerIsOnFeedbackOnOutputChange(object sender, FeedbackEventArgs args);
        protected abstract void IsWarmingUpFeedbackOnOutputChange(object sender, FeedbackEventArgs args);
        protected abstract void IsCoolingDownFeedbackOnOutputChange(object sender, FeedbackEventArgs args);

        private void RoomVacancyShutdownPromptTimer_HasFinished(object sender, EventArgs e)
        {
            switch (VacancyMode)
            {
                case eVacancyMode.None:
                    StartRoomVacancyTimer(eVacancyMode.InInitialVacancy);
                    break;
                case eVacancyMode.InInitialVacancy:
                    StartRoomVacancyTimer(eVacancyMode.InShutdownWarning);
                    break;
                case eVacancyMode.InShutdownWarning:
                {
                    StartShutdown(eShutdownType.Vacancy);
                    Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Shutting Down due to vacancy.");
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public void StartShutdown(eShutdownType type)
        {
            // Check for shutdowns running. Manual should override other shutdowns

            switch (type)
            {
                case eShutdownType.Manual:
                    ShutdownPromptTimer.SecondsToCount = ShutdownPromptSeconds;
                    break;
                case eShutdownType.Vacancy:
                    ShutdownPromptTimer.SecondsToCount = ShutdownVacancySeconds;
                    break;
            }
            ShutdownType = type;
            ShutdownPromptTimer.Start();

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "ShutdownPromptTimer Started. Type: {0}.  Seconds: {1}",
                ShutdownType, ShutdownPromptTimer.SecondsToCount);
        }

        public void StartRoomVacancyTimer(eVacancyMode mode)
        {
            switch (mode)
            {
                case eVacancyMode.None:
                    RoomVacancyShutdownTimer.SecondsToCount = RoomVacancyShutdownPromptSeconds;
                    break;
                case eVacancyMode.InInitialVacancy:
                    RoomVacancyShutdownTimer.SecondsToCount = RoomVacancyShutdownSeconds;
                    break;
                case eVacancyMode.InShutdownWarning:
                    RoomVacancyShutdownTimer.SecondsToCount = 60;
                    break;
            }
            VacancyMode = mode;
            RoomVacancyShutdownTimer.Start();

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Vacancy Timer Started. Mode: {0}.  Seconds: {1}",
                VacancyMode, RoomVacancyShutdownTimer.SecondsToCount);
        }

        /// <summary>
        /// Resets the vacancy mode and shutsdwon the room
        /// </summary>
        public void Shutdown()
        {
            VacancyMode = eVacancyMode.None;
            EndShutdown();
        }

        /// <summary>
        /// This method is for the derived class to define it's specific shutdown
        /// requirements but should not be called directly.  It is called by Shutdown()
        /// </summary>
        protected virtual void EndShutdown()
        {
            SetDefaultLevels();

            RunDefaultPresentRoute();

            //CrestronEnvironment.Sleep(1000); //why?

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Shutting down room");

            RunRouteAction("roomOff");
        }


        /// <summary>
        /// Override this to implement a default volume level(s) method
        /// </summary>
        public virtual void SetDefaultLevels()
        {
            Debug.Console(1, this, "Restoring default levels");
            var vc = CurrentVolumeControls as IBasicVolumeWithFeedback;
            if (vc != null)
            {
                vc.SetVolume(DefaultVolume);
            }
        }

        /// <summary>
        /// Sets the object to be used as the IOccupancyStatusProvider for the room. Can be an Occupancy Aggregator or a specific device
        /// </summary>
        /// <param name="statusProvider"></param>
        /// <param name="timeoutMinutes"></param>
        public void SetRoomOccupancy(IOccupancyStatusProvider statusProvider, int timeoutMinutes)
        {
            var provider = statusProvider as IKeyed;

            if (provider == null)
            {
                Debug.Console(0, this, "ERROR: Occupancy sensor device is null");
                return;
            }

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Room Occupancy set to device: '{0}'", provider.Key);
            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Timeout Minutes from Config is: {0}", timeoutMinutes);

            // If status provider is fusion, set flag to remote
            if (statusProvider is Fusion.EssentialsHuddleSpaceFusionSystemControllerBase)
            {
                OccupancyStatusProviderIsRemote = true;
            }

            if (timeoutMinutes > 0)
            {
                RoomVacancyShutdownSeconds = timeoutMinutes*60;
            }

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "RoomVacancyShutdownSeconds set to {0}",
                RoomVacancyShutdownSeconds);

            RoomOccupancy = statusProvider;

            RoomOccupancy.RoomIsOccupiedFeedback.OutputChange -= RoomIsOccupiedFeedback_OutputChange;
            RoomOccupancy.RoomIsOccupiedFeedback.OutputChange += RoomIsOccupiedFeedback_OutputChange;

            OnRoomOccupancyIsSet();
        }

        private void OnRoomOccupancyIsSet()
        {
            var handler = RoomOccupancyIsSet;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        /// <summary>
        /// To allow base class to power room on to last source
        /// </summary>
        public virtual void PowerOnToDefaultOrLastSource()
        {
            if (!EnablePowerOnToLastSource || LastSourceKey == null)
            {
                return;
            }
            RunRouteAction(LastSourceKey);   
        }

        /// <summary>
        /// To allow base class to power room on to default source
        /// </summary>
        /// <returns></returns>
        public virtual bool RunDefaultPresentRoute()
        {
            if (DefaultSourceItem == null)
            {
                Debug.Console(0, this, "Unable to run default present route, DefaultSourceItem is null.");
                return false;
            }

            RunRouteAction(DefaultSourceItem);
            return true;
        }

        private void RoomIsOccupiedFeedback_OutputChange(object sender, EventArgs e)
        {
            if (RoomOccupancy.RoomIsOccupiedFeedback.BoolValue == false)
            {
                Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Notice: Vacancy Detected");
                // Trigger the timer when the room is vacant
                StartRoomVacancyTimer(eVacancyMode.InInitialVacancy);
            }
            else
            {
                Debug.Console(1, this, Debug.ErrorLogLevel.Notice, "Notice: Occupancy Detected");
                // Reset the timer when the room is occupied
                RoomVacancyShutdownTimer.Cancel();
            }
        }

        /// <summary>
        /// Executes when RoomVacancyShutdownTimer expires.  Used to trigger specific room actions as needed.  Must nullify the timer object when executed
        /// </summary>
        /// <param name="o"></param>
        public abstract void RoomVacatedForTimeoutPeriod(object o);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeKey"></param>
        public virtual void RunRouteAction(string routeKey)
        {
            RunRouteAction(routeKey, String.Empty, () => { });
        }

        /// <summary>
        /// Gets a source from config list SourceListKey and dynamically build and executes the
        /// route or commands
        /// </summary>
        public virtual void RunRouteAction(string routeKey, Action successCallback)
        {
            RunRouteAction(routeKey, String.Empty, successCallback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeKey"></param>
        /// <param name="sourceListKey"></param>
        public virtual void RunRouteAction(string routeKey, string sourceListKey)
        {
            RunRouteAction(routeKey, sourceListKey, () => { });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeKey"></param>
        /// <param name="sourceListKey"></param>
        /// <param name="successCallback"></param>
        public virtual void RunRouteAction(string routeKey, string sourceListKey, Action successCallback)
        {
            var routeObject =
                new {RouteKey = routeKey, SourceListKey = sourceListKey, SuccessCallback = successCallback};
            CrestronInvoke.BeginInvoke(RunRouteAction, routeObject); // end of BeginInvoke
        }

        protected virtual void RunRouteAction(object routeObject)
        {
            try
            {
                RoutingLock.Enter();

                var routeObj = new {RouteKey = "", SourceListKey = "", SuccessCallback = new Action(() => { })};

                routeObj = Cast(routeObj, routeObject);

                Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Run route action '{0}'", routeObj.RouteKey);
                var sourceList = GetSourceListForKey(routeObj.RouteKey, routeObj.SourceListKey);

                if (sourceList == null)
                {
                    Debug.Console(0, this, "No source list found for key {0}", routeObj.SourceListKey);
                    return;
                }

                var item = sourceList[routeObj.RouteKey];

                // End usage timer on last source
                StopUsageTrackingOnCurrentSource(sourceList);

                // Let's run it
                if (routeObj.RouteKey.ToLower() != "roomoff")
                {
                    LastSourceKey = routeObj.RouteKey;
                }
                else
                {
                    CurrentSourceInfoKey = null;
                }

                foreach (var route in item.RouteList)
                {
                    var tempVideo = new SourceRouteListItem
                    {
                        DestinationKey = "$defaultDisplay",
                        SourceKey = route.SourceKey,
                        Type = eRoutingSignalType.Video
                    };

                    var routeItem = route.DestinationKey.Equals("$defaultAll", StringComparison.OrdinalIgnoreCase)
                        ? tempVideo
                        : route;

                    DoRoute(routeItem);
                }

                // Start usage timer on routed source
                if (item.SourceDevice is IUsageTracking)
                {
                    (item.SourceDevice as IUsageTracking).UsageTracker.StartDeviceUsage();
                }

                // Set volume control, using default if non provided
                SetVolumeControl(item);

                // store the name and UI info for routes
                if (item.SourceKey == "$off")
                {
                    CurrentSourceInfoKey = routeObj.RouteKey;
                    CurrentSourceInfo = null;
                }
                else if (item.SourceKey != null)
                {
                    CurrentSourceInfoKey = routeObj.RouteKey;
                    CurrentSourceInfo = item;
                }

                OnFeedback.FireUpdate();

                // report back when done
                if (routeObj.SuccessCallback != null)
                {
                    routeObj.SuccessCallback();
                }
            }
            finally
            {
                RoutingLock.Leave();
            }
        }

        private static T Cast<T>(T typeHolder, object m)
        {
            return (T) m;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        protected void DoRoute(SourceRouteListItem route)
        {
            var dest = GetDestination(route);

            if (route.SourceKey.Equals("$off", StringComparison.OrdinalIgnoreCase))
            {
                dest.ReleaseRoute();
                if (dest is IPower)
                {
                    (dest as IPower).PowerOff();
                }
            }
            else
            {
                var source = DeviceManager.GetDeviceForKey(route.SourceKey) as IRoutingOutputs;
                if (source == null)
                {
                    Debug.Console(1, this, "Cannot route unknown source '{0}' to {1}", route.SourceKey,
                        route.DestinationKey);
                    return;
                }
                dest.ReleaseAndMakeRoute(source, route.Type);
            }
        }

        private IRoutingSink GetDestination(SourceRouteListItem route)
        {
            IRoutingSink dest;
            if (route.DestinationKey.Equals("$defaultaudio", StringComparison.OrdinalIgnoreCase))
            {
                dest = DefaultAudioDevice;
            }
            else if (route.DestinationKey.Equals("$defaultDisplay", StringComparison.OrdinalIgnoreCase))
            {
                dest = DefaultDisplay;
            }
            else
            {
                dest = DeviceManager.GetDeviceForKey(route.DestinationKey) as IRoutingSink;
            }

            if (dest != null)
            {
                return dest;
            }

            Debug.Console(1, this, "Cannot route, unknown destination '{0}'", route.DestinationKey);
            return dest;
        }

        private void SetVolumeControl(SourceListItem item)
        {
            IBasicVolumeControls volDev = null;
            // Handle special cases for volume control
            if (string.IsNullOrEmpty(item.VolumeControlKey)
                || item.VolumeControlKey.Equals("$defaultAudio", StringComparison.OrdinalIgnoreCase))
            {
                volDev = DefaultVolumeControls;
            }
            else if (item.VolumeControlKey.Equals("$defaultDisplay", StringComparison.OrdinalIgnoreCase))
            {
                volDev = DefaultDisplay as IBasicVolumeControls;
            }
            else
            {
                var dev = DeviceManager.GetDeviceForKey(item.VolumeControlKey);
                if (dev is IBasicVolumeControls)
                {
                    volDev = dev as IBasicVolumeControls;
                }
                else if (dev is IHasVolumeDevice)
                {
                    volDev = (dev as IHasVolumeDevice).VolumeDevice;
                }
            }

            if (volDev == CurrentVolumeControls)
            {
                return;
            }

            IBasicVolumeWithFeedback vd;
            // zero the volume on the device we are leaving.  
            // Set the volume to default on device we are entering
            if (ZeroVolumeWhenSwtichingVolumeDevices && CurrentVolumeControls is IBasicVolumeWithFeedback)
            {
                vd = CurrentVolumeControls as IBasicVolumeWithFeedback;
                SavedVolumeLevels[vd] = (uint) vd.VolumeLevelFeedback.IntValue;
                vd.SetVolume(0);
            }
            CurrentVolumeControls = volDev;
            if (!ZeroVolumeWhenSwtichingVolumeDevices || !(CurrentVolumeControls is IBasicVolumeWithFeedback))
            {
                return;
            }

            vd = CurrentVolumeControls as IBasicVolumeWithFeedback;
            var vol = (SavedVolumeLevels.ContainsKey(vd) ? (ushort) SavedVolumeLevels[vd] : DefaultVolume);
            vd.SetVolume(vol);
        }

        private void StopUsageTrackingOnCurrentSource(Dictionary<string, SourceListItem> sourceList)
        {
            if (string.IsNullOrEmpty(LastSourceKey))
            {
                return;
            }

            var lastSource = sourceList[LastSourceKey].SourceDevice;

            try
            {
                if (lastSource is IUsageTracking)
                {
                    (lastSource as IUsageTracking).UsageTracker.EndDeviceUsage();
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "*#* EXCEPTION in end usage tracking (257):\r{0}", e);
            }
        }

        private Dictionary<string, SourceListItem> GetSourceListForKey(string routeKey, string sourceListKey)
        {
            var slKey = String.IsNullOrEmpty(sourceListKey) ? SourceListKey : sourceListKey;

            var sourceList = ConfigReader.ConfigObject.GetSourceListForKey(slKey);

            if (sourceList == null)
            {
                Debug.Console(1, this, "WARNING: Config source list '{0}' not found", slKey);
                return null;
            }

            // Try to get the list item by it's string key
            if (sourceList.ContainsKey(routeKey))
            {
                return sourceList;
            }

            Debug.Console(1, this, "WARNING: No source list '{0}' found in config source lists '{1}'",
                routeKey, SourceListKey);
            return null;
        }

        /// <summary>
        /// Runs "roomOff" action on all rooms not set to ExcludeFromGlobalFunctions
        /// </summary>
        public static void AllRoomsOff()
        {
            var allRooms = DeviceManager.AllDevices.OfType<EssentialsRoomBase>().Where(d =>
                !d.ExcludeFromGlobalFunctions);
            foreach (var room in allRooms)
            {
                room.RunRouteAction("roomOff");
            }
        }
    }

    /// <summary>
    /// To describe the various ways a room may be shutting down
    /// </summary>
    public enum eShutdownType
    {
        None = 0,
        External,
        Manual,
        Vacancy
    }

    public enum eVacancyMode
    {
        None = 0,
        InInitialVacancy,
        InShutdownWarning
    }

    /// <summary>
    /// 
    /// </summary>
    public enum eWarmingCoolingMode
    {
        None,
        Warming,
        Cooling
    }

    public abstract class EssentialsRoomEmergencyBase : IKeyed
    {
        protected EssentialsRoomEmergencyBase(string key)
        {
            Key = key;
        }

        #region IKeyed Members

        public string Key { get; private set; }

        #endregion
    }
}