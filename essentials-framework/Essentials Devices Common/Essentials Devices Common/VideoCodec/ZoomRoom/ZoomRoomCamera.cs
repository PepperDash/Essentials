using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
    public enum eZoomRoomCameraState
    {
        Start,
        Continue,
        Stop,
        RequestRemote,
        GiveupRemote,
        RequestedByFarEnd
    }

    public enum eZoomRoomCameraAction
    {
        Left,
        Right,
        Up,
        Down,
        In,
        Out
    }


    public class ZoomRoomCamera : CameraBase, IHasCameraPtzControl, IBridgeAdvanced
    {
        protected ZoomRoom ParentCodec { get; private set; }

        public int Id = 0;  // ID of near end selected camara is always 0

        private int ContinueTime = 10; // number of milliseconds between issuing continue commands

        private CTimer ContinueTimer;

        eZoomRoomCameraAction LastAction;

        private bool isPanning;

        private bool isTilting;

        private bool isZooming;

        //private bool isFocusing;

        private bool isMoving
        {
            get
            {
                return isPanning || isTilting || isZooming;

            }
        }

        public ZoomRoomCamera(string key, string name, ZoomRoom codec)
            : base(key, name)
        {
            ParentCodec = codec;
        }

        /// <summary>
        /// Builds the command and triggers the parent ZoomRoom to send it
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <param name="action"></param>
        void SendCommand(eZoomRoomCameraState state, eZoomRoomCameraAction action)
        {
            LastAction = action;
            ParentCodec.SendText(string.Format("zCommand Call CameraControl Id: {0} State: {1} Action: {2}", Id, state, action));
        }

        void StartContinueTimer()
        {
            if(ContinueTimer == null)
                ContinueTimer = new CTimer((o) => SendContinueAction(LastAction), ContinueTime);
        }

        void SendContinueAction(eZoomRoomCameraAction action)
        {
            SendCommand(eZoomRoomCameraState.Continue, action);
            ContinueTimer.Reset();
        }

        void StopContinueTimer()
        {
            if (ContinueTimer != null)
            {
                ContinueTimer.Stop();
                ContinueTimer.Dispose();
            }
        }

        #region IHasCameraPtzControl Members

        public void PositionHome()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasCameraPanControl Members

        public void PanLeft()
        {
            if (!isMoving)
            {
                SendCommand(eZoomRoomCameraState.Start, eZoomRoomCameraAction.Left);
                StartContinueTimer();
                isPanning = true;
            }
        }

        public void PanRight()
        {
            if (!isMoving)
            {
                SendCommand(eZoomRoomCameraState.Start, eZoomRoomCameraAction.Right);
                StartContinueTimer();
                isPanning = true;
            }
        }

        public void PanStop()
        {
            StopContinueTimer();
            SendCommand(eZoomRoomCameraState.Stop, LastAction);
            isPanning = false;
        }

        #endregion

        #region IHasCameraTiltControl Members

        public void TiltDown()
        {
            if (!isMoving)
            {
                SendCommand(eZoomRoomCameraState.Start, eZoomRoomCameraAction.Down);
                StartContinueTimer();
                isTilting = true;
            }
        }

        public void TiltUp()
        {
            if (!isMoving)
            {
                SendCommand(eZoomRoomCameraState.Start, eZoomRoomCameraAction.Up);
                StartContinueTimer();
                isTilting = true;
            }
        }

        public void TiltStop()
        {
            StopContinueTimer();
            SendCommand(eZoomRoomCameraState.Stop, LastAction);
            isTilting = false;
        }

        #endregion

        #region IHasCameraZoomControl Members

        public void ZoomIn()
        {
            if (!isMoving)
            {
                SendCommand(eZoomRoomCameraState.Start, eZoomRoomCameraAction.In);
                StartContinueTimer();
                isZooming = true;
            }
        }

        public void ZoomOut()
        {
            if (!isMoving)
            {
                SendCommand(eZoomRoomCameraState.Start, eZoomRoomCameraAction.Out);
                StartContinueTimer();
                isZooming = true;
            }
        }

        public void ZoomStop()
        {
            StopContinueTimer();
            SendCommand(eZoomRoomCameraState.Stop, LastAction);
            isZooming = false;
        }

        #endregion

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkCameraToApi(this, trilist, joinStart, joinMapKey, bridge);
        }
    }

    public class ZoomRoomFarEndCamera : ZoomRoomCamera, IAmFarEndCamera
    {

        public ZoomRoomFarEndCamera(string key, string name, ZoomRoom codec, int id)
            : base(key, name, codec)
        {
            Id = id;
        }

    }
}