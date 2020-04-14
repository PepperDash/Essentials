using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class GlsOccupancySensorBaseJoinMap : JoinMapBase
    {
        #region Digitals

        /// <summary>
        /// High when device is online
        /// </summary>
        public uint IsOnline { get; set; }
        /// <summary>
        /// Forces the device to report occupied status
        /// </summary>
        public uint ForceOccupied { get; set; }
        /// <summary>
        /// Forces the device to report vacant status
        /// </summary>
        public uint ForceVacant { get; set; }
        /// <summary>
        /// Enables raw status reporting
        /// </summary>
        public uint EnableRawStates { get; set; }
        /// <summary>
        /// High when raw occupancy is detected
        /// </summary>
        public uint RawOccupancyFeedback { get; set; }
        /// <summary>
        /// High when PIR sensor detects motion
        /// </summary>
        public uint RawOccupancyPirFeedback { get; set; }
        /// <summary>
        /// High when US sensor detects motion
        /// </summary>
        public uint RawOccupancyUsFeedback { get; set; }
        /// <summary>
        /// High when occupancy is detected
        /// </summary>
        public uint RoomOccupiedFeedback { get; set; }
        /// <summary>
        /// Hich when occupancy is detected in the grace period
        /// </summary>
        public uint GraceOccupancyDetectedFeedback { get; set; }
        /// <summary>
        /// High when vacancy is detected
        /// </summary>
        public uint RoomVacantFeedback { get; set; }

        /// <summary>
        /// Enables the LED Flash when set high
        /// </summary>
        public uint EnableLedFlash { get; set; }
        /// <summary>
        /// Disables the LED flash when set high
        /// </summary>
        public uint DisableLedFlash { get; set; }
        /// <summary>
        /// Enables the Short Timeout
        /// </summary>
        public uint EnableShortTimeout { get; set; }
        /// <summary>
        /// Disables the Short Timout
        /// </summary>
        public uint DisableShortTimeout { get; set; }
        /// <summary>
        /// Set high to enable one technology to trigger occupancy
        /// </summary>
        public uint OrWhenVacated { get; set; }
        /// <summary>
        /// Set high to require both technologies to trigger occupancy
        /// </summary>
        public uint AndWhenVacated { get; set; }
        /// <summary>
        /// Enables Ultrasonic Sensor A
        /// </summary>
        public uint EnableUsA { get; set; }
        /// <summary>
        /// Disables Ultrasonic Sensor A
        /// </summary>
        public uint DisableUsA { get; set; }
        /// <summary>
        /// Enables Ultrasonic Sensor B
        /// </summary>
        public uint EnableUsB { get; set; }
        /// <summary>
        /// Disables Ultrasonic Sensor B
        /// </summary>
        public uint DisableUsB { get; set; }
        /// <summary>
        /// Enables Pir
        /// </summary>
        public uint EnablePir { get; set; }
        /// <summary>
        /// Disables Pir
        /// </summary>
        public uint DisablePir { get; set; }
        public uint IncrementUsInOccupiedState { get; set; }
        public uint DecrementUsInOccupiedState { get; set; }
        public uint IncrementUsInVacantState { get; set; }
        public uint DecrementUsInVacantState { get; set; }
        public uint IncrementPirInOccupiedState { get; set; }
        public uint DecrementPirInOccupiedState { get; set; }
        public uint IncrementPirInVacantState { get; set; }
        public uint DecrementPirInVacantState { get; set; }
        #endregion

        #region Analogs
        /// <summary>
        /// Sets adn reports the remote timeout value
        /// </summary>
        public uint Timeout { get; set; }
        /// <summary>
        /// Reports the local timeout value
        /// </summary>
        public uint TimeoutLocalFeedback { get; set; }
        /// <summary>
        /// Sets the minimum internal photo sensor value and reports the current level
        /// </summary>
        public uint InternalPhotoSensorValue { get; set; }
        /// <summary>
        /// Sets the minimum external photo sensor value and reports the current level
        /// </summary>
        public uint ExternalPhotoSensorValue { get; set; }

        public uint UsSensitivityInOccupiedState { get; set; }

        public uint UsSensitivityInVacantState { get; set; }

        public uint PirSensitivityInOccupiedState { get; set; }

        public uint PirSensitivityInVacantState { get; set; }
        #endregion

        #region Serial
        public uint Name { get; set; }
        #endregion

        public GlsOccupancySensorBaseJoinMap()
        {
            IsOnline = 1;
            ForceOccupied = 2;
            ForceVacant = 3;
            EnableRawStates = 4;
            RoomOccupiedFeedback = 2;
            GraceOccupancyDetectedFeedback = 3;
            RoomVacantFeedback = 4;
            RawOccupancyFeedback = 5;
            RawOccupancyPirFeedback = 6;
            RawOccupancyUsFeedback = 7;
            EnableLedFlash = 11;
            DisableLedFlash = 12;
            EnableShortTimeout = 13;
            DisableShortTimeout = 14;
            OrWhenVacated = 15;
            AndWhenVacated = 16;
            EnableUsA = 17;
            DisableUsA = 18;
            EnableUsB = 19;
            DisableUsB = 20;
            EnablePir = 21;
            DisablePir = 22;
            IncrementUsInOccupiedState = 23;
            DecrementUsInOccupiedState = 24;
            IncrementUsInVacantState = 25;
            DecrementUsInVacantState = 26;
            IncrementPirInOccupiedState = 27;
            DecrementPirInOccupiedState = 28;
            IncrementPirInVacantState = 29;
            DecrementPirInVacantState = 30;

            Timeout = 1;
            TimeoutLocalFeedback = 2;
            InternalPhotoSensorValue = 3;
            ExternalPhotoSensorValue = 4;
            UsSensitivityInOccupiedState = 5;
            UsSensitivityInVacantState = 6;
            PirSensitivityInOccupiedState = 7;
            PirSensitivityInVacantState = 8;

            Name = 1;
            
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            IsOnline =                          IsOnline + joinOffset; 
            ForceOccupied =                     ForceOccupied + joinOffset; 
            ForceVacant =                       ForceVacant + joinOffset; 
            EnableRawStates =                   EnableRawStates + joinOffset; 
            RoomOccupiedFeedback =              RoomOccupiedFeedback + joinOffset; 
            GraceOccupancyDetectedFeedback =    GraceOccupancyDetectedFeedback + joinOffset;
            RoomVacantFeedback =                RoomVacantFeedback + joinOffset; 
            RawOccupancyFeedback =              RawOccupancyFeedback + joinOffset;
            RawOccupancyPirFeedback =           RawOccupancyPirFeedback + joinOffset;
            RawOccupancyUsFeedback =            RawOccupancyUsFeedback + joinOffset;
            EnableLedFlash =                    EnableLedFlash + joinOffset; 
            DisableLedFlash =                   DisableLedFlash + joinOffset; 
            EnableShortTimeout =                EnableShortTimeout + joinOffset; 
            DisableShortTimeout =               DisableShortTimeout + joinOffset; 
            OrWhenVacated =                     OrWhenVacated + joinOffset; 
            AndWhenVacated =                    AndWhenVacated + joinOffset; 
            EnableUsA =                         EnableUsA + joinOffset; 
            DisableUsA =                        DisableUsA + joinOffset; 
            EnableUsB =                         EnableUsB + joinOffset; 
            DisableUsB =                        DisableUsB + joinOffset; 
            EnablePir =                         EnablePir + joinOffset; 
            DisablePir =                        DisablePir + joinOffset; 
            IncrementUsInOccupiedState =        IncrementUsInOccupiedState + joinOffset; 
            DecrementUsInOccupiedState =        DecrementUsInOccupiedState + joinOffset; 
            IncrementUsInVacantState =          IncrementUsInVacantState + joinOffset; 
            DecrementUsInVacantState =          DecrementUsInVacantState + joinOffset; 
            IncrementPirInOccupiedState =       IncrementPirInOccupiedState + joinOffset;
            DecrementPirInOccupiedState =       DecrementPirInOccupiedState + joinOffset;
            IncrementPirInVacantState =         IncrementPirInVacantState + joinOffset; 
            DecrementPirInVacantState =         DecrementPirInVacantState + joinOffset; 
                                                
            Timeout =                           Timeout + joinOffset; 
            TimeoutLocalFeedback =              TimeoutLocalFeedback + joinOffset; 
            InternalPhotoSensorValue =          InternalPhotoSensorValue + joinOffset; 
            ExternalPhotoSensorValue =          ExternalPhotoSensorValue + joinOffset; 
            UsSensitivityInOccupiedState =      UsSensitivityInOccupiedState + joinOffset;
            UsSensitivityInVacantState =        UsSensitivityInVacantState + joinOffset; 
            PirSensitivityInOccupiedState =     PirSensitivityInOccupiedState + joinOffset;
            PirSensitivityInVacantState =       PirSensitivityInVacantState + joinOffset;

            Name =                              Name + joinOffset;
        }
    }    
         
}        
