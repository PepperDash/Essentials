using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using Crestron.SimplSharp.Reflection;


namespace PepperDash.Essentials.Bridges
{
    public class SetTopBoxControllerJoinMap : JoinMapBase
    {
        #region Digitals
        public uint DvrList { get; set; } //
        public uint Replay { get; set; }
        public uint Up { get; set; } //
        public uint Down { get; set; } //
        public uint Left { get; set; } //
        public uint Right { get; set; } //
        public uint Select { get; set; } //
        public uint Menu { get; set; } //
        public uint Exit { get; set; } //
        public uint Digit0 { get; set; } //
        public uint Digit1 { get; set; } // 
        public uint Digit2 { get; set; } //
        public uint Digit3 { get; set; } //
        public uint Digit4 { get; set; } // 
        public uint Digit5 { get; set; } //
        public uint Digit6 { get; set; } //
        public uint Digit7 { get; set; } //
        public uint Digit8 { get; set; } //
        public uint Digit9 { get; set; } //
        public uint Dash { get; set; } //
        public uint KeypadEnter { get; set; } //
        public uint ChannelUp { get; set; } //
        public uint ChannelDown { get; set; } //
        public uint LastChannel { get; set; } //
        public uint Guide { get; set; } //
        public uint Info { get; set; } //
        public uint Red { get; set; } //
        public uint Green { get; set; } //
        public uint Yellow { get; set; } //
        public uint Blue { get; set; } //
        public uint ChapMinus { get; set; }
        public uint ChapPlus { get; set; }
        public uint FFwd { get; set; } //
        public uint Pause { get; set; } //
        public uint Play { get; set; } //
        public uint Record { get; set; }
        public uint Rewind { get; set; } //
        public uint Stop { get; set; } //

        public uint PowerOn { get; set; } //
        public uint PowerOff { get; set; } //
        public uint PowerToggle { get; set; } //

        public uint HasKeypadAccessoryButton1 { get; set; }
        public uint HasKeypadAccessoryButton2 { get; set; }

        public uint KeypadAccessoryButton1Press { get; set; }
        public uint KeypadAccessoryButton2Press { get; set; }


        public uint HasDvr { get; set; }
        public uint HasPresets { get; set; }
        public uint HasNumeric { get; set; }
        public uint HasDpad { get; set; }


        #endregion

        #region Analogs

        #endregion

        #region Strings
        public uint Name { get; set; }
        public uint LoadPresets { get; set; }
        public uint KeypadAccessoryButton1Label { get; set; }
        public uint KeypadAccessoryButton2Label { get; set; }

        #endregion

        public SetTopBoxControllerJoinMap()
        {
            PowerOn = 1;
            PowerOff = 2;
            PowerToggle = 3;

            HasDpad = 4;
            Up = 4;
            Down = 5;
            Left = 6;
            Right = 7;
            Select = 8;
            Menu = 9;
            Exit = 10;

            HasNumeric = 11;
            Digit0 = 11;
            Digit1 = 12;
            Digit2 = 13;
            Digit3 = 14;
            Digit4 = 15;
            Digit5 = 16;
            Digit6 = 17;
            Digit7 = 18;
            Digit8 = 19;
            Digit9 = 20;
            Dash = 21;
            KeypadEnter = 22;
            ChannelUp = 23;
            ChannelDown = 24;
            LastChannel = 25;

            Guide = 26;
            Info = 27;
            Red = 28;
            Green = 29;
            Yellow = 30;
            Blue = 31;

            HasDvr = 32;
            DvrList = 32;
            Play = 33;
            Pause = 34;
            Stop = 35;
            FFwd = 36;
            Rewind = 37;
            ChapPlus = 38;
            ChapMinus = 39;
            Replay = 40;
            Record = 41;
            HasKeypadAccessoryButton1 = 42;
            KeypadAccessoryButton1Press = 42;
            HasKeypadAccessoryButton2 = 43;
            KeypadAccessoryButton2Press = 43;

            Name = 1;
            KeypadAccessoryButton1Label = 42;
            KeypadAccessoryButton2Label = 43;

            LoadPresets = 50;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            PowerOn += joinOffset;
            PowerOff += joinOffset;
            PowerToggle += joinOffset;

            HasDpad += joinOffset;
            Up += joinOffset;
            Down += joinOffset;
            Left += joinOffset;
            Right += joinOffset;
            Select += joinOffset;
            Menu += joinOffset;
            Exit += joinOffset;

            HasNumeric += joinOffset;
            Digit0 += joinOffset;
            Digit1 += joinOffset;
            Digit2 += joinOffset;
            Digit3 += joinOffset;
            Digit4 += joinOffset;
            Digit5 += joinOffset;
            Digit6 += joinOffset;
            Digit7 += joinOffset;
            Digit8 += joinOffset;
            Digit9 += joinOffset;
            Dash += joinOffset;
            KeypadEnter += joinOffset;
            ChannelUp += joinOffset;
            ChannelDown += joinOffset;
            LastChannel += joinOffset;

            Guide += joinOffset;
            Info += joinOffset;
            Red += joinOffset;
            Green += joinOffset;
            Yellow += joinOffset;
            Blue += joinOffset;

            HasDvr += joinOffset;
            DvrList += joinOffset;
            Play += joinOffset;
            Pause += joinOffset;
            Stop += joinOffset;
            FFwd += joinOffset;
            Rewind += joinOffset;
            ChapPlus += joinOffset;
            ChapMinus += joinOffset;
            Replay += joinOffset;
            Record += joinOffset;
            HasKeypadAccessoryButton1 += joinOffset;
            KeypadAccessoryButton1Press += joinOffset;
            HasKeypadAccessoryButton2 += joinOffset;
            KeypadAccessoryButton2Press += joinOffset;

            Name += joinOffset;
            KeypadAccessoryButton1Label += joinOffset;
            KeypadAccessoryButton2Label += joinOffset;

            LoadPresets += joinOffset;
        }

    }
}