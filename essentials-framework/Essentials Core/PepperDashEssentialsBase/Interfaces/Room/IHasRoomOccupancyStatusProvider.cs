using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Interfaces.Room
{
    public interface IHasRoomOccupancyStatusProvider
    {
        event EventHandler<EventArgs> RoomOccupancyIsSet;

        IOccupancyStatusProvider RoomOccupancy { get; }

        bool OccupancyStatusProviderIsRemote { get; }

        int ShutdownVacancySeconds { get; }

        eVacancyMode VacancyMode { get; }

        void StartRoomVacancyTimer(eVacancyMode mode);

        /// <summary>
        /// Sets the object to be used as the IOccupancyStatusProvider for the room. Can be an Occupancy Aggregator or a specific device
        /// </summary>
        /// <param name="statusProvider"></param>
        /// <param name="timoutMinutes"></param>
        void SetRoomOccupancy(IOccupancyStatusProvider statusProvider, int timeoutMinutes);

        /// <summary>
        /// Executes when RoomVacancyShutdownTimer expires.  Used to trigger specific room actions as needed.  Must nullify the timer object when executed
        /// </summary>
        /// <param name="o"></param>
        void RoomVacatedForTimeoutPeriod(object o);

    }
}