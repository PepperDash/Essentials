using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
    public class SubpageReferenceListButtonAndModeItem : SubpageReferenceListItem
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="owner"></param>
        /// <param name="buttonMode">0=Share, 1=Phone Call, 2=Video Call, 3=End Meeting</param>
        /// <param name="pressAction"></param>
        public SubpageReferenceListButtonAndModeItem(uint index, SubpageReferenceList owner, 
            ushort buttonMode, Action<bool> pressAction)
            : base(index, owner)
        {
            Owner.GetBoolFeedbackSig(Index, 1).UserObject = pressAction;
            Owner.UShortInputSig(Index, 1).UShortValue = buttonMode;
        }

        /// <summary>
        /// Called by SRL to release all referenced objects
        /// </summary>
        public override void Clear()
        {
            Owner.BoolInputSig(Index, 1).UserObject = null;
            Owner.UShortInputSig(Index, 1).UShortValue = 0;
        }
    }
}