using System;

namespace PepperDash.Essentials.Core.Bridges
{
    public class CenOdtOccupancySensorBaseJoinMap : JoinMapBaseAdvanced
    {
        #region Digitals

        [JoinName("Online")]
        public JoinDataComplete Online = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("ForceOccupied")]
        public JoinDataComplete ForceOccupied = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Force Occupied", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("ForceVacant")]
        public JoinDataComplete ForceVacant = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "Force Vacant", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("EnableRawStates")]
        public JoinDataComplete EnableRawStates = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "Enable Raw States", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("RoomOccupiedFeedback")]
        public JoinDataComplete RoomOccupiedFeedback = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Room Occupied Feedback", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("GraceOccupancyDetectedFeedback")]
        public JoinDataComplete GraceOccupancyDetectedFeedback = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "Grace Occupancy Detected Feedback", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("RoomVacantFeedback")]
        public JoinDataComplete RoomVacantFeedback = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "Room Vacant Feedback", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("RawOccupancyFeedback")]
        public JoinDataComplete RawOccupancyFeedback = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "Raw Occupancy Feedback", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("RawOccupancyPirFeedback")]
        public JoinDataComplete RawOccupancyPirFeedback = new JoinDataComplete(new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata { Description = "Raw Occupancy Pir Feedback", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("RawOccupancyUsFeedback")]
        public JoinDataComplete RawOccupancyUsFeedback = new JoinDataComplete(new JoinData { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata { Description = "Raw Occupancy Us Feedback", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IdentityModeOn")]
        public JoinDataComplete IdentityMode = new JoinDataComplete(new JoinData { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata { Description = "Enable Identity Mode", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IdentityModeFeedback")]
        public JoinDataComplete IdentityModeFeedback = new JoinDataComplete(new JoinData { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata { Description = "Identity Mode Feedback", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("EnableLedFlash")]
        public JoinDataComplete EnableLedFlash = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "Enable Led Flash", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DisableLedFlash")]
        public JoinDataComplete DisableLedFlash = new JoinDataComplete(new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata { Description = "Disable Led Flash", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("EnableShortTimeout")]
        public JoinDataComplete EnableShortTimeout = new JoinDataComplete(new JoinData { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata { Description = "Enable Short Timeout", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DisableShortTimeout")]
        public JoinDataComplete DisableShortTimeout = new JoinDataComplete(new JoinData { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata { Description = "Disable Short Timeout", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("OrWhenVacated")]
        public JoinDataComplete OrWhenVacated = new JoinDataComplete(new JoinData { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata { Description = "Or When Vacated", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("AndWhenVacated")]
        public JoinDataComplete AndWhenVacated = new JoinDataComplete(new JoinData { JoinNumber = 16, JoinSpan = 1 },
            new JoinMetadata { Description = "AndWhenVacated", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("EnableUsA")]
        public JoinDataComplete EnableUsA = new JoinDataComplete(new JoinData { JoinNumber = 17, JoinSpan = 1 },
            new JoinMetadata { Description = "Enable Us A", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DisableUsA")]
        public JoinDataComplete DisableUsA = new JoinDataComplete(new JoinData { JoinNumber = 18, JoinSpan = 1 },
            new JoinMetadata { Description = "Disable Us A", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("EnableUsB")]
        public JoinDataComplete EnableUsB = new JoinDataComplete(new JoinData { JoinNumber = 19, JoinSpan = 1 },
            new JoinMetadata { Description = "Enable Us B", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DisableUsB")]
        public JoinDataComplete DisableUsB = new JoinDataComplete(new JoinData { JoinNumber = 20, JoinSpan = 1 },
            new JoinMetadata { Description = "Disable Us B", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("EnablePir")]
        public JoinDataComplete EnablePir = new JoinDataComplete(new JoinData { JoinNumber = 21, JoinSpan = 1 },
            new JoinMetadata { Description = "Enable Pir", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DisablePir")]
        public JoinDataComplete DisablePir = new JoinDataComplete(new JoinData { JoinNumber = 22, JoinSpan = 1 },
            new JoinMetadata { Description = "Disable Pir", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IncrementUsInOccupiedState")]
        public JoinDataComplete IncrementUsInOccupiedState = new JoinDataComplete(new JoinData { JoinNumber = 23, JoinSpan = 1 },
            new JoinMetadata { Description = "Increment Us In OccupiedState", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DecrementUsInOccupiedState")]
        public JoinDataComplete DecrementUsInOccupiedState = new JoinDataComplete(new JoinData { JoinNumber = 24, JoinSpan = 1 },
            new JoinMetadata { Description = "Dencrement Us In Occupied State", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IncrementUsInVacantState")]
        public JoinDataComplete IncrementUsInVacantState = new JoinDataComplete(new JoinData { JoinNumber = 25, JoinSpan = 1 },
            new JoinMetadata { Description = "Increment Us In VacantState", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DecrementUsInVacantState")]
        public JoinDataComplete DecrementUsInVacantState = new JoinDataComplete(new JoinData { JoinNumber = 26, JoinSpan = 1 },
            new JoinMetadata { Description = "Decrement Us In VacantState", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IncrementPirInOccupiedState")]
        public JoinDataComplete IncrementPirInOccupiedState = new JoinDataComplete(new JoinData { JoinNumber = 27, JoinSpan = 1 },
            new JoinMetadata { Description = "Increment Pir In Occupied State", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DecrementPirInOccupiedState")]
        public JoinDataComplete DecrementPirInOccupiedState = new JoinDataComplete(new JoinData { JoinNumber = 28, JoinSpan = 1 },
            new JoinMetadata { Description = "Decrement Pir In OccupiedState", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IncrementPirInVacantState")]
        public JoinDataComplete IncrementPirInVacantState = new JoinDataComplete(new JoinData { JoinNumber = 29, JoinSpan = 1 },
            new JoinMetadata { Description = "Increment Pir In Vacant State", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DecrementPirInVacantState")]
        public JoinDataComplete DecrementPirInVacantState = new JoinDataComplete(new JoinData { JoinNumber = 30, JoinSpan = 1 },
            new JoinMetadata { Description = "Decrement Pir In Vacant State", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        #endregion

        #region Analog

        [JoinName("Timeout")]
        public JoinDataComplete Timeout = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Timeout", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("TimeoutLocalFeedback")]
        public JoinDataComplete TimeoutLocalFeedback = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Timeout Local Feedback", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("InternalPhotoSensorValue")]
        public JoinDataComplete InternalPhotoSensorValue = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "Internal PhotoSensor Value", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("UsSensitivityInOccupiedState")]
        public JoinDataComplete UsSensitivityInOccupiedState = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "Us Sensitivity In Occupied State", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("UsSensitivityInVacantState")]
        public JoinDataComplete UsSensitivityInVacantState = new JoinDataComplete(new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata { Description = "Us Sensitivity In Vacant State", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("PirSensitivityInOccupiedState")]
        public JoinDataComplete PirSensitivityInOccupiedState = new JoinDataComplete(new JoinData { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata { Description = "Pir Sensitivity In Occupied State", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("PirSensitivityInVacantState")]
        public JoinDataComplete PirSensitivityInVacantState = new JoinDataComplete(new JoinData { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata { Description = "Pir Sensitivity In Vacant State", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        #endregion

        #region Serial

        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        #endregion

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public CenOdtOccupancySensorBaseJoinMap(uint joinStart)
            : this(joinStart, typeof(CenOdtOccupancySensorBaseJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected CenOdtOccupancySensorBaseJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }

}
