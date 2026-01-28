using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Scheduler;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class EssentialsRoomBase : ReconfigurableDevice, IEssentialsRoom
    {
        /// <summary>
        ///
        /// </summary>
        public BoolFeedback OnFeedback { get; private set; }

        /// <summary>
        /// Fires when the RoomOccupancy object is set
        /// </summary>
        public event EventHandler<EventArgs> RoomOccupancyIsSet;

        /// <summary>
        /// Gets or sets the IsWarmingUpFeedback
        /// </summary>
        public BoolFeedback IsWarmingUpFeedback { get; private set; }

        /// <summary>
        /// Gets or sets the IsCoolingDownFeedback
        /// </summary>
        public BoolFeedback IsCoolingDownFeedback { get; private set; }

        /// <summary>
        /// Gets or sets the RoomOccupancy
        /// </summary>
        public IOccupancyStatusProvider RoomOccupancy { get; protected set; }

        /// <summary>
        /// Gets or sets the OccupancyStatusProviderIsRemote
        /// </summary>
        public bool OccupancyStatusProviderIsRemote { get; private set; }

        /// <summary>
        /// Gets or sets the EnvironmentalControlDevices
        /// </summary>
        public List<EssentialsDevice> EnvironmentalControlDevices { get; protected set; }

        /// <summary>
        /// Indicates if the room has any environmental control devices
        /// </summary>
        public bool HasEnvironmentalControlDevices
        {
            get
            {
                return EnvironmentalControlDevices != null && EnvironmentalControlDevices.Count > 0;
            }
        }

        /// <summary>
        /// Gets or sets the IsWarmingFeedbackFunc
        /// </summary>
        protected abstract Func<bool> IsWarmingFeedbackFunc { get; }

        /// <summary>
        /// Gets or sets the IsCoolingFeedbackFunc
        /// </summary>
        protected abstract Func<bool> IsCoolingFeedbackFunc { get; }

        /// <summary>
        /// Gets or sets the IsMobileControlEnabled
        /// </summary>
        public bool IsMobileControlEnabled { get; private set; }

        /// <summary>
        /// Gets or sets the MobileControlRoomBridge
        /// </summary>
        public IMobileControlRoomMessenger MobileControlRoomBridge { get; private set; }

        /// <summary>
        /// The config name of the default source list
        /// </summary>
        protected const string _defaultListKey = "default";

        /// <summary>
        /// The config name of the source list
        /// </summary>
		/// 
		private string _sourceListKey;

        /// <summary>
        /// Gets or sets the SourceListKey
        /// </summary>
        public string SourceListKey {
			get
			{
                if(string.IsNullOrEmpty(_sourceListKey))
                {
                    return _defaultListKey;
                }
                else
                {
                    return _sourceListKey;
                }
			}
			protected set
			{
                if (value != _sourceListKey)
                {
                    _sourceListKey = value;
                }
			}
		}

        private string _destinationListKey;

        /// <summary>
        /// Gets or sets the DestinationListKey
        /// </summary>
        public string DestinationListKey
        {
            get 
            {
                if (string.IsNullOrEmpty(_destinationListKey))
                {
                    return _defaultListKey;
                }
                else
                {
                    return _destinationListKey;
                }
            }
            protected set
            {
                if (value != _destinationListKey)
                {
                    _destinationListKey = value;
                }
            }
        }

        private string _audioControlPointListKey;

        /// <summary>
        /// Gets or sets the AudioControlPointListKey
        /// </summary>
        public string AudioControlPointListKey
        {
            get
            {
                if (string.IsNullOrEmpty(_audioControlPointListKey))
                {
                    return _defaultListKey;
                }
                else
                {
                    return _destinationListKey;
                }
            }
            protected set
            {
                if (value != _audioControlPointListKey)
                {
                    _audioControlPointListKey = value;
                }
            }
        }

        private string _cameraListKey;

        /// <summary>
        /// Gets or sets the CameraListKey
        /// </summary>
        public string CameraListKey
        {
            get
            {
                if (string.IsNullOrEmpty(_cameraListKey))
                {
                    return _defaultListKey;
                }
                else
                {
                    return _cameraListKey;
                }
            }
            protected set
            {
                if (value != _cameraListKey)
                {
                    _cameraListKey = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the ShutdownPromptTimer
        /// </summary>
        public SecondsCountdownTimer ShutdownPromptTimer { get; private set; }

        /// <summary>
        /// Gets or sets the ShutdownPromptSeconds
        /// </summary>
        public int ShutdownPromptSeconds { get; set; }

        /// <summary>
        /// Gets or sets the ShutdownVacancySeconds
        /// </summary>
        public int ShutdownVacancySeconds { get; set; }

        /// <summary>
        /// Gets or sets the ShutdownType
        /// </summary>
        public eShutdownType ShutdownType { get; private set; }

        /// <summary>
        /// Gets or sets the Emergency
        /// </summary>
        public EssentialsRoomEmergencyBase Emergency { get; set; }

        /// <summary>
        /// Gets or sets the MicrophonePrivacy
        /// </summary>
        public Core.Privacy.MicrophonePrivacyController MicrophonePrivacy { get; set; }

        /// <summary>
        /// Gets or sets the LogoUrlLightBkgnd
        /// </summary>
        public string LogoUrlLightBkgnd { get; set; }

        /// <summary>
        /// Gets or sets the LogoUrlDarkBkgnd
        /// </summary>
        public string LogoUrlDarkBkgnd { get; set; }

        /// <summary>
        /// Gets or sets the RoomVacancyShutdownTimer
        /// </summary>
        protected SecondsCountdownTimer RoomVacancyShutdownTimer { get; private set; }

        /// <summary>
        /// Gets or sets the VacancyMode
        /// </summary>
        public eVacancyMode VacancyMode { get; private set; }

        /// <summary>
        /// Seconds after vacancy prompt is displayed until shutdown
        /// </summary>
        protected int RoomVacancyShutdownSeconds;

        /// <summary>
        /// Seconds after vacancy detected until prompt is displayed
        /// </summary>
        protected int RoomVacancyShutdownPromptSeconds;

        /// <summary>
        /// 
        /// </summary>
        protected abstract Func<bool> OnFeedbackFunc { get; }

        /// <summary>
        /// Gets or sets the SavedVolumeLevels
        /// </summary>
		protected Dictionary<IBasicVolumeWithFeedback, uint> SavedVolumeLevels = new Dictionary<IBasicVolumeWithFeedback, uint>();

		/// <summary>
		/// When volume control devices change, should we zero the one that we are leaving?
		/// </summary>
		public bool ZeroVolumeWhenSwtichingVolumeDevices { get; private set; }

        /// <summary>
        /// Constructor for EssentialsRoomBase
        /// </summary>
        /// <param name="config">config of the device</param>
        public EssentialsRoomBase(DeviceConfig config)
            : base(config)
        {
            EnvironmentalControlDevices = new List<EssentialsDevice>();

            // Setup the ShutdownPromptTimer
            ShutdownPromptTimer = new SecondsCountdownTimer(Key + "-offTimer");
            ShutdownPromptTimer.IsRunningFeedback.OutputChange += (o, a) =>
            {
                if (!ShutdownPromptTimer.IsRunningFeedback.BoolValue)
                    ShutdownType = eShutdownType.None;
            };

            ShutdownPromptTimer.HasFinished += (o, a) => Shutdown(); // Shutdown is triggered 

            ShutdownPromptSeconds = 60;
            ShutdownVacancySeconds = 120; 
            
            ShutdownType = eShutdownType.None;

            RoomVacancyShutdownTimer = new SecondsCountdownTimer(Key + "-vacancyOffTimer");
            //RoomVacancyShutdownTimer.IsRunningFeedback.OutputChange += (o, a) =>
            //{
            //    if (!RoomVacancyShutdownTimer.IsRunningFeedback.BoolValue)
            //        ShutdownType = ShutdownType.Vacancy;
            //};
            RoomVacancyShutdownTimer.HasFinished += new EventHandler<EventArgs>(RoomVacancyShutdownPromptTimer_HasFinished); // Shutdown is triggered

            RoomVacancyShutdownPromptSeconds = 1500;    //  25 min to prompt warning
            RoomVacancyShutdownSeconds = 240;           //  4 min after prompt will trigger shutdown prompt
            VacancyMode = eVacancyMode.None;

            OnFeedback = new BoolFeedback(OnFeedbackFunc);

            IsWarmingUpFeedback = new BoolFeedback(IsWarmingFeedbackFunc);
            IsCoolingDownFeedback = new BoolFeedback(IsCoolingFeedbackFunc);

            AddPostActivationAction(() =>
            {
                if (RoomOccupancy != null)
                    OnRoomOccupancyIsSet();
            });
        }

        /// <summary>
        /// CustomActivate method
        /// </summary>
        /// <inheritdoc />
        public override bool CustomActivate()
        {
            SetUpMobileControl();

            return base.CustomActivate();
        }

        /// <summary>
        /// Sets the SourceListKey property to the passed in value or the default if no value passed in
        /// </summary>
        /// <param name="sourceListKey"></param>
        protected void SetSourceListKey(string sourceListKey)
        {
            if (!string.IsNullOrEmpty(sourceListKey))
            {
                SourceListKey = sourceListKey;
            }
            else
            {
                sourceListKey = _defaultListKey;
            }
        }

        /// <summary>
        /// Sets the DestinationListKey property to the passed in value or the default if no value passed in
        /// </summary>
        /// <param name="destinationListKey">key of the destination list object</param>
        protected void SetDestinationListKey(string destinationListKey)
        {
            if (!string.IsNullOrEmpty(destinationListKey))
            {
                DestinationListKey = destinationListKey;
            }
        }

        /// <summary>
        /// If mobile control is enabled, sets the appropriate properties
        /// </summary>
        void SetUpMobileControl()
        {
            var mcBridgeKey = string.Format("mobileControlBridge-{0}", Key);
            var mcBridge = DeviceManager.GetDeviceForKey(mcBridgeKey);
            if (mcBridge == null)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "*********************Mobile Control Bridge Not found for this room.");
                IsMobileControlEnabled = false;
                return;
            }
            else
            {
                MobileControlRoomBridge = mcBridge as IMobileControlRoomMessenger;
                Debug.LogMessage(LogEventLevel.Debug, this, "*********************Mobile Control Bridge found and enabled for this room");
                IsMobileControlEnabled = true;
            }
        }

        void RoomVacancyShutdownPromptTimer_HasFinished(object sender, EventArgs e)
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
                        Debug.LogMessage(LogEventLevel.Information, this, "Shutting Down due to vacancy.");
                        break;
                    }
                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <summary>
        /// StartShutdown method
        /// </summary>
        public void StartShutdown(eShutdownType type)
        {
            // Check for shutdowns running. Manual should override other shutdowns

            if (type == eShutdownType.Manual)
                ShutdownPromptTimer.SecondsToCount = ShutdownPromptSeconds;
            else if (type == eShutdownType.Vacancy)
                ShutdownPromptTimer.SecondsToCount = ShutdownVacancySeconds;
            ShutdownType = type;
            ShutdownPromptTimer.Start();

            Debug.LogMessage(LogEventLevel.Information, this, "ShutdownPromptTimer Started. Type: {0}.  Seconds: {1}", ShutdownType, ShutdownPromptTimer.SecondsToCount);
        }

        /// <summary>
        /// StartRoomVacancyTimer method
        /// </summary>
        public void StartRoomVacancyTimer(eVacancyMode mode)
        {
            if (mode == eVacancyMode.None)
                RoomVacancyShutdownTimer.SecondsToCount = RoomVacancyShutdownPromptSeconds;
            else if (mode == eVacancyMode.InInitialVacancy)
                RoomVacancyShutdownTimer.SecondsToCount = RoomVacancyShutdownSeconds;
            else if (mode == eVacancyMode.InShutdownWarning)
                RoomVacancyShutdownTimer.SecondsToCount = 60;
            VacancyMode = mode;
            RoomVacancyShutdownTimer.Start();

            Debug.LogMessage(LogEventLevel.Information, this, "Vacancy Timer Started. Mode: {0}.  Seconds: {1}", VacancyMode, RoomVacancyShutdownTimer.SecondsToCount);
        }

        /// <summary>
        /// Shutdown method
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
        protected abstract void EndShutdown();


        /// <summary>
        /// Override this to implement a default volume level(s) method
        /// </summary>
        public abstract void SetDefaultLevels();

        /// <summary>
        /// Sets the object to be used as the IOccupancyStatusProvider for the room. Can be an Occupancy Aggregator or a specific device
        /// </summary>
        /// <param name="statusProvider"></param>
        /// <param name="timeoutMinutes"></param>
        public void SetRoomOccupancy(IOccupancyStatusProvider statusProvider, int timeoutMinutes)
        { 
			if (statusProvider == null)
			{
				Debug.LogMessage(LogEventLevel.Information, this, "ERROR: Occupancy sensor device is null");
				return;
			}

            Debug.LogMessage(LogEventLevel.Information, this, "Room Occupancy set to device: '{0}'", (statusProvider as Device).Key);
            Debug.LogMessage(LogEventLevel.Information, this, "Timeout Minutes from Config is: {0}", timeoutMinutes);

            // If status provider is fusion, set flag to remote
            if (statusProvider is Core.Fusion.IEssentialsRoomFusionController)
                OccupancyStatusProviderIsRemote = true;

            if(timeoutMinutes > 0)
                RoomVacancyShutdownSeconds = timeoutMinutes * 60;

            Debug.LogMessage(LogEventLevel.Information, this, "RoomVacancyShutdownSeconds set to {0}", RoomVacancyShutdownSeconds);

            RoomOccupancy = statusProvider;

            RoomOccupancy.RoomIsOccupiedFeedback.OutputChange -= RoomIsOccupiedFeedback_OutputChange;
            RoomOccupancy.RoomIsOccupiedFeedback.OutputChange += RoomIsOccupiedFeedback_OutputChange;

            OnRoomOccupancyIsSet();
        }

        void OnRoomOccupancyIsSet()
        {
            var handler = RoomOccupancyIsSet;
            if (handler != null)
                handler(this, new EventArgs());
        }

        /// <summary>
        /// To allow base class to power room on to last source
        /// </summary>
        public abstract void PowerOnToDefaultOrLastSource();

        /// <summary>
        /// To allow base class to power room on to default source
        /// </summary>
        /// <returns></returns>
        public abstract bool RunDefaultPresentRoute();

        void RoomIsOccupiedFeedback_OutputChange(object sender, EventArgs e)
        {
            if (RoomOccupancy.RoomIsOccupiedFeedback.BoolValue == false  && AllowVacancyTimerToStart())
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Notice: Vacancy Detected");
                // Trigger the timer when the room is vacant
                StartRoomVacancyTimer(eVacancyMode.InInitialVacancy);
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Notice: Occupancy Detected");
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
        /// Allow the vacancy event from an occupancy sensor to turn the room off.
        /// </summary>
        /// <returns>If the timer should be allowed. Defaults to true</returns>
        protected virtual bool AllowVacancyTimerToStart()
        {
            return true;
        }
    }
        
    /// <summary>
    /// To describe the various ways a room may be shutting down
    /// </summary>
    public enum eShutdownType
    {
        /// <summary>
        /// No shutdown in progress
        /// </summary>
        None = 0,

        /// <summary>
        /// Manual shutdown initiated
        /// </summary>
        External,

        /// <summary>
        /// Vacancy based shutdown
        /// </summary>
        Manual,

        /// <summary>
        /// Shutdown due to room vacancy
        /// </summary>
        Vacancy
    }

    /// <summary>
    /// Enumeration of eVacancyMode values
    /// </summary>
    public enum eVacancyMode
    {
        /// <summary>
        /// No vacancy detected
        /// </summary>
        None = 0,

        /// <summary>
        /// InInitialVacancy - countdown to warning
        /// </summary>
        InInitialVacancy,

        /// <summary>
        /// InShutdownWarning - countdown to shutdown
        /// </summary>
        InShutdownWarning
    }

    /// <summary>
    /// Enumeration of eWarmingCoolingMode values
    /// </summary>
    public enum eWarmingCoolingMode
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Warming
        /// </summary>
        Warming,

        /// <summary>
        /// Cooling
        /// </summary>
        Cooling
    }

    /// <summary>
    /// Base class for room emergency implementations
    /// </summary>
    public abstract class EssentialsRoomEmergencyBase : IKeyed
    {
        public string Key { get; private set; }

        /// <summary>
        /// Constructor for EssentialsRoomEmergencyBase
        /// </summary>
        /// <param name="key">key of the room</param>
        public EssentialsRoomEmergencyBase(string key)
        {
            Key = key;
        }
    }
}