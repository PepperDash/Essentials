using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;


namespace PepperDash.Essentials.AppServer
{
    public class SIMPLVtcJoinMap : JoinMapBase
    {
        public const string EndCall = "EndCall";
        public const string IncomingCall = "IncomingCall";
        public const string IncomingAnswer = "IncomingAnswer";
        public const string IncomingReject = "IncomingReject";
        public const string SpeedDialStart = "SpeedDialStart";
        public const string DirectorySearchBusy = "DirectorySearchBusy";
        public const string DirectoryLineSelected = "DirectoryLineSelected";
        public const string DirectoryEntryIsContact = "DirectoryEntryIsContact";
        public const string DirectoryIsRoot = "DirectoryIsRoot";
        public const string DDirectoryHasChanged = "DDirectoryHasChanged";
        public const string DirectoryRoot = "DirectoryRoot";
        public const string DirectoryFolderBack = "DirectoryFoldeBrack";
        public const string DirectoryDialSelectedLine = "DirectoryDialSelectedLine";

        public const string CameraTiltUp = "CameraTiltUp";
        public const string CameraTiltDown = "CameraTiltDown";
        public const string CameraPanLeft = "CameraPanLeft";
        public const string CameraPanRight = "CameraPanRight";
        public const string CameraZoomIn = "CameraZoomIn";
        public const string CameraZoomOut = "CameraZoomOut";
        public const string CameraPresetStart = "CameraPresetStart";
        public const string CameraModeAuto = "CameraModeAuto";
        public const string CameraModeManual = "CameraModeManual";
        public const string CameraModeOff = "CameraModeOff";

        public const string CameraSelfView = "CameraSelfView";
        public const string CameraLayout = "CameraLayout";

        public const string CameraSupportsAutoMode = "CameraSupportsAutoMode";
        public const string CameraSupportsOffMode = "CameraSupportsOffMode";

        public const string CameraNumberSelect = "CameraNumberSelect";
        public const string DirectorySelectRow = "DirectorySelectRow";
        public const string DirectoryRowCount = "DirectoryRowCount";

        public const string CurrentDialString = "CurrentDialString";
        public const string CurrentCallNumber = "CurrentCallNumber";
        public const string CurrentCallName = "CurrentCallName";
        public const string HookState = "HookState";
        public const string CallDirection = "CallDirection";
        public const string IncomingCallName = "IncomingCallName";
        public const string IncomingCallNumber = "IncomingCallNumber";
        public const string DirectorySearchString = "DirectorySearchString";
        public const string DirectoryEntriesStart = "EndCaDirectoryEntriesStartll";
        public const string DirectoryEntrySelectedName = "DirectoryEntrySelectedName";
        public const string DirectoryEntrySelectedNumber = "DirectoryEntrySelectedNumber";
        public const string DirectorySelectedFolderName = "DirectorySelectedFolderName";

        public const string Dtmf0 = "0";
        public const string Dtmf1 = "1";
        public const string Dtmf2 = "2";
        public const string Dtmf3 = "3";
        public const string Dtmf4 = "4";
        public const string Dtmf5 = "5";
        public const string Dtmf6 = "6";
        public const string Dtmf7 = "7";
        public const string Dtmf8 = "8";
        public const string Dtmf9 = "9";
        public const string DtmfStar = "*";
        public const string DtmfPound = "#";
    

        public SIMPLVtcJoinMap()
        {
            // TODO: Set Join metedata

            Joins.Add(EndCall, new JoinMetadata() { JoinNumber = 24, Label = "Hang Up", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(IncomingCall, new JoinMetadata() { JoinNumber = 50, Label = "Incoming Call FB", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(IncomingAnswer, new JoinMetadata() { JoinNumber = 51, Label = "Answer Incoming Call", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(IncomingReject, new JoinMetadata() { JoinNumber = 52, Label = "Reject Incoming Call", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(SpeedDialStart, new JoinMetadata() { JoinNumber = 41, Label = "Speed Dial", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 4, JoinType = eJoinType.Digital });

            Joins.Add(DirectorySearchBusy, new JoinMetadata() { JoinNumber = 100, Label = "Directory Search Busy FB", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(DirectoryLineSelected, new JoinMetadata() { JoinNumber = 101, Label = "Directory Line Selected FB", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(DirectoryEntryIsContact, new JoinMetadata() { JoinNumber = 101, Label = "Directory Selected Entry Is Contact FB", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(DirectoryIsRoot, new JoinMetadata() { JoinNumber = 102, Label = "Directory is on Root FB", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(DDirectoryHasChanged, new JoinMetadata() { JoinNumber = 103, Label = "Directory has changed FB", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });

            
            Joins.Add(CurrentDialString, new JoinMetadata() { JoinNumber = 1, Label = "Current Dial String", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(CurrentCallNumber, new JoinMetadata() { JoinNumber = 11, Label = "Current Call Number", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(CurrentCallName, new JoinMetadata() { JoinNumber = 12, Label = "Current Call Name", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(HookState, new JoinMetadata() { JoinNumber = 21, Label = "Current Hook State", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(CallDirection, new JoinMetadata() { JoinNumber = 21, Label = "Current Call Direction", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(Dtmf1, new JoinMetadata() { JoinNumber = 1, Label = "DTMF 1", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(Dtmf2, new JoinMetadata() { JoinNumber = 2, Label = "DTMF 2", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(Dtmf3, new JoinMetadata() { JoinNumber = 3, Label = "DTMF 3", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(Dtmf4, new JoinMetadata() { JoinNumber = 4, Label = "DTMF 4", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(Dtmf5, new JoinMetadata() { JoinNumber = 5, Label = "DTMF 5", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(Dtmf6, new JoinMetadata() { JoinNumber = 6, Label = "DTMF 6", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(Dtmf7, new JoinMetadata() { JoinNumber = 7, Label = "DTMF 7", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(Dtmf8, new JoinMetadata() { JoinNumber = 8, Label = "DTMF 8", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(Dtmf9, new JoinMetadata() { JoinNumber = 9, Label = "DTMF 9", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(Dtmf0, new JoinMetadata() { JoinNumber = 10, Label = "DTMF 0", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(DtmfStar, new JoinMetadata() { JoinNumber = 11, Label = "DTMF *", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(DtmfPound, new JoinMetadata() { JoinNumber = 12, Label = "DTMF #", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });

        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            foreach (var join in Joins)
            {
                join.Value.JoinNumber = join.Value.JoinNumber + joinOffset;
            }

            PrintJoinMapInfo();
        }
    }
}