using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.PageManagers
{
    public class ThreePanelPlusOnePageManager : PageManager
    {
        protected BasicTriListWithSmartObject TriList;
		
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
        /// 
        /// </summary>
        protected uint CurrentVisiblePosition5Item;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trilist"></param>
        public ThreePanelPlusOnePageManager(BasicTriListWithSmartObject trilist)
        {
            TriList                     = trilist;
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
            var fixedSigs = FixedVisibilityJoins.Select(u => TriList.BooleanInput[(uint)u]).ToList();
            foreach (var sig in fixedSigs)
                sig.BoolValue = true;
			
            if (ShowPosition5Tabs)
            {
                // Show selected tab
                TriList.BooleanInput[Position5SubpageJoins[CurrentVisiblePosition5Item]].BoolValue = true;
                // hook up tab object
                var tabSo = TriList.SmartObjects[Position5TabsId];
                tabSo.BooleanOutput["Tab Button 1 Press"].UserObject =  new Action<bool>(b => { if (!b) ShowTab(1); });
                tabSo.BooleanOutput["Tab Button 2 Press"].UserObject =  new Action<bool>(b => { if (!b) ShowTab(2); });
                tabSo.SigChange                                      -= tabSo_SigChange;
                tabSo.SigChange                                      += tabSo_SigChange;
            }
        }

        void tabSo_SigChange(Crestron.SimplSharpPro.GenericBase currentDevice, Crestron.SimplSharpPro.SmartObjectEventArgs args)
        {
            var uo = args.Sig.UserObject;
            if(uo is Action<bool>)
                (uo as Action<bool>)(args.Sig.BoolValue);
        }

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
            CurrentVisiblePosition5Item                                      = number;
            bi[Position5SubpageJoins[CurrentVisiblePosition5Item]].BoolValue = true;
        }
    }
}