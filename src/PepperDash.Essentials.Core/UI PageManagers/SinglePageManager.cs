using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.PageManagers
{
	/// <summary>
	/// Represents a SinglePageManager
	/// </summary>
	public class SinglePageManager : PageManager
	{
		BasicTriList TriList;
		uint BackingPageJoin;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="pageJoin">join for thepage</param>
		/// <param name="trilist">trilist</param>
		public SinglePageManager(uint pageJoin, BasicTriList trilist)
		{
			TriList = trilist;
			BackingPageJoin = pageJoin;
		}

		/// <summary>
		/// Show method
		/// </summary>
		/// <inheritdoc />
		public override void Show()
		{
			TriList.BooleanInput[BackingPageJoin].BoolValue = true;
		}

		/// <summary>
		/// Hide method
		/// </summary>
		public override void Hide()
		{
			TriList.BooleanInput[BackingPageJoin].BoolValue = false;
		}
	}
}