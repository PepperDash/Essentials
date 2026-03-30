using System;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Event arguments for the CameraSelected event
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CameraSelectedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets or sets the SelectedCamera
        /// </summary>
        public T SelectedCamera { get; private set; }

        /// <summary>
        /// Constructor for CameraSelectedEventArgs
        /// </summary>
        /// <param name="camera"></param>
        public CameraSelectedEventArgs(T camera)
        {
            SelectedCamera = camera;
        }
    }
}
