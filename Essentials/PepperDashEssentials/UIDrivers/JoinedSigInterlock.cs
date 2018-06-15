using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
    public class JoinedSigInterlock
    {
        public uint CurrentJoin { get; private set; }

        BasicTriList TriList;

        public BoolFeedback IsShownFeedback;

        bool _IsShown;

        public bool IsShown 
        {
            get
            {
                return _IsShown;
            }
            private set
            {
                _IsShown = value;
                IsShownFeedback.FireUpdate();
            }
        }

        //public BoolFeedback ShownFeedback { get; private set; }

        public JoinedSigInterlock(BasicTriList triList)
        {
            TriList = triList;

            IsShownFeedback = new BoolFeedback(new Func<bool>( () => _IsShown));
        }

        /// <summary>
        /// Hides CurrentJoin and shows join. Will check and re-set signal if join
		/// equals CurrentJoin
        /// </summary>
        public void ShowInterlocked(uint join)
        {
			Debug.Console(2, "Trilist {0:X2}, interlock swapping {1} for {2}", TriList.ID, CurrentJoin, join);
            if (CurrentJoin == join && TriList.BooleanInput[join].BoolValue)
                return;
            SetButDontShow(join);
            TriList.SetBool(CurrentJoin, true);
            IsShown = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="join"></param>
        public void ShowInterlockedWithToggle(uint join)
        {
			Debug.Console(2, "Trilist {0:X2}, interlock swapping {1} for {2}", TriList.ID, CurrentJoin, join);
			if (CurrentJoin == join)
                HideAndClear();
            else
            {
                if (CurrentJoin > 0)
                    TriList.BooleanInput[CurrentJoin].BoolValue = false;
                CurrentJoin = join;
                TriList.BooleanInput[CurrentJoin].BoolValue = true;
                IsShown = true;
            }
        }
        /// <summary>
        /// Hides current join and clears CurrentJoin
        /// </summary>
        public void HideAndClear()
        {
			Debug.Console(2, "Trilist {0:X2}, interlock hiding {1}", TriList.ID, CurrentJoin);
            Hide();
            CurrentJoin = 0;
        }

        /// <summary>
        /// Hides the current join but does not clear the selected join in case
        /// it needs to be reshown
        /// </summary>
        public void Hide()
        {
			Debug.Console(2, "Trilist {0:X2}, interlock hiding {1}", TriList.ID, CurrentJoin);
            if (CurrentJoin > 0)
            {
                TriList.BooleanInput[CurrentJoin].BoolValue = false;
                IsShown = false;
            }
        }

        /// <summary>
        /// If CurrentJoin is set, it restores that join
        /// </summary>
        public void Show()
        {
			Debug.Console(2, "Trilist {0:X2}, interlock showing {1}", TriList.ID, CurrentJoin);
            if (CurrentJoin > 0)
            {
                TriList.BooleanInput[CurrentJoin].BoolValue = true;
                IsShown = true;
            }
        }

        /// <summary>
        /// Useful for pre-setting the interlock but not enabling it. Sets CurrentJoin
        /// </summary>
        /// <param name="join"></param>
        public void SetButDontShow(uint join)
        {
            if (CurrentJoin > 0)
            {
                TriList.BooleanInput[CurrentJoin].BoolValue = false;
                IsShown = false;
            }
            CurrentJoin = join;
        }

    }
}