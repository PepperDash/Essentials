using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.PageManagers
{
 /// <summary>
 /// Represents a SinglePageManager
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