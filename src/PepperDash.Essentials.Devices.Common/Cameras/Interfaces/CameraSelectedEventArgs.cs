using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Event arguments for the CameraSelected event
    /// </summary>
    [Obsolete("Use CameraSelectedEventArgs<T> instead. This class will be removed in a future version")]
    public class CameraSelectedEventArgs : EventArgs
    {
        /// Gets or sets the SelectedCamera
        /// </summary>
        public CameraBase SelectedCamera { get; private set; }

        /// <summary>
        /// Constructor for CameraSelectedEventArgs
        /// </summary>
        /// <param name="camera"></param>
        public CameraSelectedEventArgs(CameraBase camera)
        {
            SelectedCamera = camera;
        }
    }

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
