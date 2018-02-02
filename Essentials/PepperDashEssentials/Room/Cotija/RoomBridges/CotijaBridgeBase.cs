using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class CotijaBridgeBase: Device
	{
		public CotijaSystemController Parent { get; private set; }

		public CotijaBridgeBase(string key, string name)
			: base(key, name)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent"></param>
		public void AddParent(CotijaSystemController parent)
		{
			Parent = parent;
		}
	}
}