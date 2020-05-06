using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Bridges
{
    public class DisplayControllerJoinMap : JoinMapBase
    {
        #region Digitals
        /// <summary>
        /// Turns the display off and reports power off feedback
        /// </summary>
        public uint PowerOff { get; set; }
        /// <summary>
        /// Turns the display on and repots power on feedback
        /// </summary>
        public uint PowerOn { get; set; }
        /// <summary>
        /// Indicates that the display device supports two way communication when high
        /// </summary>
        public uint IsTwoWayDisplay { get; set; }
        /// <summary>
        /// Increments the volume while high
        /// </summary>
        public uint VolumeUp { get; set; }
        /// <summary>
        /// Decrements teh volume while high
        /// </summary>
        public uint VolumeDown { get; set; }
        /// <summary>
        /// Toggles the mute state.  Feedback is high when volume is muted
        /// </summary>
        public uint VolumeMute { get; set; }
        /// <summary>
        /// Range of digital joins to select inputs and report current input as feedback
        /// </summary>
        public uint InputSelectOffset { get; set; }
        /// <summary>
        /// Range of digital joins to report visibility for input buttons
        /// </summary>
        public uint ButtonVisibilityOffset { get; set; }
        /// <summary>
        /// High if the device is online
        /// </summary>
        public uint IsOnline { get; set; }
        #endregion

        #region Analogs
        /// <summary>
        /// Analog join to set the input and report current input as feedback
        /// </summary>
        public uint InputSelect { get; set; }
        /// <summary>
        /// Sets the volume level and reports the current level as feedback 
        /// </summary>
        public uint VolumeLevel { get; set; }
        #endregion

        #region Serials
        /// <summary>
        /// Reports the name of the display as defined in config as feedback
        /// </summary>
        public uint Name { get; set; }
        /// <summary>
        /// Range of serial joins that reports the names of the inputs as feedback
        /// </summary>
        public uint InputNamesOffset { get; set; }
        #endregion

        public DisplayControllerJoinMap()
        {
            // Digital
            IsOnline = 50;
            PowerOff = 1;
            PowerOn = 2;
            IsTwoWayDisplay = 3;
            VolumeUp = 5;
            VolumeDown = 6;
            VolumeMute = 7;

            ButtonVisibilityOffset = 40;
            InputSelectOffset = 10;

            // Analog
            InputSelect = 11;
            VolumeLevel = 5;

            // Serial
            Name = 1;
            InputNamesOffset = 10;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            IsOnline = IsOnline + joinOffset;
            PowerOff = PowerOff + joinOffset;
            PowerOn = PowerOn + joinOffset;
            IsTwoWayDisplay = IsTwoWayDisplay + joinOffset;
            ButtonVisibilityOffset = ButtonVisibilityOffset + joinOffset;
            Name = Name + joinOffset;
            InputNamesOffset = InputNamesOffset + joinOffset;
            InputSelectOffset = InputSelectOffset + joinOffset;

            InputSelect = InputSelect + joinOffset;

            VolumeUp = VolumeUp + joinOffset;
            VolumeDown = VolumeDown + joinOffset;
            VolumeMute = VolumeMute + joinOffset;
            VolumeLevel = VolumeLevel + joinOffset;
        }
    }
}