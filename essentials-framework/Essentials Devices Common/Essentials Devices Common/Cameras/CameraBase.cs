using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Presets;
using PepperDash.Essentials.Devices.Common.Codec;

using Newtonsoft.Json;

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

    public abstract class CameraBase : Device, IRoutingOutputs
	{
        public eCameraControlMode ControlMode { get; protected set; }

        #region IRoutingOutputs Members

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; protected set; }

        #endregion

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
			base(key, name) 
        {
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

            ControlMode = eCameraControlMode.Manual;
        }		
	}

    public class CameraPreset : PresetBase
    {
        public CameraPreset(int id, string description, bool isDefined, bool isDefinable)
            : base(id, description, isDefined, isDefinable)
        {

        }
    }


	public class CameraPropertiesConfig
	{
		public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

		public ControlPropertiesConfig Control { get; set; }

        [JsonProperty("supportsAutoMode")]
        public bool SupportsAutoMode { get; set; }

        [JsonProperty("supportsOffMode")]
        public bool SupportsOffMode { get; set; }

        [JsonProperty("presets")]
        public List<CameraPreset> Presets { get; set; }
	}
}