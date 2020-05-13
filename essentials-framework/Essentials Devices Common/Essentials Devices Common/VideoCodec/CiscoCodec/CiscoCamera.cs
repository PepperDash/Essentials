using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Devices.Common.Cameras;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Cisco
{
    public class CiscoFarEndCamera : CameraBase, IHasCameraPtzControl, IAmFarEndCamera, IBridgeAdvanced
    {
        protected CiscoSparkCodec ParentCodec { get; private set; }

        protected string CallId {
            get
            {
                return (ParentCodec as CiscoSparkCodec).GetCallId();
            }
        }

        public CiscoFarEndCamera(string key, string name, CiscoSparkCodec codec)
            : base(key, name)
        {
            Capabilities = eCameraCapabilities.Pan | eCameraCapabilities.Tilt | eCameraCapabilities.Zoom;

            ParentCodec = codec;
        }

        #region IHasCameraPtzControl Members

        public void PositionHome()
        {
            // Not supported on far end camera
        }

        #endregion

        #region IHasCameraPanControl Members

        public void PanLeft()
        {
            ParentCodec.SendText(string.Format("xCommand Call FarEndControl Camera Move Value: Left CallId: {0}", CallId));           
        }

        public void PanRight()
        {
            ParentCodec.SendText(string.Format("xCommand Call FarEndControl Camera Move Value: Right CallId: {0}", CallId)); 
        }

        public void PanStop()
        {
            Stop();
        }

        #endregion

        #region IHasCameraTiltControl Members

        public void TiltDown()
        {
            ParentCodec.SendText(string.Format("xCommand Call FarEndControl Camera Move Value: Down CallId: {0}", CallId)); 
        }

        public void TiltUp()
        {
            ParentCodec.SendText(string.Format("xCommand Call FarEndControl Camera Move Value: Up CallId: {0}", CallId)); 
        }

        public void TiltStop()
        {
            Stop();
        }

        #endregion

        #region IHasCameraZoomControl Members

        public void ZoomIn()
        {
            ParentCodec.SendText(string.Format("xCommand Call FarEndControl Camera Move Value: ZoomIn CallId: {0}", CallId)); 
        }

        public void ZoomOut()
        {
            ParentCodec.SendText(string.Format("xCommand Call FarEndControl Camera Move Value: ZoomOut CallId: {0}", CallId)); 
        }

        public void ZoomStop()
        {
            Stop();
        }

        #endregion


        void Stop()
        {
            ParentCodec.SendText(string.Format("xCommand Call FarEndControl Camera Stop CallId: {0}", CallId)); 
        }

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkCameraToApi(this, trilist, joinStart, joinMapKey, bridge);
        }
    }

    public class CiscoSparkCamera : CameraBase, IHasCameraPtzControl, IHasCameraFocusControl, IBridgeAdvanced
    {
        /// <summary>
        /// The codec this camera belongs to
        /// </summary>
        protected CiscoSparkCodec ParentCodec { get; private set; }

        /// <summary>
        /// The ID of the camera on the codec
        /// </summary>
        protected uint CameraId { get; private set; }

        /// <summary>
        /// Valid range 1-15
        /// </summary>
        protected uint PanSpeed { get; private set; }

        /// <summary>
        /// Valid range 1-15
        /// </summary>
        protected uint TiltSpeed { get; private set; }

        /// <summary>
        /// Valid range 1-15
        /// </summary>
        protected uint ZoomSpeed { get; private set; }

        private bool isPanning;

        private bool isTilting;

        private bool isZooming;

        private bool isFocusing;

        private bool isMoving
        {
            get
            {
                return isPanning || isTilting || isZooming || isFocusing;

            }
        }

        public CiscoSparkCamera(string key, string name, CiscoSparkCodec codec, uint id)
            : base(key, name)
        {
            // Default to all capabilties
            Capabilities = eCameraCapabilities.Pan | eCameraCapabilities.Tilt | eCameraCapabilities.Zoom | eCameraCapabilities.Focus; 

            ParentCodec = codec;

            CameraId = id;

            // Set default speeds
            PanSpeed = 7;
            TiltSpeed = 7;
            ZoomSpeed = 7;
        }


        //  Takes a string from the camera capabilities value and converts from "ptzf" to enum bitmask
        public void SetCapabilites(string capabilites)
        {
            var c = capabilites.ToLower();

            if (c.Contains("p"))
                Capabilities = Capabilities | eCameraCapabilities.Pan;

            if (c.Contains("t"))
                Capabilities = Capabilities | eCameraCapabilities.Tilt;

            if (c.Contains("z"))
                Capabilities = Capabilities | eCameraCapabilities.Zoom;

            if (c.Contains("f"))
                Capabilities = Capabilities | eCameraCapabilities.Focus;
        }

        #region IHasCameraPtzControl Members

        public void PositionHome()
        {
            // Not supported on Internal Spark Camera

           
        }

        #endregion

        #region IHasCameraPanControl Members

        public void PanLeft()
        {
            if (!isMoving)
            {
                ParentCodec.SendText(string.Format("xCommand Camera Ramp CameraId: {0} Pan: Left PanSpeed: {1}", CameraId, PanSpeed));
                isPanning = true;
            }
        }

        public void PanRight()
        {
            if (!isMoving)
            {
                ParentCodec.SendText(string.Format("xCommand Camera Ramp CameraId: {0} Pan: Right PanSpeed: {1}", CameraId, PanSpeed));
                isPanning = true;
            }
        }

        public void PanStop()
        {
            ParentCodec.SendText(string.Format("xCommand Camera Ramp CameraId: {0} Pan: Stop", CameraId));
            isPanning = false;
        }

        #endregion



        #region IHasCameraTiltControl Members

        public void TiltDown()
        {
            if (!isMoving)
            {
                ParentCodec.SendText(string.Format("xCommand Camera Ramp CameraId: {0} Tilt: Down TiltSpeed: {1}", CameraId, TiltSpeed));
                isTilting = true;
            }
        }

        public void TiltUp()
        {
            if (!isMoving)
            {
                ParentCodec.SendText(string.Format("xCommand Camera Ramp CameraId: {0} Tilt: Up TiltSpeed: {1}", CameraId, TiltSpeed));
                isTilting = true;
            }
        }

        public void TiltStop()
        {
            ParentCodec.SendText(string.Format("xCommand Camera Ramp CameraId: {0} Tilt: Stop", CameraId));
            isTilting = false;
        }

        #endregion

        #region IHasCameraZoomControl Members

        public void ZoomIn()
        {
            if (!isMoving)
            {
                ParentCodec.SendText(string.Format("xCommand Camera Ramp CameraId: {0} Zoom: In ZoomSpeed: {1}", CameraId, ZoomSpeed));
                isZooming = true;
            }
        }

        public void ZoomOut()
        {
            if (!isMoving)
            {            
                ParentCodec.SendText(string.Format("xCommand Camera Ramp CameraId: {0} Zoom: Out ZoomSpeed: {1}", CameraId, ZoomSpeed));
                isZooming = true;
            }
        }

        public void ZoomStop()
        {
            ParentCodec.SendText(string.Format("xCommand Camera Ramp CameraId: {0} Zoom: Stop", CameraId));
            isZooming = false;
        }

        #endregion
    
        #region IHasCameraFocusControl Members

        public void  FocusNear()
        {
            if (!isMoving)
            {            
                ParentCodec.SendText(string.Format("xCommand Camera Ramp CameraId: {0} Focus: Near", CameraId));
                isFocusing = true;
            }
        }

        public void  FocusFar()
        {
            if (!isMoving)
            {
                ParentCodec.SendText(string.Format("xCommand Camera Ramp CameraId: {0} Focus: Far", CameraId));
                isFocusing = true;
            }
        }

        public void FocusStop()
        {
            ParentCodec.SendText(string.Format("xCommand Camera Ramp CameraId: {0} Focus: Stop", CameraId));
            isFocusing = false;
        }

        public void TriggerAutoFocus()
        {
            ParentCodec.SendText(string.Format("xCommand Camera TriggerAutofocus CameraId: {0}", CameraId));
        }

        #endregion

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkCameraToApi(this, trilist, joinStart, joinMapKey, bridge);
        }
    }
}