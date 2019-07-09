using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common.Codec;
using System.Text.RegularExpressions;
using Crestron.SimplSharp.Reflection;


namespace PepperDash.Essentials.Devices.Common.Cameras
{
    public enum eCameraCapabilities
    {
        None = 0,
        Pan = 1,
        Tilt = 2, 
        Zoom = 4,
        Focus = 8
    }

	public abstract class CameraBase : Device
	{
        public bool CanPan
        {
            get
            {
                return (Capabilities & eCameraCapabilities.Pan) == eCameraCapabilities.Pan;
            }
        }

        public bool CanTilt
        {
            get
            {
                return (Capabilities & eCameraCapabilities.Tilt) == eCameraCapabilities.Tilt;
            }
        }

        public bool CanZoom
        {
            get
            {
                return (Capabilities & eCameraCapabilities.Zoom) == eCameraCapabilities.Zoom;
            }
        }

        public bool CanFocus
        {
            get
            {
                return (Capabilities & eCameraCapabilities.Focus) == eCameraCapabilities.Focus;
            }
        }

        // A bitmasked value to indicate the movement capabilites of this camera
        protected eCameraCapabilities Capabilities { get; set; }

		public CameraBase(string key, string name) :
			base(key, name) { }		
	}

	public class CameraPropertiesConfig
	{
		public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

		public ControlPropertiesConfig Control { get; set; }

	}
}