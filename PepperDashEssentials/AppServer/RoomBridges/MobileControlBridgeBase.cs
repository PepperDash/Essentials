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
	public abstract class MobileControlBridgeBase: EssentialsDevice
	{
		public MobileControlSystemController Parent { get; private set; }

		public string UserCode { get; private set; }

		public abstract string RoomName { get; }

		public MobileControlBridgeBase(string key, string name)
			: base(key, name)
		{
		}

		/// <summary>
		/// Set the parent.  Does nothing else.  Override to add functionality such
		/// as adding actions to parent
		/// </summary>
		/// <param name="parent"></param>
		public virtual void AddParent(MobileControlSystemController parent)
		{
			Parent = parent;
		}

		/// <summary>
		/// Sets the UserCode on the bridge object. Called from controller. A changed code will
		/// fire method UserCodeChange.  Override that to handle changes
		/// </summary>
		/// <param name="code"></param>
		public void SetUserCode(string code)
		{
			var changed = UserCode != code;
			UserCode = code;
			if (changed)
			{
				UserCodeChange();
			}
		}

		/// <summary>
		/// Empty method in base class.  Override this to add functionality
		/// when code changes
		/// </summary>
		protected virtual void UserCodeChange()
		{

		}
	}
}