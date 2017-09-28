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
        /// <summary>
        /// 
        /// </summary>
        SmartObjectDynamicList MenuList;
        /// <summary>
        /// 
        /// </summary>
        SubpageReferenceList StatusList;
        /// <summary>
        /// References lines in the list against device instances
        /// </summary>
        Dictionary<ICommunicationMonitor, uint> StatusListDeviceIndexes;
        /// <summary>
        /// 
        /// </summary>
        IAVDriver Parent;
        /// <summary>
        /// 
        /// </summary>
        JoinedSigInterlock PagesInterlock;

        /// <summary>
        /// 1
        /// </summary>
        public const uint JoinText = 1;



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

            MenuList.SetItemMainText(2, "Display Controls");
            MenuList.SetItemButtonAction(2, b => { 
                if (b) PagesInterlock.ShowInterlocked(UIBoolJoin.TechDisplayControlsVisible); 
                MenuList.SetFeedback(2, true);
            });           
            
            MenuList.SetItemMainText(3, "Panel Setup");
            MenuList.SetItemButtonAction(3, b => { 
                if (b) PagesInterlock.ShowInterlocked(UIBoolJoin.TechPanelSetupVisible);
                MenuList.SetFeedback(3, true);
            });
            MenuList.Count = 3;

            BuildStatusList();
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Show()
        {
            TriList.SetBool(UIBoolJoin.TechCommonItemsVisbible, true);
            PagesInterlock.Show();
            base.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Hide()
        {
            TriList.SetBool(UIBoolJoin.TechCommonItemsVisbible, false);
            PagesInterlock.Hide();
            base.Hide();
        }

        /// <summary>
        /// 
        /// </summary>
        void BuildStatusList()
        {
            StatusList = new SubpageReferenceList(TriList, UISmartObjectJoin.TechStatusList, 3, 3, 3);
            StatusListDeviceIndexes = new Dictionary<ICommunicationMonitor, uint>();
            uint i = 0;
            foreach (var d in DeviceManager.AllDevices)
            {
                // make sure it is both ICommunicationMonitor and a Device
                var sd = d as ICommunicationMonitor;
                if (sd == null)
                    continue;
                var dd = sd as Device;
                if(dd == null)
                    continue;
                i++;
                StatusList.StringInputSig(i, 1).StringValue = dd.Name;
                StatusList.UShortInputSig(i, 1).UShortValue = (ushort)sd.CommunicationMonitor.Status;
                StatusListDeviceIndexes.Add(sd, i);
                sd.CommunicationMonitor.StatusChange += CommunicationMonitor_StatusChange ;
            }
            StatusList.Count = (ushort)i;
        }

        /// <summary>
        /// 
        /// </summary>
        void CommunicationMonitor_StatusChange(object sender, MonitorStatusChangeEventArgs e)
        {
            var c = sender as ICommunicationMonitor;
            if (StatusListDeviceIndexes.ContainsKey(c))
            {
                var i = StatusListDeviceIndexes[c];
                StatusList.UShortInputSig(i, 1).UShortValue = (ushort)e.Status;
            }
        }
    }
}