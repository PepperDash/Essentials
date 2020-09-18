using System;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_Core.Bridges.JoinMaps
{
    public class VideoCodecControllerJoinMap : JoinMapBaseAdvanced
    {
        #region Status

        [JoinName("IsOnline")] public JoinDataComplete IsOnline =
            new JoinDataComplete(new JoinData {JoinNumber = 1, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Device is Online",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Digital
                });

        #endregion

        /*#region NearEndCameraControls

        [JoinName("NearCamDown")] public JoinDataComplete NearEndCameraDown =
            new JoinDataComplete(new JoinData {JoinNumber = 12, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Near Camera Down",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("NearCamFocusFar")] public JoinDataComplete NearEndCameraFocusFar =
            new JoinDataComplete(new JoinData {JoinNumber = 18, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Near Camera Focus Far",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("NearCamFocusNear")] public JoinDataComplete NearEndCameraFocusNear =
            new JoinDataComplete(new JoinData {JoinNumber = 17, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Near Camera Focus Near",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("NearCamLeft")] public JoinDataComplete NearEndCameraLeft =
            new JoinDataComplete(new JoinData {JoinNumber = 13, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Near Camera Left",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("NearCamPresetNames")] public JoinDataComplete NearEndCameraNames =
            new JoinDataComplete(new JoinData {JoinNumber = 11, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "XSig - Camera Names",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("NearCamPresetSave")] public JoinDataComplete NearEndCameraPresetSave =
            new JoinDataComplete(new JoinData {JoinNumber = 21, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Save Current Preset",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("NearCamPresetSelect")] public JoinDataComplete NearEndCameraPresetSelect =
            new JoinDataComplete(new JoinData {JoinNumber = 31, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Recall Preset",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Analog
                });

        [JoinName("NearCamRight")] public JoinDataComplete NearEndCameraRight =
            new JoinDataComplete(new JoinData {JoinNumber = 14, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Near Camera Right",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("NearCamSelectAnalog")] public JoinDataComplete NearEndCameraSelectAnalog = new JoinDataComplete(
            new JoinData {JoinNumber = 12, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Camera Select & Feedback by index",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Analog
            });

        [JoinName("NearCamSelectSerial")] public JoinDataComplete NearEndCameraSelectSerial = new JoinDataComplete(
            new JoinData {JoinNumber = 12, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Camera Select & Feedback by name",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Serial
            });

        [JoinName("NearCamUp")] public JoinDataComplete NearEndCameraUp =
            new JoinDataComplete(new JoinData {JoinNumber = 11, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Near Camera Up",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("NearCamZoomIn")] public JoinDataComplete NearEndCameraZoomIn =
            new JoinDataComplete(new JoinData {JoinNumber = 15, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Near Camera Zoom In",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("NearCamZoomOut")] public JoinDataComplete NearEndCameraZoomOut =
            new JoinDataComplete(new JoinData {JoinNumber = 16, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Near Camera Zoom Out",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        #endregion

        #region Far End Camera Controls

        [JoinName("FarCamDown")] public JoinDataComplete FarEndCameraDown =
            new JoinDataComplete(new JoinData {JoinNumber = 32, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Far Camera Down",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("FarCamFocusFar")] public JoinDataComplete FarEndCameraFocusFar =
            new JoinDataComplete(new JoinData {JoinNumber = 38, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Far Camera Focus Far",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("FarCamFocusNear")] public JoinDataComplete FarEndCameraFocusNear =
            new JoinDataComplete(new JoinData {JoinNumber = 37, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Far Camera Focus Near",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("FarCamLeft")] public JoinDataComplete FarEndCameraLeft =
            new JoinDataComplete(new JoinData {JoinNumber = 33, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Far Camera Left",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("FarCamPresetSelect")] public JoinDataComplete FarEndCameraPresetSelect =
            new JoinDataComplete(new JoinData {JoinNumber = 41, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Recall Preset",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Analog
                });

        [JoinName("FarCamRight")] public JoinDataComplete FarEndCameraRight =
            new JoinDataComplete(new JoinData {JoinNumber = 34, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Far Camera Right",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("FarCamUp")] public JoinDataComplete FarEndCameraUp =
            new JoinDataComplete(new JoinData {JoinNumber = 31, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Far Camera Up",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("FarCamZoomIn")] public JoinDataComplete FarEndCameraZoomIn =
            new JoinDataComplete(new JoinData {JoinNumber = 35, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Far Camera Zoom In",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("FarCamZoomOut")] public JoinDataComplete FarEndCameraZoomOut =
            new JoinDataComplete(new JoinData {JoinNumber = 36, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Far Camera Zoom Out",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        #endregion

        #region Camera Tracking Controls

        [JoinName("CameraTrackingOff")] public JoinDataComplete CameraTrackingOff =
            new JoinDataComplete(new JoinData {JoinNumber = 52, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Tracking Off",
                    JoinType = eJoinType.Digital,
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL
                });

        [JoinName("CameraTrackingOn")] public JoinDataComplete CameraTrackingOn =
            new JoinDataComplete(new JoinData {JoinNumber = 51, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Tracking On",
                    JoinType = eJoinType.Digital,
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL
                });

        [JoinName("CameraTrackingToggle")] public JoinDataComplete CameraTrackingToggle = new JoinDataComplete(
            new JoinData {JoinNumber = 53, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Camera Tracking Toggle",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        #endregion

        #region Incoming Call

        [JoinName("IncomingCallAnswer")] public JoinDataComplete IncomingCallAnswer =
            new JoinDataComplete(new JoinData {JoinNumber = 61, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Answer Incoming Call & Incoming Call FB",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("IncomingCallId")] public JoinDataComplete IncomingCallId =
            new JoinDataComplete(new JoinData {JoinNumber = 64, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Incoming Call ID",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("IncomingCallName")] public JoinDataComplete IncomingCallName =
            new JoinDataComplete(new JoinData {JoinNumber = 61, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Incoming Call Name",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("IncomingCallNumber")] public JoinDataComplete IncomingCallNumber =
            new JoinDataComplete(new JoinData {JoinNumber = 62, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Incoming Call Number",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("IncomingCallReject")] public JoinDataComplete IncomingCallReject =
            new JoinDataComplete(new JoinData {JoinNumber = 62, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Reject Incoming Call",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("IncomingCallType")] public JoinDataComplete IncomingCallType =
            new JoinDataComplete(new JoinData {JoinNumber = 63, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Incoming Call Type",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        #endregion

        #region Manual Dial
        [JoinName("ManualDial")]
        public JoinDataComplete ManualDial = new JoinDataComplete(new JoinData {JoinNumber = 71, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Dial Entered String",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("HangUpAllCalls")]
        public JoinDataComplete HangUpAllCalls = new JoinDataComplete(new JoinData {JoinNumber = 72, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Hang up All calls",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });
        [JoinName("ManualDialString")]
        public JoinDataComplete ManualDialString = new JoinDataComplete(new JoinData {JoinNumber = 71, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Manual String to dial",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        #endregion

        #region Connected Calls

        [JoinName("ConnectedCallCount")]
        public JoinDataComplete ConnectedCallCount = new JoinDataComplete(new JoinData {JoinNumber = 81, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Connected Call Count",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });

        [JoinName("ConnectedCallHangup")]
        public JoinDataComplete ConnectedCallHangupStart =
            new JoinDataComplete(new JoinData {JoinNumber = 81, JoinSpan = 8},
                new JoinMetadata
                {
                    Description = "Hang up selected call",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("ConnectedCallData")] public JoinDataComplete ConnectedCallData =
            new JoinDataComplete(new JoinData {JoinNumber = 81, JoinSpan = 1},
                new JoinMetadata
                {
                    Description =
                        "XSig - Connected Call info\r\nDigital 1: Connecting\r\nDigital 2: Connected\r\nSerial 1: Name\r\nSerial 2: Number\r\nSerial 3: Status\r\nSerial 4: Type\r\nSerial 5: ID",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        #endregion
        #region NearEnd Source Select

        [JoinName("NearEndSourceSelect")] public JoinDataComplete NearEndSourceSelect =
            new JoinDataComplete(new JoinData {JoinNumber = 5, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Near End Source Select",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Analog,
                    ValidValues = new[] {"1", "2", "3", "4"}
                });
        #endregion

        #region Sharing 
        //Putting share start/stop only in this base join map. The map can be extended for features of other codecs

        public JoinDataComplete SharingStart = new JoinDataComplete(new JoinData {JoinNumber = 101, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Start sharing & feedback",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });

        public JoinDataComplete SharingStop = new JoinDataComplete(new JoinData {JoinNumber = 102, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Stop Sharing & Feedback",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Digital
            });
        #endregion

        #region Phonebook

        #endregion*/

        [JoinName("CallDirection")] public JoinDataComplete CallDirection =
            new JoinDataComplete(new JoinData {JoinNumber = 22, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Current Call Direction",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("CameraLayout")] public JoinDataComplete CameraLayout =
            new JoinDataComplete(new JoinData {JoinNumber = 142, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Layout Toggle",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CameraModeAuto")] public JoinDataComplete CameraModeAuto =
            new JoinDataComplete(new JoinData {JoinNumber = 131, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Mode Auto",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CameraModeManual")] public JoinDataComplete CameraModeManual =
            new JoinDataComplete(new JoinData {JoinNumber = 132, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Mode Manual",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CameraModeOff")] public JoinDataComplete CameraModeOff =
            new JoinDataComplete(new JoinData {JoinNumber = 133, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Mode Off",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CameraNumberSelect")] public JoinDataComplete CameraNumberSelect =
            new JoinDataComplete(new JoinData {JoinNumber = 60, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Number Select/FB",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Analog
                });

        [JoinName("CameraPanLeft")] public JoinDataComplete CameraPanLeft =
            new JoinDataComplete(new JoinData {JoinNumber = 113, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Pan Left",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CameraPanRight")] public JoinDataComplete CameraPanRight =
            new JoinDataComplete(new JoinData {JoinNumber = 114, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Pan Right",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CameraPresetSelect")] public JoinDataComplete CameraPresetSelect =
            new JoinDataComplete(new JoinData {JoinNumber = 121, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Preset Select",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Analog
                });

        [JoinName("CameraPresetNames")] public JoinDataComplete CameraPresetNames =
            new JoinDataComplete(new JoinData {JoinNumber = 121, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Preset Names - XSIG, max of 15",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("CameraSelfView")] public JoinDataComplete CameraSelfView =
            new JoinDataComplete(new JoinData {JoinNumber = 141, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Self View Toggle/FB",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CameraSupportsAutoMode")] public JoinDataComplete CameraSupportsAutoMode =
            new JoinDataComplete(new JoinData {JoinNumber = 143, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Supports Auto Mode FB",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CameraSupportsOffMode")] public JoinDataComplete CameraSupportsOffMode =
            new JoinDataComplete(new JoinData {JoinNumber = 144, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Supports Off Mode FB",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CameraTiltDown")] public JoinDataComplete CameraTiltDown =
            new JoinDataComplete(new JoinData {JoinNumber = 112, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Tilt Down",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CameraTiltUp")] public JoinDataComplete CameraTiltUp =
            new JoinDataComplete(new JoinData {JoinNumber = 111, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Tilt Up",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CameraZoomIn")] public JoinDataComplete CameraZoomIn =
            new JoinDataComplete(new JoinData {JoinNumber = 115, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Zoom In",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CameraZoomOut")] public JoinDataComplete CameraZoomOut =
            new JoinDataComplete(new JoinData {JoinNumber = 116, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Camera Zoom Out",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("CurrentCallName")] public JoinDataComplete CurrentCallData =
            new JoinDataComplete(new JoinData {JoinNumber = 2, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Current Call Data - XSIG",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });
        /*
        [JoinName("CurrentCallNumber")] public JoinDataComplete CurrentCallNumber =
            new JoinDataComplete(new JoinData {JoinNumber = 3, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Current Call Number",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });*/

        [JoinName("CurrentDialString")] public JoinDataComplete CurrentDialString =
            new JoinDataComplete(new JoinData {JoinNumber = 1, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Current Dial String",
                    JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("DirectoryHasChanged")] public JoinDataComplete DirectoryHasChanged =
            new JoinDataComplete(new JoinData {JoinNumber = 103, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Directory has changed FB",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("DirectoryDialSelectedLine")] public JoinDataComplete DirectoryDialSelectedLine =
            new JoinDataComplete(new JoinData {JoinNumber = 106, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Dial selected directory line",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("DirectoryEntries")] public JoinDataComplete DirectoryEntries =
            new JoinDataComplete(new JoinData {JoinNumber = 101, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Directory Entries - XSig, 255 entries",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("DirectoryEntryIsContact")] public JoinDataComplete DirectoryEntryIsContact =
            new JoinDataComplete(new JoinData {JoinNumber = 101, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Directory Selected Entry Is Contact FB",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("DirectoryEntrySelectedName")] public JoinDataComplete DirectoryEntrySelectedName =
            new JoinDataComplete(new JoinData {JoinNumber = 356, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Selected Directory Entry Name",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("DirectoryEntrySelectedNumber")] public JoinDataComplete DirectoryEntrySelectedNumber =
            new JoinDataComplete(new JoinData {JoinNumber = 357, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Selected Directory Entry Number",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("DirectoryFolderBack")] public JoinDataComplete DirectoryFolderBack =
            new JoinDataComplete(new JoinData {JoinNumber = 105, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Go back one directory level",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("DirectoryIsRoot")] public JoinDataComplete DirectoryIsRoot =
            new JoinDataComplete(new JoinData {JoinNumber = 102, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Directory is on Root FB",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("DirectoryLineSelected")] public JoinDataComplete DirectoryLineSelected =
            new JoinDataComplete(new JoinData {JoinNumber = 101, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Directory Line Selected FB",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("DirectoryRoot")] public JoinDataComplete DirectoryRoot =
            new JoinDataComplete(new JoinData {JoinNumber = 104, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Go to Directory Root",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("DirectoryRowCount")] public JoinDataComplete DirectoryRowCount =
            new JoinDataComplete(new JoinData {JoinNumber = 101, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Directory Row Count FB",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Analog
                });

        [JoinName("DirectorySearchBusy")] public JoinDataComplete DirectorySearchBusy =
            new JoinDataComplete(new JoinData {JoinNumber = 100, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Directory Search Busy FB",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("DirectorySearchString")] public JoinDataComplete DirectorySearchString =
            new JoinDataComplete(new JoinData {JoinNumber = 100, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Directory Search String",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("DirectorySelectRow")] public JoinDataComplete DirectorySelectRow =
            new JoinDataComplete(new JoinData {JoinNumber = 101, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Directory Select Row",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Analog
                });

        [JoinName("DirectorySelectedFolderName")] public JoinDataComplete DirectorySelectedFolderName =
            new JoinDataComplete(new JoinData {JoinNumber = 358, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Selected Directory Folder Name",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("0")] public JoinDataComplete Dtmf0 =
            new JoinDataComplete(new JoinData {JoinNumber = 20, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "DTMF 0",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("1")] public JoinDataComplete Dtmf1 =
            new JoinDataComplete(new JoinData {JoinNumber = 11, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "DTMF 1",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("2")] public JoinDataComplete Dtmf2 =
            new JoinDataComplete(new JoinData {JoinNumber = 12, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "DTMF 2",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("3")] public JoinDataComplete Dtmf3 =
            new JoinDataComplete(new JoinData {JoinNumber = 13, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "DTMF 3",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("4")] public JoinDataComplete Dtmf4 =
            new JoinDataComplete(new JoinData {JoinNumber = 14, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "DTMF 4",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("5")] public JoinDataComplete Dtmf5 =
            new JoinDataComplete(new JoinData {JoinNumber = 15, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "DTMF 5",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("6")] public JoinDataComplete Dtmf6 =
            new JoinDataComplete(new JoinData {JoinNumber = 16, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "DTMF 6",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("7")] public JoinDataComplete Dtmf7 =
            new JoinDataComplete(new JoinData {JoinNumber = 17, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "DTMF 7",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("8")] public JoinDataComplete Dtmf8 =
            new JoinDataComplete(new JoinData {JoinNumber = 18, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "DTMF 8",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("9")] public JoinDataComplete Dtmf9 =
            new JoinDataComplete(new JoinData {JoinNumber = 19, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "DTMF 9",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("#")] public JoinDataComplete DtmfPound =
            new JoinDataComplete(new JoinData {JoinNumber = 22, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "DTMF #",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("*")] public JoinDataComplete DtmfStar =
            new JoinDataComplete(new JoinData {JoinNumber = 21, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "DTMF *",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("EndCall")] public JoinDataComplete EndCall =
            new JoinDataComplete(new JoinData {JoinNumber = 24, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Hang Up",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("HookState")] public JoinDataComplete HookState =
            new JoinDataComplete(new JoinData {JoinNumber = 31, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Current Hook State",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("IncomingAnswer")] public JoinDataComplete IncomingAnswer =
            new JoinDataComplete(new JoinData {JoinNumber = 51, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Answer Incoming Call",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("IncomingCall")] public JoinDataComplete IncomingCall =
            new JoinDataComplete(new JoinData {JoinNumber = 50, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Incoming Call",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("IncomingCallName")] public JoinDataComplete IncomingCallName =
            new JoinDataComplete(new JoinData {JoinNumber = 51, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Incoming Call Name",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("IncomingCallNumber")] public JoinDataComplete IncomingCallNumber =
            new JoinDataComplete(new JoinData {JoinNumber = 52, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Incoming Call Number",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("IncomingReject")] public JoinDataComplete IncomingReject =
            new JoinDataComplete(new JoinData {JoinNumber = 52, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Reject Incoming Call",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("SpeedDialStart")] public JoinDataComplete SpeedDialStart =
            new JoinDataComplete(new JoinData {JoinNumber = 41, JoinSpan = 4},
                new JoinMetadata
                {
                    Description = "Speed Dial",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Digital
                });

        [JoinName("Schedule")] public JoinDataComplete Schedule =
            new JoinDataComplete(new JoinData {JoinNumber = 102, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Schedule Data - XSIG",
                    JoinCapabilities = eJoinCapabilities.ToSIMPL,
                    JoinType = eJoinType.Serial
                });

        [JoinName("CameraLayoutStringFb")]
        public JoinDataComplete CameraLayoutStringFb =
            new JoinDataComplete(new JoinData { JoinNumber = 141, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Current Layout Fb",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });

        [JoinName("ManualDial")] public JoinDataComplete ManualDial =
            new JoinDataComplete(new JoinData {JoinNumber = 71, JoinSpan = 1},
                new JoinMetadata
                {
                    Description = "Dial manual string",
                    JoinCapabilities = eJoinCapabilities.FromSIMPL,
                    JoinType = eJoinType.Digital
                });

        public VideoCodecControllerJoinMap(uint joinStart) : base(joinStart, typeof (VideoCodecControllerJoinMap))
        {
        }

        public VideoCodecControllerJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }
}