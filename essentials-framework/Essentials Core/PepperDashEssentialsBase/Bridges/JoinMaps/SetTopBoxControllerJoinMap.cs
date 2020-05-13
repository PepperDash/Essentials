using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using Crestron.SimplSharp.Reflection;


namespace PepperDash.Essentials.Core.Bridges
{
    public class SetTopBoxControllerJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("PowerOn")]
        public JoinDataComplete PowerOn = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Power On", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("PowerOff")]
        public JoinDataComplete PowerOff = new JoinDataComplete(new JoinData() { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Power Off", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("PowerToggle")]
        public JoinDataComplete PowerToggle = new JoinDataComplete(new JoinData() { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Power Toggle", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("HasDpad")]
        public JoinDataComplete HasDpad = new JoinDataComplete(new JoinData() { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Has DPad", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Up")]
        public JoinDataComplete Up = new JoinDataComplete(new JoinData() { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Nav Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Down")]
        public JoinDataComplete Down = new JoinDataComplete(new JoinData() { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Nav Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Left")]
        public JoinDataComplete Left = new JoinDataComplete(new JoinData() { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Nav Left", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Right")]
        public JoinDataComplete Right = new JoinDataComplete(new JoinData() { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Nav Right", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Select")]
        public JoinDataComplete Select = new JoinDataComplete(new JoinData() { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Select", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Menu")]
        public JoinDataComplete Menu = new JoinDataComplete(new JoinData() { JoinNumber = 9, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Menu", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Exit")]
        public JoinDataComplete Exit = new JoinDataComplete(new JoinData() { JoinNumber = 10, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Exit", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("HasNumeric")]
        public JoinDataComplete HasNumeric = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Has Numeric", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Digit0")]
        public JoinDataComplete Digit0 = new JoinDataComplete(new JoinData() { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Digit 0", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Digit1")]
        public JoinDataComplete Digit1 = new JoinDataComplete(new JoinData() { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Digit 1", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Digit2")]
        public JoinDataComplete Digit2 = new JoinDataComplete(new JoinData() { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Digit 2", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Digit3")]
        public JoinDataComplete Digit3 = new JoinDataComplete(new JoinData() { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Digit 3", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Digit4")]
        public JoinDataComplete Digit4 = new JoinDataComplete(new JoinData() { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Digit 4", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Digit5")]
        public JoinDataComplete Digit5 = new JoinDataComplete(new JoinData() { JoinNumber = 16, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Digit 5", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Digit6")]
        public JoinDataComplete Digit6 = new JoinDataComplete(new JoinData() { JoinNumber = 17, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Digit 6", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Digit7")]
        public JoinDataComplete Digit7 = new JoinDataComplete(new JoinData() { JoinNumber = 18, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Digit 7", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Digit8")]
        public JoinDataComplete Digit8 = new JoinDataComplete(new JoinData() { JoinNumber = 19, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Digit 8", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Digit9")]
        public JoinDataComplete Digit9 = new JoinDataComplete(new JoinData() { JoinNumber = 20, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Digit 9", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Dash")]
        public JoinDataComplete Dash = new JoinDataComplete(new JoinData() { JoinNumber = 21, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Dash", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("KeypadEnter")]
        public JoinDataComplete KeypadEnter = new JoinDataComplete(new JoinData() { JoinNumber = 22, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Keypad Enter", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("ChannelUp")]
        public JoinDataComplete ChannelUp = new JoinDataComplete(new JoinData() { JoinNumber = 23, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Channel Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("ChannelDown")]
        public JoinDataComplete ChannelDown = new JoinDataComplete(new JoinData() { JoinNumber = 24, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Channel Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("LastChannel")]
        public JoinDataComplete LastChannel = new JoinDataComplete(new JoinData() { JoinNumber = 25, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Last Channel", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Guide")]
        public JoinDataComplete Guide = new JoinDataComplete(new JoinData() { JoinNumber = 26, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Guide", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Info")]
        public JoinDataComplete Info = new JoinDataComplete(new JoinData() { JoinNumber = 27, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Info", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Red")]
        public JoinDataComplete Red = new JoinDataComplete(new JoinData() { JoinNumber = 28, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Red", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Green")]
        public JoinDataComplete Green = new JoinDataComplete(new JoinData() { JoinNumber = 29, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Green", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Yellow")]
        public JoinDataComplete Yellow = new JoinDataComplete(new JoinData() { JoinNumber = 30, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Yellow", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Blue")]
        public JoinDataComplete Blue = new JoinDataComplete(new JoinData() { JoinNumber = 31, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Blue", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("HasDvr")]
        public JoinDataComplete HasDvr = new JoinDataComplete(new JoinData() { JoinNumber = 32, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Has DVR", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("DvrList")]
        public JoinDataComplete DvrList = new JoinDataComplete(new JoinData() { JoinNumber = 32, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB DvrList", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Play")]
        public JoinDataComplete Play = new JoinDataComplete(new JoinData() { JoinNumber = 33, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Play", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Pause")]
        public JoinDataComplete Pause = new JoinDataComplete(new JoinData() { JoinNumber = 34, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Pause", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Stop")]
        public JoinDataComplete Stop = new JoinDataComplete(new JoinData() { JoinNumber = 35, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Stop", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("FFwd")]
        public JoinDataComplete FFwd = new JoinDataComplete(new JoinData() { JoinNumber = 36, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB FFwd", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Rewind")]
        public JoinDataComplete Rewind = new JoinDataComplete(new JoinData() { JoinNumber = 37, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Rewind", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("ChapPlus")]
        public JoinDataComplete ChapPlus = new JoinDataComplete(new JoinData() { JoinNumber = 38, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Chapter Plus", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("ChapMinus")]
        public JoinDataComplete ChapMinus = new JoinDataComplete(new JoinData() { JoinNumber = 39, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Chapter Minus", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Replay")]
        public JoinDataComplete Replay = new JoinDataComplete(new JoinData() { JoinNumber = 40, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Replay", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Record")]
        public JoinDataComplete Record = new JoinDataComplete(new JoinData() { JoinNumber = 41, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Record", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("HasKeypadAccessoryButton1")]
        public JoinDataComplete HasKeypadAccessoryButton1 = new JoinDataComplete(new JoinData() { JoinNumber = 42, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Has Keypad Accessory Button 1", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("HasKeypadAccessoryButton2")]
        public JoinDataComplete HasKeypadAccessoryButton2 = new JoinDataComplete(new JoinData() { JoinNumber = 43, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Has Keypad Accessory Button 2", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("KeypadAccessoryButton1Press")]
        public JoinDataComplete KeypadAccessoryButton1Press = new JoinDataComplete(new JoinData() { JoinNumber = 42, JoinSpan = 2 },
            new JoinMetadata() { Label = "STB Keypad Accessory Button 1 Press", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("KeypadAccessoryButton2Press")]
        public JoinDataComplete KeypadAccessoryButton2Press = new JoinDataComplete(new JoinData() { JoinNumber = 43, JoinSpan = 2 },
            new JoinMetadata() { Label = "STB Keypad Accessory Button 2 Press", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData() { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        [JoinName("KeypadAccessoryButton1Label")]
        public JoinDataComplete KeypadAccessoryButton1Label = new JoinDataComplete(new JoinData() { JoinNumber = 42, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Keypad Accessory Button 1 Label", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("KeypadAccessoryButton2Label")]
        public JoinDataComplete KeypadAccessoryButton2Label = new JoinDataComplete(new JoinData() { JoinNumber = 43, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Keypad Accessory Button 1 Label", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        [JoinName("LoadPresets")]
        public JoinDataComplete LoadPresets = new JoinDataComplete(new JoinData() { JoinNumber = 50, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Load Presets", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        [JoinName("HasPresets")]
        public JoinDataComplete HasPresets = new JoinDataComplete(new JoinData() { JoinNumber = 50, JoinSpan = 1 },
            new JoinMetadata() { Label = "STB Load Presets", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });



        public SetTopBoxControllerJoinMap(uint joinStart)
            : base(joinStart, typeof(SetTopBoxControllerJoinMap))
        {
        }
    }
}