using Newtonsoft.Json;
using PepperDash.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Devices.Common.Cameras
{
    /// <summary>
    /// Interface for camera capabilities
    /// </summary>
    public interface ICameraCapabilities: IKeyName
    {
        /// <summary>
        /// Indicates whether the camera can pan
        /// </summary>
        [JsonProperty("canPan", NullValueHandling = NullValueHandling.Ignore)]
        bool CanPan { get;  }

        /// <summary>
        /// Indicates whether the camera can tilt
        /// </summary>
        [JsonProperty("canTilt", NullValueHandling = NullValueHandling.Ignore)]
         bool CanTilt { get; }

        /// <summary>
        /// Indicates whether the camera can zoom
        /// </summary>
        [JsonProperty("canZoom", NullValueHandling = NullValueHandling.Ignore)]
        bool CanZoom { get;  }


        /// <summary>
        /// Indicates whether the camera can focus
        /// </summary>
        [JsonProperty("canFocus", NullValueHandling = NullValueHandling.Ignore)]
        bool CanFocus { get;  }
    }

    /// <summary>
    /// Indicates the capabilities of a camera
    /// </summary>
    public class CameraCapabilities : ICameraCapabilities
    {

        /// <summary>
        /// Unique Key
        /// </summary>
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        /// <summary>
        /// Isn't it obvious :)
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }


        /// <summary>
        /// Indicates whether the camera can pan
        /// </summary>
        [JsonProperty("canPan", NullValueHandling = NullValueHandling.Ignore)]
        public bool CanPan { get; set; }

        /// <summary>
        /// Indicates whether the camera can tilt
        /// </summary>
        [JsonProperty("canTilt", NullValueHandling = NullValueHandling.Ignore)]
        public bool CanTilt { get; set; }

        /// <summary>
        /// Indicates whether the camera can zoom
        /// </summary>
        [JsonProperty("canZoom", NullValueHandling = NullValueHandling.Ignore)]
        public bool CanZoom { get; set; }


        /// <summary>
        /// Indicates whether the camera can focus
        /// </summary>
        [JsonProperty("canFocus", NullValueHandling = NullValueHandling.Ignore)]
        public bool CanFocus { get; set; }
    }
}
