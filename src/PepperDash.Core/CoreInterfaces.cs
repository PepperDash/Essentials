using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Serilog;

namespace PepperDash.Core
{
    /// <summary>
    /// Unique key interface to require a unique key for the class
    /// </summary>
	public interface IKeyed
	{
        /// <summary>
        /// Gets the unique key associated with the object.
        /// </summary>
        [JsonProperty("key")]
		string Key { get; }
    }

    /// <summary>
    /// Named Keyed device interface. Forces the device to have a Unique Key and a name. 
    /// </summary>
	public interface IKeyName : IKeyed
    {
        /// <summary>
        /// Gets the name associated with the current object.
        /// </summary>
        [JsonProperty("name")]
		string Name { get; }
    }

}