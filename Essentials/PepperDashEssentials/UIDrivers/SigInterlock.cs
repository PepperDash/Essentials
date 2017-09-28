using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Used for interlocking sigs, using a set-clears-last-set model.
    /// </summary>
    public class SigInterlock
    {
        /// <summary>
        /// 
        /// </summary>
        public BoolInputSig CurrentSig { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public SigInterlock()
        {
        }

        /// <summary>
        /// Hides CurrentJoin and shows join. Does nothing when resending CurrentJoin
        /// </summary>
        public void ShowInterlocked(BoolInputSig sig)
        {
            if (CurrentSig == sig)
                return;
            SetButDontShow(sig);
            sig.BoolValue = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="join"></param>
        public void ShowInterlockedWithToggle(BoolInputSig sig)
        {
            if(CurrentSig == sig)
                HideAndClear();
            else
            {
                if(CurrentSig != null)
                    CurrentSig.BoolValue = false;
                CurrentSig = sig;
                CurrentSig.BoolValue = true;
            }

        }

        /// <summary>
        /// Hides current Sig and clears CurrentSig
        /// </summary>
        public void HideAndClear()
        {
            Hide();
            CurrentSig = null;
        }

        /// <summary>
        /// Hides the current Sig but does not clear the selected Sig in case
        /// it needs to be reshown
        /// </summary>
        public void Hide()
        {
            if(CurrentSig != null)
                CurrentSig.BoolValue = false;
        }

        /// <summary>
        /// If CurrentSig is set, it restores that Sig
        /// </summary>
        public void Show()
        {
            if(CurrentSig != null)
                CurrentSig.BoolValue = true;
        }

        /// <summary>
        /// Useful for pre-setting the interlock but not enabling it. Sets CurrentSig
        /// </summary>
        /// <param name="join"></param>
        public void SetButDontShow(BoolInputSig sig)
        {
            if (CurrentSig != null)
                CurrentSig.BoolValue = false;
            CurrentSig = sig;
        }
    }
}