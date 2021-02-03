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

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class EssentialsRoomBase : ReconfigurableDevice
    {
        /// <summary>
        ///
        /// </summary>
        public BoolFeedback OnFeedback { get; private set; }

        /// <summary>
        /// Fires when the RoomOccupancy object is set
        /// </summary>
        public event EventHandler<EventArgs> RoomOccupancyIsSet;

        public BoolFeedback IsWarmingUpFeedback { get; private set; }
        public BoolFeedback IsCoolingDownFeedback { get; private set; }

        public IOccupancyStatusProvider RoomOccupancy { get; private set; }

        public bool OccupancyStatusProviderIsRemote { get; private set; }

        protected abstract Func<bool> IsWarmingFeedbackFunc { get; }
        protected abstract Func<bool> IsCoolingFeedbackFunc { get; }

        /// <summary>
        /// Indicates if this room is Mobile Control Enabled
        /// </summary>
        public bool IsMobileControlEnabled { get; private set; }

        /// <summary>
        /// The bridge for this room if Mobile Control is enabled
        /// </summary>
        public IMobileControlRoomBridge MobileControlRoomBridge { get; private set; }

        /// <summary>
        /// The config name of the source list
        /// </summary>
		/// 
		protected string _SourceListKey;
        public virtual string SourceListKey {
			get
			{
				return _SourceListKey;
			}
			set
			{
				_SourceListKey = value; 

			}
		}

        /// <summary>
        /// Timer used for informing the UIs of a shutdown
        /// </summary>        
        public SecondsCountdownTimer ShutdownPromptTimer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int ShutdownPromptSeconds { get; set; }
        public int ShutdownVacancySeconds { get; set; }
        public eShutdownType ShutdownType { get; private set; }

        public EssentialsRoomEmergencyBase Emergency { get; set; }

        public Core.Privacy.MicrophonePrivacyController MicrophonePrivacy { get; set; }

        public string LogoUrlLightBkgnd { get; set; }

        public string LogoUrlDarkBkgnd { get; set; }

        protected SecondsCountdownTimer RoomVacancyShutdownTimer { get; private set; }

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

		protected Dictionary<IBasicVolumeWithFeedback, uint> SavedVolumeLevels = new Dictionary<IBasicVolumeWithFeedback, uint>();

		/// <summary>
		/// When volume control devices change, should we zero the one that we are leaving?
		/// </summary>
		public bool ZeroVolumeWhenSwtichingVolumeDevices { get; private set; }


        public EssentialsRoomBase(DeviceConfig config)
            : base(config)
        {
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

        public override bool CustomActivate()
        {
            SetUpMobileControl();

            return base.CustomActivate();
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
                Debug.Console(1, this, "*********************Mobile Control Bridge Not found for this room.");
                IsMobileControlEnabled = false;
                return;
            }
            else
            {
                MobileControlRoomBridge = mcBridge as IMobileControlRoomBridge;
                Debug.Console(1, this, "*********************Mobile Control Bridge found and enabled for this room");
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
                        Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Shutting Down due to vacancy.");
                        break;
                    }
                default:
                    break;
            }
        }

        /// <summary>
        /// Starts the Shutdown process based on the type
        /// </summary>
        /// <param name="type"></param>
        public void StartShutdown(eShutdownType type)
        {
            // Check for shutdowns running. Manual should override other shutdowns

            if (type == eShutdownType.Manual)
                ShutdownPromptTimer.SecondsToCount = ShutdownPromptSeconds;
            else if (type == eShutdownType.Vacancy)
                ShutdownPromptTimer.SecondsToCount = ShutdownVacancySeconds;
            ShutdownType = type;
            ShutdownPromptTimer.Start();

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "ShutdownPromptTimer Started. Type: {0}.  Seconds: {1}", ShutdownType, ShutdownPromptTimer.SecondsToCount);
        }

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

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Vacancy Timer Started. Mode: {0}.  Seconds: {1}", VacancyMode, RoomVacancyShutdownTimer.SecondsToCount);
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
        protected abstract void EndShutdown();


        /// <summary>
        /// Override this to implement a default volume level(s) method
        /// </summary>
        public abstract void SetDefaultLevels();

        /// <summary>
        /// Sets the object to be used as the IOccupancyStatusProvider for the room. Can be an Occupancy Aggregator or a specific device
        /// </summary>
        /// <param name="statusProvider"></param>
        public void SetRoomOccupancy(IOccupancyStatusProvider statusProvider, int timeoutMinutes)
        { 
			if (statusProvider == null)
			{
				Debug.Console(0, this, "ERROR: Occupancy sensor device is null");
				return;
			}

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Room Occupancy set to device: '{0}'", (statusProvider as Device).Key);
            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Timeout Minutes from Config is: {0}", timeoutMinutes);

            // If status provider is fusion, set flag to remote
            if (statusProvider is Core.Fusion.EssentialsHuddleSpaceFusionSystemControllerBase)
                OccupancyStatusProviderIsRemote = true;

            if(timeoutMinutes > 0)
                RoomVacancyShutdownSeconds = timeoutMinutes * 60;

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "RoomVacancyShutdownSeconds set to {0}", RoomVacancyShutdownSeconds);

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
        public string Key { get; private set; }

        public EssentialsRoomEmergencyBase(string key)
        {
            Key = key;
        }
    }
}