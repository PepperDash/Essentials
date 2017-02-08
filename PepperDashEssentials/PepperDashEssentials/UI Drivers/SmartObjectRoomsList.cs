using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials
{
	public class SmartObjectRoomsList : SmartObjectDynamicList
	{
		public uint StatusSigOffset { get; private set; }
		List<SmartObjectRoomsListItem> Items;

		public SmartObjectRoomsList(SmartObject so, uint nameSigOffset, uint statusSigOffset)
			: base(so, true, nameSigOffset)
		{
			StatusSigOffset = statusSigOffset;
			Items = new List<SmartObjectRoomsListItem>();
		}

		public void AddRoomItem(SmartObjectRoomsListItem item)
		{
			Items.Add(item);
		}

		public void SetItemStatusText(uint index, string text)
		{
			if (index > MaxCount) return;
			// The list item template defines CIPS tags that refer to standard joins
			(SmartObject.Device as BasicTriList).StringInput[StatusSigOffset + index].StringValue = text;
		}

		/// <summary>
		/// Sets feedback for the given room
		/// </summary>
		public void SetFeedbackForRoom(EssentialsHuddleSpaceRoom room)
		{
			var itemToSet = Items.FirstOrDefault(i => i.Room == room);
			if (itemToSet != null)
				SetFeedback(itemToSet.Index, true);
		}
	}

	public class SmartObjectRoomsListItem
	{
		public EssentialsHuddleSpaceRoom Room { get; private set; }
		SmartObjectRoomsList Parent;
		public uint Index { get; private set; }

		public SmartObjectRoomsListItem(EssentialsHuddleSpaceRoom room, uint index, SmartObjectRoomsList parent, 
			Action<bool> buttonAction)
		{
			Room = room;
			Parent = parent;
			Index = index;
			if (room == null) return;

			// Set "now" states
			parent.SetItemMainText(index, room.Name);
			UpdateItem(room.CurrentSourceInfo);
			// Watch for later changes
			room.CurrentSourceInfoChange += new SourceInfoChangeHandler(room_CurrentSourceInfoChange);
			parent.SetItemButtonAction(index, buttonAction);
		}

		void room_CurrentSourceInfoChange(EssentialsRoomBase room, SourceListItem info, ChangeType type)
		{
			UpdateItem(info);
		}

		/// <summary>
		/// Helper to handle source events and startup syncing with room's current source
		/// </summary>
		/// <param name="info"></param>
		void UpdateItem(SourceListItem info)
		{
			if (info == null || info.Type == eSourceListItemType.Off)
			{
				Parent.SetItemStatusText(Index, "");
				Parent.SetItemIcon(Index, "Blank");
			}
			else
			{
				Parent.SetItemStatusText(Index, info.PreferredName);
				Parent.SetItemIcon(Index, info.AltIcon);
			}
		}
	}
}