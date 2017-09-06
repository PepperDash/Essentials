using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials
{
    public class JoinedSigInterlock
    {
        public uint CurrentJoin { get; private set; }

        BasicTriList TriList;

        public JoinedSigInterlock(BasicTriList triList)
        {
            TriList = triList;
        }

        /// <summary>
        /// Hides CurrentJoin and shows join
        /// </summary>
        public void ShowInterlocked(uint join)
        {
            if (CurrentJoin > 0)
                TriList.BooleanInput[CurrentJoin].BoolValue = false;
            CurrentJoin = join;
            TriList.BooleanInput[CurrentJoin].BoolValue = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="join"></param>
        public void ShowInterlockedWithToggle(uint join)
        {
            if (CurrentJoin == join)
                HideAndClear();
            else
            {
                if (CurrentJoin > 0)
                    TriList.BooleanInput[CurrentJoin].BoolValue = false;
                CurrentJoin = join;
                TriList.BooleanInput[CurrentJoin].BoolValue = true;
            }
        }
        /// <summary>
        /// Hides current join and clears CurrentJoin
        /// </summary>
        public void HideAndClear()
        {
            Hide();
            CurrentJoin = 0;
        }

        /// <summary>
        /// Hides the current join but does not clear the selected join in case
        /// it needs to be reshown
        /// </summary>
        public void Hide()
        {
            if (CurrentJoin > 0)
                TriList.BooleanInput[CurrentJoin].BoolValue = false;
        }

        /// <summary>
        /// If CurrentJoin is set, it restores that join
        /// </summary>
        public void Show()
        {
            if (CurrentJoin > 0)
                TriList.BooleanInput[CurrentJoin].BoolValue = true;
        }

    }
}