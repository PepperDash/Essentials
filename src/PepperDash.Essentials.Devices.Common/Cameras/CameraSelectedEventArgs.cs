using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    public class CameraSelectedEventArgs : EventArgs
    {
        public CameraBase SelectedCamera { get; private set; }

        public CameraSelectedEventArgs(CameraBase camera)
        {
            SelectedCamera = camera;
        }
    }
}