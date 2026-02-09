using System;


namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a SetTopBoxControllerJoinMap
    /// </summary>
    public class SetTopBoxControllerJoinMap : JoinMapBaseAdvanced
    {
        /// <summary>
        /// STB Power On
        /// </summary>
        [JoinName("PowerOn")]
        public JoinDataComplete PowerOn = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Power On", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Power Off
        /// </summary>
        [JoinName("PowerOff")]
        public JoinDataComplete PowerOff = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Power Off", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Power Toggle
        /// </summary>
        [JoinName("PowerToggle")]
        public JoinDataComplete PowerToggle = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Power Toggle", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Has DPad
        /// </summary>
        [JoinName("HasDpad")]
        public JoinDataComplete HasDpad = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Has DPad", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Nav Up
        /// </summary>
        [JoinName("Up")]
        public JoinDataComplete Up = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Nav Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Nav Down
        /// </summary>
        [JoinName("Down")]
        public JoinDataComplete Down = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Nav Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Nav Left
        /// </summary>
        [JoinName("Left")]
        public JoinDataComplete Left = new JoinDataComplete(new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Nav Left", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Nav Right
        /// </summary>
        [JoinName("Right")]
        public JoinDataComplete Right = new JoinDataComplete(new JoinData { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Nav Right", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Select
        /// </summary>
        [JoinName("Select")]
        public JoinDataComplete Select = new JoinDataComplete(new JoinData { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Select", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Menu
        /// </summary>
        [JoinName("Menu")]
        public JoinDataComplete Menu = new JoinDataComplete(new JoinData { JoinNumber = 9, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Menu", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Exit
        /// </summary>
        [JoinName("Exit")]
        public JoinDataComplete Exit = new JoinDataComplete(new JoinData { JoinNumber = 10, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Exit", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Has Numeric
        /// </summary>
        [JoinName("HasNumeric")]
        public JoinDataComplete HasNumeric = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Has Numeric", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Digit 0
        /// </summary>
        [JoinName("Digit0")]
        public JoinDataComplete Digit0 = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Digit 0", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Digit 1
        /// </summary>
        [JoinName("Digit1")]
        public JoinDataComplete Digit1 = new JoinDataComplete(new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Digit 1", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Digit 2
        /// </summary>
        [JoinName("Digit2")]
        public JoinDataComplete Digit2 = new JoinDataComplete(new JoinData { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Digit 2", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Digit 3
        /// </summary>
        [JoinName("Digit3")]
        public JoinDataComplete Digit3 = new JoinDataComplete(new JoinData { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Digit 3", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Digit 4
        /// </summary>
        [JoinName("Digit4")]
        public JoinDataComplete Digit4 = new JoinDataComplete(new JoinData { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Digit 4", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Digit 5
        /// </summary>
        [JoinName("Digit5")]
        public JoinDataComplete Digit5 = new JoinDataComplete(new JoinData { JoinNumber = 16, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Digit 5", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Digit 6
        /// </summary>
        [JoinName("Digit6")]
        public JoinDataComplete Digit6 = new JoinDataComplete(new JoinData { JoinNumber = 17, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Digit 6", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Digit 7
        /// </summary>
        [JoinName("Digit7")]
        public JoinDataComplete Digit7 = new JoinDataComplete(new JoinData { JoinNumber = 18, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Digit 7", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Digit 8
        /// </summary>
        [JoinName("Digit8")]
        public JoinDataComplete Digit8 = new JoinDataComplete(new JoinData { JoinNumber = 19, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Digit 8", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Digit 9
        /// </summary>
        [JoinName("Digit9")]
        public JoinDataComplete Digit9 = new JoinDataComplete(new JoinData { JoinNumber = 20, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Digit 9", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Dash
        /// </summary>
        [JoinName("Dash")]
        public JoinDataComplete Dash = new JoinDataComplete(new JoinData { JoinNumber = 21, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Dash", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Keypad Enter
        /// </summary>
        [JoinName("KeypadEnter")]
        public JoinDataComplete KeypadEnter = new JoinDataComplete(new JoinData { JoinNumber = 22, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Keypad Enter", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Channel Up
        /// </summary>
        [JoinName("ChannelUp")]
        public JoinDataComplete ChannelUp = new JoinDataComplete(new JoinData { JoinNumber = 23, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Channel Up", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Channel Down
        /// </summary>
        [JoinName("ChannelDown")]
        public JoinDataComplete ChannelDown = new JoinDataComplete(new JoinData { JoinNumber = 24, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Channel Down", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Last Channel
        /// </summary>
        [JoinName("LastChannel")]
        public JoinDataComplete LastChannel = new JoinDataComplete(new JoinData { JoinNumber = 25, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Last Channel", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Guide
        /// </summary>
        [JoinName("Guide")]
        public JoinDataComplete Guide = new JoinDataComplete(new JoinData { JoinNumber = 26, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Guide", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Info
        /// </summary>
        [JoinName("Info")]
        public JoinDataComplete Info = new JoinDataComplete(new JoinData { JoinNumber = 27, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Info", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Red
        /// </summary>
        [JoinName("Red")]
        public JoinDataComplete Red = new JoinDataComplete(new JoinData { JoinNumber = 28, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Red", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Green
        /// </summary>
        [JoinName("Green")]
        public JoinDataComplete Green = new JoinDataComplete(new JoinData { JoinNumber = 29, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Green", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Yellow
        /// </summary>
        [JoinName("Yellow")]
        public JoinDataComplete Yellow = new JoinDataComplete(new JoinData { JoinNumber = 30, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Yellow", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Blue
        /// </summary>
        [JoinName("Blue")]
        public JoinDataComplete Blue = new JoinDataComplete(new JoinData { JoinNumber = 31, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Blue", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Has DVR
        /// </summary>
        [JoinName("HasDvr")]
        public JoinDataComplete HasDvr = new JoinDataComplete(new JoinData { JoinNumber = 32, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Has DVR", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Dvr List
        /// </summary>
        [JoinName("DvrList")]
        public JoinDataComplete DvrList = new JoinDataComplete(new JoinData { JoinNumber = 32, JoinSpan = 1 },
            new JoinMetadata { Description = "STB DvrList", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Play
        /// </summary>
        [JoinName("Play")]
        public JoinDataComplete Play = new JoinDataComplete(new JoinData { JoinNumber = 33, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Play", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Pause
        /// </summary>
        [JoinName("Pause")]
        public JoinDataComplete Pause = new JoinDataComplete(new JoinData { JoinNumber = 34, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Pause", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Stop
        /// </summary>
        [JoinName("Stop")]
        public JoinDataComplete Stop = new JoinDataComplete(new JoinData { JoinNumber = 35, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Stop", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB FFwd
        /// </summary>
        [JoinName("FFwd")]
        public JoinDataComplete FFwd = new JoinDataComplete(new JoinData { JoinNumber = 36, JoinSpan = 1 },
            new JoinMetadata { Description = "STB FFwd", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Rewind
        /// </summary>
        [JoinName("Rewind")]
        public JoinDataComplete Rewind = new JoinDataComplete(new JoinData { JoinNumber = 37, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Rewind", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Chapter Plus
        /// </summary>
        [JoinName("ChapPlus")]
        public JoinDataComplete ChapPlus = new JoinDataComplete(new JoinData { JoinNumber = 38, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Chapter Plus", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Chapter Minus
        /// </summary>
        [JoinName("ChapMinus")]
        public JoinDataComplete ChapMinus = new JoinDataComplete(new JoinData { JoinNumber = 39, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Chapter Minus", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Replay
        /// </summary>
        [JoinName("Replay")]
        public JoinDataComplete Replay = new JoinDataComplete(new JoinData { JoinNumber = 40, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Replay", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Record
        /// </summary>
        [JoinName("Record")]
        public JoinDataComplete Record = new JoinDataComplete(new JoinData { JoinNumber = 41, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Record", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Has Keypad Accessory Button 1
        /// </summary>
        [JoinName("HasKeypadAccessoryButton1")]
        public JoinDataComplete HasKeypadAccessoryButton1 = new JoinDataComplete(new JoinData { JoinNumber = 42, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Has Keypad Accessory Button 1", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Has Keypad Accessory Button 2
        /// </summary>
        [JoinName("HasKeypadAccessoryButton2")]
        public JoinDataComplete HasKeypadAccessoryButton2 = new JoinDataComplete(new JoinData { JoinNumber = 43, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Has Keypad Accessory Button 2", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Keypad Accessory Button 1 Press
        /// </summary>
        [JoinName("KeypadAccessoryButton1Press")]
        public JoinDataComplete KeypadAccessoryButton1Press = new JoinDataComplete(new JoinData { JoinNumber = 42, JoinSpan = 2 },
            new JoinMetadata { Description = "STB Keypad Accessory Button 1 Press", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Keypad Accessory Button 2 Press
        /// </summary>
        [JoinName("KeypadAccessoryButton2Press")]
        public JoinDataComplete KeypadAccessoryButton2Press = new JoinDataComplete(new JoinData { JoinNumber = 43, JoinSpan = 2 },
            new JoinMetadata { Description = "STB Keypad Accessory Button 2 Press", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Name
        /// </summary>
        [JoinName("Name")]
        public JoinDataComplete Name = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Keypad Accessory Button 1 Label
        /// </summary>
        [JoinName("KeypadAccessoryButton1Label")]
        public JoinDataComplete KeypadAccessoryButton1Label = new JoinDataComplete(new JoinData { JoinNumber = 42, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Keypad Accessory Button 1 Label", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// STB Keypad Accessory Button 2 Label
        /// </summary>
        [JoinName("KeypadAccessoryButton2Label")]
        public JoinDataComplete KeypadAccessoryButton2Label = new JoinDataComplete(new JoinData { JoinNumber = 43, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Keypad Accessory Button 1 Label", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// STB Load Presets
        /// </summary>
        [JoinName("LoadPresets")]
        public JoinDataComplete LoadPresets = new JoinDataComplete(new JoinData { JoinNumber = 50, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Load Presets", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// STB Has Presets
        /// </summary>
        [JoinName("HasPresets")]
        public JoinDataComplete HasPresets = new JoinDataComplete(new JoinData { JoinNumber = 50, JoinSpan = 1 },
            new JoinMetadata { Description = "STB Load Presets", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });


        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public SetTopBoxControllerJoinMap(uint joinStart)
            : this(joinStart, typeof(SetTopBoxControllerJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected SetTopBoxControllerJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}