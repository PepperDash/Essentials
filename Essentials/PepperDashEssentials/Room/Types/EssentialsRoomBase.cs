using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Occupancy;

namespace PepperDash.Essentials
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHasCurrentSourceInfoChange
    {
        event SourceInfoChangeHandler CurrentSingleSourceChange;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class EssentialsRoomBase : Device
    {
        /// <summary>
        ///
        /// </summary>
        public BoolFeedback OnFeedback { get; private set; }

        public BoolFeedback IsWarmingUpFeedback { get; private set; }
        public BoolFeedback IsCoolingDownFeedback { get; private set; }

        public IOccupancyStatusProvider RoomOccupancy { get; private set; }

        protected abstract Func<bool> IsWarmingFeedbackFunc { get; }
        protected abstract Func<bool> IsCoolingFeedbackFunc { get; }

        /// <summary>
        /// Timer used for informing the UIs of a shutdown
        /// </summary>        
        public SecondsCountdownTimer ShutdownPromptTimer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int ShutdownPromptSeconds { get; set; }
        public int ShutdownVacancySeconds { get; set; }
        public ShutdownType ShutdownType { get; private set; }

        public PepperDash.Essentials.Room.EssentialsRoomEmergencyBase Emergency { get; set; }

        public string LogoUrl { get; set; }

        protected CTimer RoomVacancyShutdownTimer;

        protected long RoomVacancyShutdownTimeout = 1800000;  // 30 minutes by default

        /// <summary>
        /// 
        /// </summary>
        protected abstract Func<bool> OnFeedbackFunc { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        public EssentialsRoomBase(string key, string name) : base(key, name)
        {
            ShutdownPromptTimer = new SecondsCountdownTimer(Key + "-offTimer");
            ShutdownPromptTimer.IsRunningFeedback.OutputChange += (o, a) =>
            {
                if (!ShutdownPromptTimer.IsRunningFeedback.BoolValue)
                    ShutdownType = ShutdownType.None;
            };
            ShutdownPromptTimer.HasFinished += (o, a) => Shutdown(); // Shutdown is triggered 

            ShutdownPromptSeconds = 60;
            ShutdownVacancySeconds = 120;
            ShutdownType = ShutdownType.None;

            OnFeedback = new BoolFeedback(OnFeedbackFunc);

            IsWarmingUpFeedback = new BoolFeedback(IsWarmingFeedbackFunc);
            IsCoolingDownFeedback = new BoolFeedback(IsCoolingFeedbackFunc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public void StartShutdown(ShutdownType type)
        {
            // Check for shutdowns running. Manual should override other shutdowns

            if (type == ShutdownType.Manual)
                ShutdownPromptTimer.SecondsToCount = ShutdownPromptSeconds;
            else if (type == ShutdownType.Vacancy)
                ShutdownPromptTimer.SecondsToCount = ShutdownVacancySeconds;
            ShutdownType = type;
            ShutdownPromptTimer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract void Shutdown();

        /// <summary>
        /// Override this to implement a default volume level(s) method
        /// </summary>
        public abstract void SetDefaultLevels();

        /// <summary>
        /// Sets the object to be used as the IOccupancyStatusProvider for the room. Can be an Occupancy Aggregator or a specific device
        /// </summary>
        /// <param name="statusProvider"></param>
        public void SetRoomOccupancy(IOccupancyStatusProvider statusProvider)
        {
            RoomOccupancy = statusProvider;

            RoomOccupancy.RoomIsOccupiedFeedback.OutputChange += new EventHandler<EventArgs>(RoomIsOccupiedFeedback_OutputChange);
        }

        void RoomIsOccupiedFeedback_OutputChange(object sender, EventArgs e)
        {
            if ((sender as IOccupancyStatusProvider).RoomIsOccupiedFeedback.BoolValue == false)
            {
                // Trigger the timer when the room is vacant
                RoomVacancyShutdownTimer = new CTimer(RoomVacatedForTimeoutPeriod, RoomVacancyShutdownTimeout);
            }
            else
            {
                // Reset the timer when the room is occupied
                if(RoomVacancyShutdownTimer != null)
                    RoomVacancyShutdownTimer.Reset(RoomVacancyShutdownTimeout);
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
    public enum ShutdownType
    {
        None,
        External,
        Manual,
        Vacancy
    }

    /// <summary>
    /// 
    /// </summary>
    public enum WarmingCoolingMode
    {
        None,
        Warming,
        Cooling
    }
}