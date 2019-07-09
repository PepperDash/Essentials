using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Presets;

namespace PepperDash.Essentials.Core.PageManagers
{
	/// <summary>
	/// A fixed-layout page manager that expects a DPad on the right, fixed portion of the page, and a two/three 
	/// tab switchable area on the left for presets, numeric and transport controls
	/// </summary>
	public class SetTopBoxMediumPageManager : MediumLeftSwitchablePageManager
	{
		ISetTopBoxControls SetTopBox;
		DevicePresetsView PresetsView;

		public SetTopBoxMediumPageManager(ISetTopBoxControls stb, BasicTriListWithSmartObject trilist)
			: base(stb.DisplayUiType)
		{
			SetTopBox = stb;
			TriList = trilist;
			if(stb.PresetsModel != null)
				PresetsView = new DevicePresetsView(trilist, stb.PresetsModel);
		}

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

		public override void Hide()
		{
			TriList.BooleanInput[BackingPageJoin].BoolValue = false;
			TriList.BooleanInput[LeftSubpageJoin].BoolValue = false;
		}
	}
}