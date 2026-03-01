using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.PageManagers
{
 /// <summary>
 /// Represents a DiscPlayerMediumPageManager
 /// </summary>
	public class DiscPlayerMediumPageManager : MediumLeftSwitchablePageManager
	{
		IDiscPlayerControls Player;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="player">player controls object</param>
		/// <param name="trilist">trilist object</param>
		public DiscPlayerMediumPageManager(IDiscPlayerControls player, BasicTriListWithSmartObject trilist)
			: base(player.DisplayUiType)
		{
			Player = player;
			TriList = trilist;
		}

		/// <summary>
		/// Show method
		/// </summary>
		/// <inheritdoc />
		public override void Show()
		{
			uint offset = GetOffsetJoin();
			BackingPageJoin = offset + 1;
			AllLeftSubpages = new uint[] { 7, 8 };

			if (LeftSubpageJoin == 0)
				LeftSubpageJoin = offset + 8; // default to transport
			TriList.BooleanInput[BackingPageJoin].BoolValue = true;
			TriList.BooleanInput[LeftSubpageJoin].BoolValue = true;

			// Attach buttons to interlock
			foreach(var p in AllLeftSubpages)
			{
				var p2 = p; // scope
				TriList.SetSigFalseAction(10000 + p2, () => InterlockLeftSubpage(p2));
			}
		}

		/// <summary>
		/// Hide method
		/// </summary>
		/// <inheritdoc />
		public override void Hide()
		{
			TriList.BooleanInput[BackingPageJoin].BoolValue = false;
			TriList.BooleanInput[LeftSubpageJoin].BoolValue = false;
		}
	}
}