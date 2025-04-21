using System;
using System.Collections.Generic;
using System.Linq;

using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Basically a List , with an indexer to find ports by key name
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
}