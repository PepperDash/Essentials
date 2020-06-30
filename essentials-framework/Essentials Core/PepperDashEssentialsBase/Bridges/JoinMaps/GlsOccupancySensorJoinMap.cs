using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class GlsOccupancySensorBaseJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Is Online", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("ForceOccupied")]
        public JoinDataComplete ForceOccupied = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Set to Occupied", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("ForceVacant")]
        public JoinDataComplete ForceVacant = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Set to Vacant", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("EnableRawStates")]
        public JoinDataComplete EnableRawStates = new JoinDataComplete(new JoinData() { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Enable Raw", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("RoomOccupiedFeedback")]
        public JoinDataComplete RoomOccupiedFeedback = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Room Is Occupied", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("GraceOccupancyDetectedFeedback")]
        public JoinDataComplete GraceOccupancyDetectedFeedback = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Grace Occupancy Detected", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("RoomVacantFeedback")]
        public JoinDataComplete RoomVacantFeedback = new JoinDataComplete(new JoinData() { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Room Is Vacant", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("RawOccupancyFeedback")]
        public JoinDataComplete RawOccupancyFeedback = new JoinDataComplete(new JoinData() { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Raw Occupancy Detected", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("RawOccupancyPirFeedback")]
        public JoinDataComplete RawOccupancyPirFeedback = new JoinDataComplete(new JoinData() { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Raw PIR Occupancy Detected", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("RawOccupancyUsFeedback")]
        public JoinDataComplete RawOccupancyUsFeedback = new JoinDataComplete(new JoinData() { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Raw US Occupancy Detected", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("EnableLedFlash")]
        public JoinDataComplete EnableLedFlash = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Enable LED Flash", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DisableLedFlash")]
        public JoinDataComplete DisableLedFlash = new JoinDataComplete(new JoinData() { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Disable LED Flash", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("EnableShortTimeout")]
        public JoinDataComplete EnableShortTimeout = new JoinDataComplete(new JoinData() { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Enable Short Timeout", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DisableShortTimeout")]
        public JoinDataComplete DisableShortTimeout = new JoinDataComplete(new JoinData() { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Disable Short Timeout", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("OrWhenVacated")]
        public JoinDataComplete OrWhenVacated = new JoinDataComplete(new JoinData() { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Set To Vacant when Either Sensor is Vacant", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("AndWhenVacated")]
        public JoinDataComplete AndWhenVacated = new JoinDataComplete(new JoinData() { JoinNumber = 16, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Set To Vacant when Both Sensors are Vacant", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("EnableUsA")]
        public JoinDataComplete EnableUsA = new JoinDataComplete(new JoinData() { JoinNumber = 17, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Enable Ultrasonic Sensor A", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DisableUsA")]
        public JoinDataComplete DisableUsA = new JoinDataComplete(new JoinData() { JoinNumber = 18, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Disable Ultrasonic Sensor A", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("EnableUsB")]
        public JoinDataComplete EnableUsB = new JoinDataComplete(new JoinData() { JoinNumber = 19, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Enable Ultrasonic Sensor B", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DisableUsB")]
        public JoinDataComplete DisableUsB = new JoinDataComplete(new JoinData() { JoinNumber = 20, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Disable Ultrasonic Sensor B", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("EnablePir")]
        public JoinDataComplete EnablePir = new JoinDataComplete(new JoinData() { JoinNumber = 21, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Enable IR Sensor", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DisablePir")]
        public JoinDataComplete DisablePir = new JoinDataComplete(new JoinData() { JoinNumber = 22, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Disable IR Sensor", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IncrementUsInOccupiedState")]
        public JoinDataComplete IncrementUsInOccupiedState = new JoinDataComplete(new JoinData() { JoinNumber = 23, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Increment US Occupied State Sensitivity", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DecrementUsInOccupiedState")]
        public JoinDataComplete DecrementUsInOccupiedState = new JoinDataComplete(new JoinData() { JoinNumber = 24, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Decrement US Occupied State Sensitivity", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IncrementUsInVacantState")]
        public JoinDataComplete IncrementUsInVacantState = new JoinDataComplete(new JoinData() { JoinNumber = 25, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Increment US Vacant State Sensitivity", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DecrementUsInVacantState")]
        public JoinDataComplete DecrementUsInVacantState = new JoinDataComplete(new JoinData() { JoinNumber = 26, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Decrement US Vacant State Sensitivity", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IncrementPirInOccupiedState")]
        public JoinDataComplete IncrementPirInOccupiedState = new JoinDataComplete(new JoinData() { JoinNumber = 27, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Increment IR Occupied State Sensitivity", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DecrementPirInOccupiedState")]
        public JoinDataComplete DecrementPirInOccupiedState = new JoinDataComplete(new JoinData() { JoinNumber = 28, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Decrement IR Occupied State Sensitivity", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("IncrementPirInVacantState")]
        public JoinDataComplete IncrementPirInVacantState = new JoinDataComplete(new JoinData() { JoinNumber = 29, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Increment IR Vacant State Sensitivity", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DecrementPirInVacantState")]
        public JoinDataComplete DecrementPirInVacantState = new JoinDataComplete(new JoinData() { JoinNumber = 30, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Decrement IR Vacant State Sensitivity", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Timeout")]
        public JoinDataComplete Timeout = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Timeout Value", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Analog });

        [JoinName("TimeoutLocalFeedback")]
        public JoinDataComplete TimeoutLocalFeedback = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Local Timeout Value", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("InternalPhotoSensorValue")]
        public JoinDataComplete InternalPhotoSensorValue = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Internal PhotoSensor Value", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("ExternalPhotoSensorValue")]
        public JoinDataComplete ExternalPhotoSensorValue = new JoinDataComplete(new JoinData() { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor External PhotoSensor Value", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("UsSensitivityInOccupiedState")]
        public JoinDataComplete UsSensitivityInOccupiedState = new JoinDataComplete(new JoinData() { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Ultrasonic Sensitivity in Occupied State", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("UsSensitivityInVacantState")]
        public JoinDataComplete UsSensitivityInVacantState = new JoinDataComplete(new JoinData() { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Ultrasonic Sensitivity in Vacant State", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("PirSensitivityInOccupiedState")]
        public JoinDataComplete PirSensitivityInOccupiedState = new JoinDataComplete(new JoinData() { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor PIR Sensitivity in Occupied State", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("PirSensitivityInVacantState")]
        public JoinDataComplete PirSensitivityInVacantState = new JoinDataComplete(new JoinData() { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Ultrasonic Sensitivity in Vacant State", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Occ Sensor Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });



        public GlsOccupancySensorBaseJoinMap(uint joinStart)
            : base(joinStart, typeof(GlsOccupancySensorBaseJoinMap))
        {
        }

    }
}   
