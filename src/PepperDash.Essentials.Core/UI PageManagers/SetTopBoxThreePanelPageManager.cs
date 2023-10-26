using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Presets;

namespace PepperDash.Essentials.Core.PageManagers
{
    public class SetTopBoxThreePanelPageManager : ThreePanelPlusOnePageManager
	{
		ISetTopBoxControls SetTopBox;
		DevicePresetsView PresetsView;

		public uint DpadSmartObjectId { get; set; }
		public uint NumberPadSmartObjectId { get; set; }
		public uint PresetsSmartObjectId { get; set; }

		/// <summary>
		/// A page manager for set top box that shows some combination of four different panels,
		/// in three slots on the page.
		/// </summary>
		/// <param name="stb"></param>
		/// <param name="trilist"></param>
		public SetTopBoxThreePanelPageManager(ISetTopBoxControls stb, BasicTriListWithSmartObject trilist)
			: base(trilist)
		{
			SetTopBox = stb;
			TriList = trilist;

			DpadSmartObjectId = 10011;
			NumberPadSmartObjectId = 10014;
			PresetsSmartObjectId = 10012;
			Position5TabsId = 10081;

			bool dpad = stb.HasDpad;
			bool preset = stb.HasPresets;
			bool dvr = stb.HasDvr;
			bool numbers = stb.HasNumeric;

            if (dpad && !preset && !dvr && !numbers) FixedVisibilityJoins = new uint[] { 10031, 10091 };
            else if (!dpad && preset && !dvr && !numbers) FixedVisibilityJoins = new uint[] { 10032, 10091 };
            else if (!dpad && !preset && dvr && !numbers) FixedVisibilityJoins = new uint[] { 10033, 10091 };
            else if (!dpad && !preset && !dvr && numbers) FixedVisibilityJoins = new uint[] { 10034, 10091 };

            else if (dpad && preset && !dvr && !numbers) FixedVisibilityJoins = new uint[] { 10042, 10021, 10092 };
            else if (dpad && !preset && dvr && !numbers) FixedVisibilityJoins = new uint[] { 10043, 10021, 10092 };
            else if (dpad && !preset && !dvr && numbers) FixedVisibilityJoins = new uint[] { 10044, 10021, 10092 };
            else if (!dpad && preset && dvr && !numbers) FixedVisibilityJoins = new uint[] { 10043, 10022, 10092 };
            else if (!dpad && preset && !dvr && numbers) FixedVisibilityJoins = new uint[] { 10044, 10022, 10092 };
            else if (!dpad && !preset && dvr && numbers) FixedVisibilityJoins = new uint[] { 10044, 10023, 10092 };

            else if (dpad && preset && dvr && !numbers) FixedVisibilityJoins = new uint[] { 10053, 10032, 10011, 10093 };
            else if (dpad && preset && !dvr && numbers) FixedVisibilityJoins = new uint[] { 10054, 10032, 10011, 10093 };
            else if (dpad && !preset && dvr && numbers) FixedVisibilityJoins = new uint[] { 10054, 10033, 10011, 10093 };
            else if (!dpad && preset && dvr && numbers) FixedVisibilityJoins = new uint[] { 10054, 10033, 10012, 10093 };

            else if (dpad && preset && dvr && numbers)
            {
                FixedVisibilityJoins = new uint[] { 10081, 10032, 10011, 10093 }; // special case
                ShowPosition5Tabs = true;
            }
            // Bad config case
            else
            {
                Debug.Console(1, stb, "WARNING: Not configured to show any UI elements");
                FixedVisibilityJoins = new uint[] { 10091 };
            }

			// Build presets
            if (stb.HasPresets && stb.TvPresets != null)
			{
				PresetsView = new DevicePresetsView(trilist, stb.TvPresets);
			}
		}

		public override void Show()
		{
			if(PresetsView != null)
				PresetsView.Attach();
			base.Show();
		}

		public override void Hide()
		{
            if (PresetsView != null)
    			PresetsView.Detach();
			base.Hide();
		}
	}
}