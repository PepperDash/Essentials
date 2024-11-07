using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.UI_PageManagers
{
	/// <summary>
	/// A simple class that hides and shows the default subpage for a given source type
	/// </summary>
	public class SinglePageManager : PageManager
	{
		BasicTriList TriList;
		uint BackingPageJoin;

		public SinglePageManager(uint pageJoin, BasicTriList trilist)
		{
			TriList = trilist;
			BackingPageJoin = pageJoin;
		}

		public override void Show()
		{
			TriList.BooleanInput[BackingPageJoin].BoolValue = true;
		}

		public override void Hide()
		{
			TriList.BooleanInput[BackingPageJoin].BoolValue = false;
		}
	}
}