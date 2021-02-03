using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.Core.Interfaces.Room
{
    /// <summary>
    /// Describes the necessary functions for a room that can be powered on and shutdown
    /// </summary>
    public interface IHasRoomOn
    {
        BoolFeedback OnFeedback { get; }

        BoolFeedback IsWarmingUpFeedback { get; }
        BoolFeedback IsCoolingDownFeedback { get; }

        /// <summary>
        /// Timer used for informing the UIs of a shutdown
        /// </summary>        
        SecondsCountdownTimer ShutdownPromptTimer { get; }

        /// <summary>
        /// 
        /// </summary>
        int ShutdownPromptSeconds { get; }
        int ShutdownVacancySeconds { get; }
        eShutdownType ShutdownType { get; }


        /// <summary>
        /// Starts the Shutdown process based on the type
        /// </summary>
        /// <param name="type"></param>
        void StartShutdown(eShutdownType type);


        /// <summary>
        /// Resets the vacancy mode and shuts down the room
        /// </summary>
        void Shutdown();

    }
}