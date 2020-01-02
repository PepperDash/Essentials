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
            Joins.Add(DirectoryRoot, new JoinMetadata() { JoinNumber = 104, Label = "Go to Directory Root", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(DirectoryFolderBack, new JoinMetadata() { JoinNumber = 105, Label = "Go back one directory level", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(DirectoryDialSelectedLine, new JoinMetadata() { JoinNumber = 106, Label = "Dial selected directory line", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });

            Joins.Add(CameraTiltUp, new JoinMetadata() { JoinNumber = 111, Label = "Camera Tilt Up", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(CameraTiltDown, new JoinMetadata() { JoinNumber = 112, Label = "Camera Tilt Down", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(CameraPanLeft, new JoinMetadata() { JoinNumber = 113, Label = "Camera Pan Left", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(CameraPanRight, new JoinMetadata() { JoinNumber = 114, Label = "Camera Pan Right", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(CameraZoomIn, new JoinMetadata() { JoinNumber = 115, Label = "Camera Zoom In", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(CameraZoomOut, new JoinMetadata() { JoinNumber = 116, Label = "Camera Zoom Out", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(CameraPresetStart, new JoinMetadata() { JoinNumber = 121, Label = "Camera Presets", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 5, JoinType = eJoinType.Digital });
            Joins.Add(CameraModeAuto, new JoinMetadata() { JoinNumber = 131, Label = "Camera Mode Auto", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(CameraModeManual, new JoinMetadata() { JoinNumber = 132, Label = "Camera Mode Manual", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(CameraModeOff, new JoinMetadata() { JoinNumber = 133, Label = "Camera Mode Off", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });

            Joins.Add(CameraSelfView, new JoinMetadata() { JoinNumber = 141, Label = "Camera Self View Toggle/FB", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(CameraLayout, new JoinMetadata() { JoinNumber = 142, Label = "Camera Layout Toggle", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });

            Joins.Add(CameraSupportsAutoMode, new JoinMetadata() { JoinNumber = 143, Label = "Camera Supports Auto Mode FB", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });
            Joins.Add(CameraSupportsOffMode, new JoinMetadata() { JoinNumber = 144, Label = "Camera Supports Off Mode FB", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Digital });


            Joins.Add(CameraNumberSelect, new JoinMetadata() { JoinNumber = 60, Label = "Camera Number Select/FB", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Analog });
            Joins.Add(DirectorySelectRow, new JoinMetadata() { JoinNumber = 101, Label = "Directory Select Row/FB", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Analog });
            Joins.Add(DirectoryRowCount, new JoinMetadata() { JoinNumber = 101, Label = "Directory Row Count FB", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Analog });
            
            Joins.Add(CurrentDialString, new JoinMetadata() { JoinNumber = 1, Label = "Current Dial String", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(CurrentCallNumber, new JoinMetadata() { JoinNumber = 3, Label = "Current Call Number", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(CurrentCallName, new JoinMetadata() { JoinNumber = 2, Label = "Current Call Name", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(HookState, new JoinMetadata() { JoinNumber = 31, Label = "Current Hook State", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(CallDirection, new JoinMetadata() { JoinNumber = 22, Label = "Current Call Direction", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(IncomingCallName, new JoinMetadata() { JoinNumber = 51, Label = "Incoming Call Name", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(IncomingCallNumber, new JoinMetadata() { JoinNumber = 52, Label = "Incoming Call Number", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });

            Joins.Add(DirectorySearchString, new JoinMetadata() { JoinNumber = 52, Label = "Directory Search String", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(DirectoryEntriesStart, new JoinMetadata() { JoinNumber = 52, Label = "Directory Entries", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 255, JoinType = eJoinType.Serial });
            Joins.Add(DirectoryEntrySelectedName, new JoinMetadata() { JoinNumber = 52, Label = "Selected Directory Entry Name", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(DirectoryEntrySelectedNumber, new JoinMetadata() { JoinNumber = 52, Label = "Selected Directory Entry Number", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });
            Joins.Add(DirectorySelectedFolderName, new JoinMetadata() { JoinNumber = 52, Label = "Selected Directory Folder Name", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinSpan = 1, JoinType = eJoinType.Serial });

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