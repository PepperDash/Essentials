using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
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
		public void SetFeedbackForRoom(IEssentialsHuddleSpaceRoom room)
		{
			var itemToSet = Items.FirstOrDefault(i => i.Room == room);
			if (itemToSet != null)
				SetFeedback(itemToSet.Index, true);
		}
	}
}