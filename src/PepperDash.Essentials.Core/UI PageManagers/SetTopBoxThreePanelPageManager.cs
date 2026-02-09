using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Presets;
using Serilog.Events;

namespace PepperDash.Essentials.Core.PageManagers
{
	/// <summary>
	/// Represents a ThreePanelPlusOnePageManager
	/// </summary>
	public class ThreePanelPlusOnePageManager : PageManager
	{
		/// <summary>
		/// The trilist
		/// </summary>
		protected BasicTriListWithSmartObject TriList;
		
		/// <summary>
		/// Gets or sets the Position5TabsId
		/// </summary>
		public uint Position5TabsId { get; set; }

		/// <summary>
		/// Show the tabs on the third panel
		/// </summary>
		protected bool ShowPosition5Tabs;

		/// <summary>
		/// Joins that are always visible when this manager is visible
		/// </summary>
		protected uint[] FixedVisibilityJoins;

		/// <summary>
		/// Gets or sets the current visible item in position 5
		/// </summary>
		protected uint CurrentVisiblePosition5Item;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="trilist"></param>
		public ThreePanelPlusOnePageManager(BasicTriListWithSmartObject trilist)
		{
			TriList = trilist;
            CurrentVisiblePosition5Item = 1;
		}
	
		/// <summary>
		/// The joins for the switchable panel in position 5
		/// </summary>
		Dictionary<uint, uint> Position5SubpageJoins = new Dictionary<uint, uint>
	        {
	            { 1, 10053 },
	            { 2, 10054 }
	        };

		/// <summary>
		/// 
		/// </summary>
		public override void Show()
		{
			// Project the joins into corresponding sigs.
			var fixedSigs = FixedVisibilityJoins.Select(u => TriList.BooleanInput[u]).ToList();
            foreach (var sig in fixedSigs)
                sig.BoolValue = true;
			
			if (ShowPosition5Tabs)
			{
				// Show selected tab
				TriList.BooleanInput[Position5SubpageJoins[CurrentVisiblePosition5Item]].BoolValue = true;
				// hook up tab object
				var tabSo = TriList.SmartObjects[Position5TabsId];
                tabSo.BooleanOutput["Tab Button 1 Press"].UserObject = new Action<bool>(b => { if (!b) ShowTab(1); });
                tabSo.BooleanOutput["Tab Button 2 Press"].UserObject = new Action<bool>(b => { if (!b) ShowTab(2); });
                tabSo.SigChange -= tabSo_SigChange;
                tabSo.SigChange += tabSo_SigChange;
            }
		}

        void tabSo_SigChange(Crestron.SimplSharpPro.GenericBase currentDevice, Crestron.SimplSharpPro.SmartObjectEventArgs args)
        {
            var uo = args.Sig.UserObject;
            if(uo is Action<bool>)
                (uo as Action<bool>)(args.Sig.BoolValue);
        }

		/// <summary>
		/// Hide method
		/// </summary>
		/// <inheritdoc />
		public override void Hide()
		{
            var fixedSigs = FixedVisibilityJoins.Select(u => TriList.BooleanInput[u]).ToList();
            foreach (var sig in fixedSigs)
                sig.BoolValue = false;
			if (ShowPosition5Tabs)
			{
                TriList.BooleanInput[Position5SubpageJoins[CurrentVisiblePosition5Item]].BoolValue = false;

                //var tabSo = TriList.SmartObjects[Position5TabsId];
                //tabSo.BooleanOutput["Tab Button 1 Press"].UserObject = null;
                //tabSo.BooleanOutput["Tab Button 2 Press"].UserObject = null;
			}
		}

		void ShowTab(uint number)
		{
			// Ignore re-presses
			if (CurrentVisiblePosition5Item == number) return;
			// Swap subpage
			var bi = TriList.BooleanInput;
			if (CurrentVisiblePosition5Item > 0)
				bi[Position5SubpageJoins[CurrentVisiblePosition5Item]].BoolValue = false;
			CurrentVisiblePosition5Item = number;
			bi[Position5SubpageJoins[CurrentVisiblePosition5Item]].BoolValue = true;
		}
	}



	/// <summary>
	/// Represents a SetTopBoxThreePanelPageManager
	/// </summary>
	public class SetTopBoxThreePanelPageManager : ThreePanelPlusOnePageManager
	{
		ISetTopBoxControls SetTopBox;
		DevicePresetsView PresetsView;

		/// <summary>
		/// Gets or sets the DpadSmartObjectId
		/// </summary>
		public uint DpadSmartObjectId { get; set; }

		/// <summary>
		/// Gets or sets the NumberPadSmartObjectId
		/// </summary>
		public uint NumberPadSmartObjectId { get; set; }

		/// <summary>
		/// Gets or sets the PresetsSmartObjectId
		/// </summary>
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
                Debug.LogMessage(LogEventLevel.Debug, stb, "WARNING: Not configured to show any UI elements");
                FixedVisibilityJoins = new uint[] { 10091 };
            }

			// Build presets
            if (stb.HasPresets && stb.TvPresets != null)
			{
				PresetsView = new DevicePresetsView(trilist, stb.TvPresets);
			}
		}

		/// <summary>
		/// Show method
		/// </summary>
		/// <inheritdoc />
		public override void Show()
		{
			if(PresetsView != null)
				PresetsView.Attach();
			base.Show();
		}

		/// <summary>
		/// Hide method
		/// </summary>
		/// <inheritdoc />
		public override void Hide()
		{
            if (PresetsView != null)
    			PresetsView.Detach();
			base.Hide();
		}
	}
}