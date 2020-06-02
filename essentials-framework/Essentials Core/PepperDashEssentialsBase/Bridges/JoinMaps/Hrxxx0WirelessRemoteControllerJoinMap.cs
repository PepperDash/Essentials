using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Bridges
{
    public class Hrxxx0WirelessRemoteControllerJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("Power")]
        public JoinDataComplete Power = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "Power", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Menu")]
        public JoinDataComplete Menu = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "Menu", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Guide")]
        public JoinDataComplete Guide = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata() { Label = "Guide", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Info")]
        public JoinDataComplete Info = new JoinDataComplete(new JoinData() { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata() { Label = "Info", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeUp")]
        public JoinDataComplete VolumeUp = new JoinDataComplete(new JoinData() { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata() { Label = "VolumeUp", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("VolumeDown")]
        public JoinDataComplete VolumeDown = new JoinDataComplete(new JoinData() { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata() { Label = "VolumeDown", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DialPadUp")]
        public JoinDataComplete DialPadUp = new JoinDataComplete(new JoinData() { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata() { Label = "DialPadUp", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DialPadDown")]
        public JoinDataComplete DialPadDown = new JoinDataComplete(new JoinData() { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata() { Label = "DialPadDown", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DialPadLeft")]
        public JoinDataComplete DialPadLeft = new JoinDataComplete(new JoinData() { JoinNumber = 9, JoinSpan = 1 },
            new JoinMetadata() { Label = "DialPadLeft", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DialPadRight")]
        public JoinDataComplete DialPadRight = new JoinDataComplete(new JoinData() { JoinNumber = 10, JoinSpan = 1 },
            new JoinMetadata() { Label = "DialPadRight", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DialPadSelect")]
        public JoinDataComplete DialPadSelect = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata() { Label = "DialPadSelect", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("ChannelUp")]
        public JoinDataComplete ChannelUp = new JoinDataComplete(new JoinData() { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata() { Label = "ChannelUp", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("ChannelDown")]
        public JoinDataComplete ChannelDown = new JoinDataComplete(new JoinData() { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata() { Label = "ChannelDown", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Mute")]
        public JoinDataComplete Mute = new JoinDataComplete(new JoinData() { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata() { Label = "Mute", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Exit")]
        public JoinDataComplete Exit = new JoinDataComplete(new JoinData() { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata() { Label = "Exit", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Last")]
        public JoinDataComplete Last = new JoinDataComplete(new JoinData() { JoinNumber = 16, JoinSpan = 1 },
            new JoinMetadata() { Label = "Last", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Play")]
        public JoinDataComplete Play = new JoinDataComplete(new JoinData() { JoinNumber = 17, JoinSpan = 1 },
            new JoinMetadata() { Label = "Play", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Pause")]
        public JoinDataComplete Pause = new JoinDataComplete(new JoinData() { JoinNumber = 18, JoinSpan = 1 },
            new JoinMetadata() { Label = "Pause", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Rewind")]
        public JoinDataComplete Rewind = new JoinDataComplete(new JoinData() { JoinNumber = 19, JoinSpan = 1 },
            new JoinMetadata() { Label = "Rewind", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("FastForward")]
        public JoinDataComplete FastForward = new JoinDataComplete(new JoinData() { JoinNumber = 20, JoinSpan = 1 },
            new JoinMetadata() { Label = "FastForward", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("PreviousTrack")]
        public JoinDataComplete PreviousTrack = new JoinDataComplete(new JoinData() { JoinNumber = 21, JoinSpan = 1 },
            new JoinMetadata() { Label = "PreviousTrack", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("NextTrack")]
        public JoinDataComplete NextTrack = new JoinDataComplete(new JoinData() { JoinNumber = 22, JoinSpan = 1 },
            new JoinMetadata() { Label = "NextTrack", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Stop")]
        public JoinDataComplete Stop = new JoinDataComplete(new JoinData() { JoinNumber = 23, JoinSpan = 1 },
            new JoinMetadata() { Label = "Stop", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Record")]
        public JoinDataComplete Record = new JoinDataComplete(new JoinData() { JoinNumber = 24, JoinSpan = 1 },
            new JoinMetadata() { Label = "Record", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Dvr")]
        public JoinDataComplete Dvr = new JoinDataComplete(new JoinData() { JoinNumber = 25, JoinSpan = 1 },
            new JoinMetadata() { Label = "Dvr", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad1")]
        public JoinDataComplete Keypad1 = new JoinDataComplete(new JoinData() { JoinNumber = 26, JoinSpan = 1 },
            new JoinMetadata() { Label = "Keypad1", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad2Abc")]
        public JoinDataComplete Keypad2 = new JoinDataComplete(new JoinData() { JoinNumber = 27, JoinSpan = 1 },
            new JoinMetadata() { Label = "Keypad2Abc", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad3Def")]
        public JoinDataComplete Keypad3Def = new JoinDataComplete(new JoinData() { JoinNumber = 28, JoinSpan = 1 },
            new JoinMetadata() { Label = "Keypad3Def", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad4Ghi")]
        public JoinDataComplete Keypad4Ghi = new JoinDataComplete(new JoinData() { JoinNumber = 29, JoinSpan = 1 },
            new JoinMetadata() { Label = "Keypad4Ghi", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad5Jkl")]
        public JoinDataComplete Keypad5Jkl = new JoinDataComplete(new JoinData() { JoinNumber = 30, JoinSpan = 1 },
            new JoinMetadata() { Label = "Keypad5Jkl", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad6Mno")]
        public JoinDataComplete Keypad6Mno = new JoinDataComplete(new JoinData() { JoinNumber = 31, JoinSpan = 1 },
            new JoinMetadata() { Label = "Keypad6Mno", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad7Pqrs")]
        public JoinDataComplete Keypad7Pqrs = new JoinDataComplete(new JoinData() { JoinNumber = 32, JoinSpan = 1 },
            new JoinMetadata() { Label = "Keypad7Pqrs", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad8Tuv")]
        public JoinDataComplete Keypad8Tuv = new JoinDataComplete(new JoinData() { JoinNumber = 33, JoinSpan = 1 },
            new JoinMetadata() { Label = "Keypad8Tuv", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad9Wxyz")]
        public JoinDataComplete Keypad9Wxyz = new JoinDataComplete(new JoinData() { JoinNumber = 34, JoinSpan = 1 },
            new JoinMetadata() { Label = "Keypad9Wxyz", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Keypad0")]
        public JoinDataComplete Keypad0 = new JoinDataComplete(new JoinData() { JoinNumber = 35, JoinSpan = 1 },
            new JoinMetadata() { Label = "Keypad0", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Clear")]
        public JoinDataComplete Clear = new JoinDataComplete(new JoinData() { JoinNumber = 36, JoinSpan = 1 },
            new JoinMetadata() { Label = "Clear", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Enter")]
        public JoinDataComplete Enter = new JoinDataComplete(new JoinData() { JoinNumber = 37, JoinSpan = 1 },
            new JoinMetadata() { Label = "Enter", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Red")]
        public JoinDataComplete Red = new JoinDataComplete(new JoinData() { JoinNumber = 38, JoinSpan = 1 },
            new JoinMetadata() { Label = "Red", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Green")]
        public JoinDataComplete Green = new JoinDataComplete(new JoinData() { JoinNumber = 39, JoinSpan = 1 },
            new JoinMetadata() { Label = "Green", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Yellow")]
        public JoinDataComplete Yellow = new JoinDataComplete(new JoinData() { JoinNumber = 40, JoinSpan = 1 },
            new JoinMetadata() { Label = "Yellow", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Blue")]
        public JoinDataComplete Blue = new JoinDataComplete(new JoinData() { JoinNumber = 41, JoinSpan = 1 },
            new JoinMetadata() { Label = "Blue", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom1")]
        public JoinDataComplete Custom1 = new JoinDataComplete(new JoinData() { JoinNumber = 42, JoinSpan = 1 },
            new JoinMetadata() { Label = "Custom1", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom2")]
        public JoinDataComplete Custom2 = new JoinDataComplete(new JoinData() { JoinNumber = 43, JoinSpan = 1 },
            new JoinMetadata() { Label = "Custom2", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom3")]
        public JoinDataComplete Custom3 = new JoinDataComplete(new JoinData() { JoinNumber = 44, JoinSpan = 1 },
            new JoinMetadata() { Label = "Custom3", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom4")]
        public JoinDataComplete Custom4 = new JoinDataComplete(new JoinData() { JoinNumber = 45, JoinSpan = 1 },
            new JoinMetadata() { Label = "Custom4", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom5")]
        public JoinDataComplete Custom5 = new JoinDataComplete(new JoinData() { JoinNumber = 46, JoinSpan = 1 },
            new JoinMetadata() { Label = "Custom5", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom6")]
        public JoinDataComplete Custom6 = new JoinDataComplete(new JoinData() { JoinNumber = 47, JoinSpan = 1 },
            new JoinMetadata() { Label = "Custom6", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom7")]
        public JoinDataComplete Custom7 = new JoinDataComplete(new JoinData() { JoinNumber = 48, JoinSpan = 1 },
            new JoinMetadata() { Label = "Custom7", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom8")]
        public JoinDataComplete Custom8 = new JoinDataComplete(new JoinData() { JoinNumber = 49, JoinSpan = 1 },
            new JoinMetadata() { Label = "Custom8", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Custom9")]
        public JoinDataComplete Custom9 = new JoinDataComplete(new JoinData() { JoinNumber = 50, JoinSpan = 1 },
            new JoinMetadata() { Label = "Custom9", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Fav")]
        public JoinDataComplete Fav = new JoinDataComplete(new JoinData() { JoinNumber = 51, JoinSpan = 1 },
            new JoinMetadata() { Label = "Fav", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Home")]
        public JoinDataComplete Home = new JoinDataComplete(new JoinData() { JoinNumber = 52, JoinSpan = 1 },
            new JoinMetadata() { Label = "Home", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("BatteryLow")]
        public JoinDataComplete BatteryLow = new JoinDataComplete(new JoinData() { JoinNumber = 53, JoinSpan = 1 },
            new JoinMetadata() { Label = "BatteryLow", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("BatteryCritical")]
        public JoinDataComplete BatteryCritical = new JoinDataComplete(new JoinData() { JoinNumber = 54, JoinSpan = 1 },
            new JoinMetadata() { Label = "BatteryCritical", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("BatteryVoltage")]
        public JoinDataComplete BatteryVoltage = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "BatteryVoltage", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        public Hrxxx0WirelessRemoteControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(Hrxxx0WirelessRemoteControllerJoinMap))
        {
        }


    }
}