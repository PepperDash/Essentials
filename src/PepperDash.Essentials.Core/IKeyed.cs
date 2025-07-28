using Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Unique key interface to require a unique key for the class
    /// </summary>
    public interface IKeyed
	{
        /// <summary>
        /// Unique Key
        /// </summary>
        [JsonProperty("key")]
		string Key { get; }
    }

}