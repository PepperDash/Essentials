using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Presets;

namespace PepperDash.Essentials.Core.PageManagers
{
	/// <summary>
	/// Represents a SetTopBoxMediumPageManager
	/// </summary>
	public class SetTopBoxMediumPageManager : MediumLeftSwitchablePageManager
	{
		ISetTopBoxControls SetTopBox;
		DevicePresetsView PresetsView;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="stb">set top box controls device</param>
		/// <param name="trilist">trilist device</param>
		public SetTopBoxMediumPageManager(ISetTopBoxControls stb, BasicTriListWithSmartObject trilist)
			: base(stb.DisplayUiType)
		{
			SetTopBox = stb;
			TriList = trilist;
			if(stb.TvPresets != null)
				PresetsView = new DevicePresetsView(trilist, stb.TvPresets);
		}

		/// <summary>
		/// Show method
		/// </summary>
		/// <inheritdoc />
		public override void Show()
		{
			if(PresetsView != null)
				PresetsView.Attach();
			uint offset = GetOffsetJoin();
			if (SetTopBox.HasDvr) // Show backing page with DVR controls
			{
				BackingPageJoin = offset + 1;
				AllLeftSubpages = new uint[] { 6, 7, 8 };
			}
			else // Show the backing page with no DVR controls
			{
				BackingPageJoin = offset + 2;
				AllLeftSubpages = new uint[] { 6, 7 };
			}

			if (LeftSubpageJoin == 0)
				LeftSubpageJoin = offset + 6; // default to presets
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