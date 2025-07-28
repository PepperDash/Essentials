using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Communications
{
    /// <summary>
    /// Describes a device that can automatically attempt to reconnect
    /// </summary>
    public interface IAutoReconnect
	{
        /// <summary>
        /// Enable automatic recconnect
        /// </summary>
        [JsonProperty("autoReconnect")]
		bool AutoReconnect { get; set; }
        /// <summary>
        /// Interval in ms to attempt automatic recconnections
        /// </summary>
        [JsonProperty("autoReconnectIntervalMs")]
		int AutoReconnectIntervalMs { get; set; }
	}
}