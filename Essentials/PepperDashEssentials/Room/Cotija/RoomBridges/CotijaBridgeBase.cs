using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class CotijaBridgeBase
	{
		public CotijaSystemController Parent { get; private set; }

		public CotijaBridgeBase(CotijaSystemController parent)
		{
			Parent = parent;
		}
	}
}