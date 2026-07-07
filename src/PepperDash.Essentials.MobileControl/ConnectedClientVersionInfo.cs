using System;
using Newtonsoft.Json;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Represents the version information reported by a connected Mobile Control UI client
    /// </summary>
    public class ConnectedClientVersionInfo
    {
        /// <summary>
        /// Gets or sets the client id
        /// </summary>
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the room key the client joined
        /// </summary>
        [JsonProperty("roomKey")]
        public string RoomKey { get; set; }

        /// <summary>
        /// Gets or sets the touchpanel key the client joined as, if any
        /// </summary>
        [JsonProperty("touchpanelKey")]
        public string TouchpanelKey { get; set; }

        /// <summary>
        /// Gets or sets the app version reported by the client (e.g. the React app's build-time APP_VERSION)
        /// </summary>
        [JsonProperty("appVersion")]
        public string AppVersion { get; set; }

        /// <summary>
        /// Gets or sets the expected app version from the system config's versions.touchpanelWrapperApp, if configured
        /// </summary>
        [JsonProperty("expectedAppVersion")]
        public string ExpectedAppVersion { get; set; }

        /// <summary>
        /// Gets or sets the UTC time the client last reported this version
        /// </summary>
        [JsonProperty("lastSeen")]
        public DateTime LastSeen { get; set; }
    }
}
