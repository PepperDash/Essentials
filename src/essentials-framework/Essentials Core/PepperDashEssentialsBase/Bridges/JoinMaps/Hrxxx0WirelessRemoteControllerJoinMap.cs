﻿using System;

namespace PepperDash.Essentials.Core.Bridges
{
    public class Hrxxx0WirelessRemoteControllerJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("Power")]
        public JoinDataComplete Power = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Power", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Menu")]
        public JoinDataComplete Menu = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Menu", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Guide")]
        public JoinDataComplete Guide = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "Guide", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Info")]
        public JoinDataComplete Info = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "Info", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeUp")]
        public JoinDataComplete VolumeUp = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "VolumeUp", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeDown")]
        public JoinDataComplete VolumeDown = new JoinDataComplete(new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata { Description = "VolumeDown", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DialPadUp")]
        public JoinDataComplete DialPadUp = new JoinDataComplete(new JoinData { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata { Description = "DialPadUp", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DialPadDown")]
        public JoinDataComplete DialPadDown = new JoinDataComplete(new JoinData { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata { Description = "DialPadDown", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DialPadLeft")]
        public JoinDataComplete DialPadLeft = new JoinDataComplete(new JoinData { JoinNumber = 9, JoinSpan = 1 },
            new JoinMetadata { Description = "DialPadLeft", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DialPadRight")]
        public JoinDataComplete DialPadRight = new JoinDataComplete(new JoinData { JoinNumber = 10, JoinSpan = 1 },
            new JoinMetadata { Description = "DialPadRight", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DialPadSelect")]
        public JoinDataComplete DialPadSelect = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "DialPadSelect", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("ChannelUp")]
        public JoinDataComplete ChannelUp = new JoinDataComplete(new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata { Description = "ChannelUp", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("ChannelDown")]
        public JoinDataComplete ChannelDown = new JoinDataComplete(new JoinData { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata { Description = "ChannelDown", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Mute")]
        public JoinDataComplete Mute = new JoinDataComplete(new JoinData { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata { Description = "Mute", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Exit")]
        public JoinDataComplete Exit = new JoinDataComplete(new JoinData { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata { Description = "Exit", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Last")]
        public JoinDataComplete Last = new JoinDataComplete(new JoinData { JoinNumber = 16, JoinSpan = 1 },
            new JoinMetadata { Description = "Last", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Play")]
        public JoinDataComplete Play = new JoinDataComplete(new JoinData { JoinNumber = 17, JoinSpan = 1 },
            new JoinMetadata { Description = "Play", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Pause")]
        public JoinDataComplete Pause = new JoinDataComplete(new JoinData { JoinNumber = 18, JoinSpan = 1 },
            new JoinMetadata { Description = "Pause", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Rewind")]
        public JoinDataComplete Rewind = new JoinDataComplete(new JoinData { JoinNumber = 19, JoinSpan = 1 },
            new JoinMetadata { Description = "Rewind", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("FastForward")]
        public JoinDataComplete FastForward = new JoinDataComplete(new JoinData { JoinNumber = 20, JoinSpan = 1 },
            new JoinMetadata { Description = "FastForward", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("PreviousTrack")]
        public JoinDataComplete PreviousTrack = new JoinDataComplete(new JoinData { JoinNumber = 21, JoinSpan = 1 },
            new JoinMetadata { Description = "PreviousTrack", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("NextTrack")]
        public JoinDataComplete NextTrack = new JoinDataComplete(new JoinData { JoinNumber = 22, JoinSpan = 1 },
            new JoinMetadata { Description = "NextTrack", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Stop")]
        public JoinDataComplete Stop = new JoinDataComplete(new JoinData { JoinNumber = 23, JoinSpan = 1 },
            new JoinMetadata { Description = "Stop", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Record")]
        public JoinDataComplete Record = new JoinDataComplete(new JoinData { JoinNumber = 24, JoinSpan = 1 },
            new JoinMetadata { Description = "Record", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Dvr")]
        public JoinDataComplete Dvr = new JoinDataComplete(new JoinData { JoinNumber = 25, JoinSpan = 1 },
            new JoinMetadata { Description = "Dvr", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad1")]
        public JoinDataComplete Keypad1 = new JoinDataComplete(new JoinData { JoinNumber = 26, JoinSpan = 1 },
            new JoinMetadata { Description = "Keypad1", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad2Abc")]
        public JoinDataComplete Keypad2 = new JoinDataComplete(new JoinData { JoinNumber = 27, JoinSpan = 1 },
            new JoinMetadata { Description = "Keypad2Abc", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad3Def")]
        public JoinDataComplete Keypad3Def = new JoinDataComplete(new JoinData { JoinNumber = 28, JoinSpan = 1 },
            new JoinMetadata { Description = "Keypad3Def", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad4Ghi")]
        public JoinDataComplete Keypad4Ghi = new JoinDataComplete(new JoinData { JoinNumber = 29, JoinSpan = 1 },
            new JoinMetadata { Description = "Keypad4Ghi", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad5Jkl")]
        public JoinDataComplete Keypad5Jkl = new JoinDataComplete(new JoinData { JoinNumber = 30, JoinSpan = 1 },
            new JoinMetadata { Description = "Keypad5Jkl", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad6Mno")]
        public JoinDataComplete Keypad6Mno = new JoinDataComplete(new JoinData { JoinNumber = 31, JoinSpan = 1 },
            new JoinMetadata { Description = "Keypad6Mno", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad7Pqrs")]
        public JoinDataComplete Keypad7Pqrs = new JoinDataComplete(new JoinData { JoinNumber = 32, JoinSpan = 1 },
            new JoinMetadata { Description = "Keypad7Pqrs", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad8Tuv")]
        public JoinDataComplete Keypad8Tuv = new JoinDataComplete(new JoinData { JoinNumber = 33, JoinSpan = 1 },
            new JoinMetadata { Description = "Keypad8Tuv", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad9Wxyz")]
        public JoinDataComplete Keypad9Wxyz = new JoinDataComplete(new JoinData { JoinNumber = 34, JoinSpan = 1 },
            new JoinMetadata { Description = "Keypad9Wxyz", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad0")]
        public JoinDataComplete Keypad0 = new JoinDataComplete(new JoinData { JoinNumber = 35, JoinSpan = 1 },
            new JoinMetadata { Description = "Keypad0", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Clear")]
        public JoinDataComplete Clear = new JoinDataComplete(new JoinData { JoinNumber = 36, JoinSpan = 1 },
            new JoinMetadata { Description = "Clear", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Enter")]
        public JoinDataComplete Enter = new JoinDataComplete(new JoinData { JoinNumber = 37, JoinSpan = 1 },
            new JoinMetadata { Description = "Enter", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Red")]
        public JoinDataComplete Red = new JoinDataComplete(new JoinData { JoinNumber = 38, JoinSpan = 1 },
            new JoinMetadata { Description = "Red", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Green")]
        public JoinDataComplete Green = new JoinDataComplete(new JoinData { JoinNumber = 39, JoinSpan = 1 },
            new JoinMetadata { Description = "Green", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Yellow")]
        public JoinDataComplete Yellow = new JoinDataComplete(new JoinData { JoinNumber = 40, JoinSpan = 1 },
            new JoinMetadata { Description = "Yellow", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Blue")]
        public JoinDataComplete Blue = new JoinDataComplete(new JoinData { JoinNumber = 41, JoinSpan = 1 },
            new JoinMetadata { Description = "Blue", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom1")]
        public JoinDataComplete Custom1 = new JoinDataComplete(new JoinData { JoinNumber = 42, JoinSpan = 1 },
            new JoinMetadata { Description = "Custom1", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom2")]
        public JoinDataComplete Custom2 = new JoinDataComplete(new JoinData { JoinNumber = 43, JoinSpan = 1 },
            new JoinMetadata { Description = "Custom2", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom3")]
        public JoinDataComplete Custom3 = new JoinDataComplete(new JoinData { JoinNumber = 44, JoinSpan = 1 },
            new JoinMetadata { Description = "Custom3", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom4")]
        public JoinDataComplete Custom4 = new JoinDataComplete(new JoinData { JoinNumber = 45, JoinSpan = 1 },
            new JoinMetadata { Description = "Custom4", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom5")]
        public JoinDataComplete Custom5 = new JoinDataComplete(new JoinData { JoinNumber = 46, JoinSpan = 1 },
            new JoinMetadata { Description = "Custom5", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom6")]
        public JoinDataComplete Custom6 = new JoinDataComplete(new JoinData { JoinNumber = 47, JoinSpan = 1 },
            new JoinMetadata { Description = "Custom6", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom7")]
        public JoinDataComplete Custom7 = new JoinDataComplete(new JoinData { JoinNumber = 48, JoinSpan = 1 },
            new JoinMetadata { Description = "Custom7", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom8")]
        public JoinDataComplete Custom8 = new JoinDataComplete(new JoinData { JoinNumber = 49, JoinSpan = 1 },
            new JoinMetadata { Description = "Custom8", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom9")]
        public JoinDataComplete Custom9 = new JoinDataComplete(new JoinData { JoinNumber = 50, JoinSpan = 1 },
            new JoinMetadata { Description = "Custom9", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Fav")]
        public JoinDataComplete Fav = new JoinDataComplete(new JoinData { JoinNumber = 51, JoinSpan = 1 },
            new JoinMetadata { Description = "Fav", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Home")]
        public JoinDataComplete Home = new JoinDataComplete(new JoinData { JoinNumber = 52, JoinSpan = 1 },
            new JoinMetadata { Description = "Home", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("BatteryLow")]
        public JoinDataComplete BatteryLow = new JoinDataComplete(new JoinData { JoinNumber = 53, JoinSpan = 1 },
            new JoinMetadata { Description = "BatteryLow", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("BatteryCritical")]
        public JoinDataComplete BatteryCritical = new JoinDataComplete(new JoinData { JoinNumber = 54, JoinSpan = 1 },
            new JoinMetadata { Description = "BatteryCritical", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("BatteryVoltage")]
        public JoinDataComplete BatteryVoltage = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "BatteryVoltage", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public Hrxxx0WirelessRemoteControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(Hrxxx0WirelessRemoteControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected Hrxxx0WirelessRemoteControllerJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}