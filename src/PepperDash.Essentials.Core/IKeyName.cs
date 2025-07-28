using Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{

    /// <summary>
    /// Named Keyed device interface. Forces the device to have a Unique Key and a name. 
    /// </summary>
	public interface IKeyName : IKeyed
    {
        /// <summary>
        /// Isn't it obvious :)
        /// </summary>
        [JsonProperty("name")]
		string Name { get; }
    }

}