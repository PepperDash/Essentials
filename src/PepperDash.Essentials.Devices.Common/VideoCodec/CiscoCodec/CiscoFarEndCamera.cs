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
            ParentCodec.EnqueueCommand(string.Format("xCommand Call FarEndControl Camera Move Value: Left CallId: {0}", CallId));           
        }

        public void PanRight()
        {
            ParentCodec.EnqueueCommand(string.Format("xCommand Call FarEndControl Camera Move Value: Right CallId: {0}", CallId)); 
        }

        public void PanStop()
        {
            Stop();
        }

        #endregion

        #region IHasCameraTiltControl Members

        public void TiltDown()
        {
            ParentCodec.EnqueueCommand(string.Format("xCommand Call FarEndControl Camera Move Value: Down CallId: {0}", CallId)); 
        }

        public void TiltUp()
        {
            ParentCodec.EnqueueCommand(string.Format("xCommand Call FarEndControl Camera Move Value: Up CallId: {0}", CallId)); 
        }

        public void TiltStop()
        {
            Stop();
        }

        #endregion

        #region IHasCameraZoomControl Members

        public void ZoomIn()
        {
            ParentCodec.EnqueueCommand(string.Format("xCommand Call FarEndControl Camera Move Value: ZoomIn CallId: {0}", CallId)); 
        }

        public void ZoomOut()
        {
            ParentCodec.EnqueueCommand(string.Format("xCommand Call FarEndControl Camera Move Value: ZoomOut CallId: {0}", CallId)); 
        }

        public void ZoomStop()
        {
            Stop();
        }

        #endregion


        void Stop()
        {
            ParentCodec.EnqueueCommand(string.Format("xCommand Call FarEndControl Camera Stop CallId: {0}", CallId)); 
        }

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkCameraToApi(this, trilist, joinStart, joinMapKey, bridge);
        }
    }
}