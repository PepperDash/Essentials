using System;
using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials.Core.Routing
{
 /// <summary>
 /// Represents a RoutingPortCollection, which is essentially a List with an indexer for case-insensitive lookup of ports by their key names.
 /// </summary>
	public class RoutingPortCollection<T> : List<T> where T: RoutingPort
	{
		/// <summary>
		/// Case-insensitive port lookup linked to ports' keys
		/// </summary>
		public T this[string key] 
		{
			get
			{
				return this.FirstOrDefault(i => i.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
			}
		}
	}

/*    /// <summary>
	/// Basically a List , with an indexer to find ports by key name
	/// </summary>
	public class RoutingPortCollection<T, TSelector> : List<T> where T : RoutingPort<TSelector>
    {
        /// <summary>
        /// Case-insensitive port lookup linked to ports' keys
        /// </summary>
        public T this[string key]
        {
            get
            {
                return this.FirstOrDefault(i => i.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            }
        }
    }*/
}