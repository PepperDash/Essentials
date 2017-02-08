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
    public class SubpageReferenceListSourceItem : SubpageReferenceListItem
    {
        public SubpageReferenceListSourceItem(uint index, SubpageReferenceList owner, 
            string name, Action<bool> routeAction)
            : base(index, owner)
        {
            owner.GetBoolFeedbackSig(index, 1).UserObject = new Action<bool>(routeAction);
            owner.StringInputSig(index, 1).StringValue = name;
        }

        /// <summary>
        /// Called by SRL to release all referenced objects
        /// </summary>
        public override void Clear()
        {
            Owner.BoolInputSig(Index, 1).UserObject = null;
            Owner.StringInputSig(Index, 1).StringValue = "";
        }

        //public override void Refresh() 
        //{ 
     
        //}
    }
}