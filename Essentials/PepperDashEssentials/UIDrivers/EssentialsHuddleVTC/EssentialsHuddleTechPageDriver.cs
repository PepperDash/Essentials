using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;
using PepperDash.Essentials.Core.Touchpanels.Keyboards;

namespace PepperDash.Essentials.UIDrivers
{
    public class EssentialsHuddleTechPageDriver : PanelDriverBase
    {
        SmartObjectDynamicList MenuList;
        IAVDriver Parent;
        JoinedSigInterlock PagesInterlock;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trilist"></param>
        /// <param name="parent"></param>
        public EssentialsHuddleTechPageDriver(BasicTriListWithSmartObject trilist, IAVDriver parent)
            : base(trilist)
        {
            Parent = parent;
            PagesInterlock = new JoinedSigInterlock(trilist);
            PagesInterlock.SetButDontShow(UIBoolJoin.TechSystemStatusVisible);

            trilist.SetSigFalseAction(UIBoolJoin.TechExitButton, Hide);

            MenuList = new SmartObjectDynamicList(trilist.SmartObjects[UISmartObjectJoin.TechMenuList], 
                true, 3100);

            MenuList.SetFeedback(1, true); // initial fb

            MenuList.SetItemMainText(1, "System Status");
            MenuList.SetItemButtonAction(1, b => { 
                if (b) PagesInterlock.ShowInterlocked(UIBoolJoin.TechSystemStatusVisible); 
                MenuList.SetFeedback(1, true);
            });

            MenuList.SetItemMainText(2, "Panel Setup");
            MenuList.SetItemButtonAction(2, b => { 
                if (b) PagesInterlock.ShowInterlocked(UIBoolJoin.TechPanelSetupVisible);
                MenuList.SetFeedback(2, true);
            });

            MenuList.SetItemMainText(3, "System Status");
            MenuList.SetItemButtonAction(3, b => { 
                if (b) PagesInterlock.ShowInterlocked(UIBoolJoin.TechDisplayControlsVisible); 
                MenuList.SetFeedback(3, true);
            });

            MenuList.Count = 3;
        }

        public override void Show()
        {
            TriList.SetBool(UIBoolJoin.TechCommonItemsVisbible, true);
            PagesInterlock.Show();
            base.Show();
        }

        public override void Hide()
        {
            TriList.SetBool(UIBoolJoin.TechCommonItemsVisbible, false);
            PagesInterlock.Hide();
            base.Hide();
        }
    }
}