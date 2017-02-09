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
        public SourceListItem SourceItem { get; private set; }

        public SubpageReferenceListSourceItem(uint index, SubpageReferenceList owner, 
            SourceListItem sourceItem, Action<bool> routeAction)
            : base(index, owner)
        {
            SourceItem = sourceItem;
            owner.GetBoolFeedbackSig(index, 1).UserObject = new Action<bool>(routeAction);
            owner.StringInputSig(index, 1).StringValue = SourceItem.PreferredName;
        }

        public void RegisterForSourceChange(IHasCurrentSourceInfoChange room)
        {
            room.CurrentSourceInfoChange -= room_CurrentSourceInfoChange;
            room.CurrentSourceInfoChange += room_CurrentSourceInfoChange;
        }

        void room_CurrentSourceInfoChange(EssentialsRoomBase room, SourceListItem info, ChangeType type)
        {
            if (type == ChangeType.WillChange && info == SourceItem)
                Owner.BoolInputSig(Index, 1).BoolValue = false;
            else if (type == ChangeType.DidChange && info == SourceItem)
                Owner.BoolInputSig(Index, 1).BoolValue = true;
        }

        /// <summary>
        /// Called by SRL to release all referenced objects
        /// </summary>
        public override void Clear()
        {
            Owner.BoolInputSig(Index, 1).UserObject = null;
            Owner.StringInputSig(Index, 1).StringValue = "";
        }
    }
}