//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
//using Crestron.SimplSharpPro.UI;

//using PepperDash.Core;
//using PepperDash.Essentials.Core;

//namespace PepperDash.Essentials
//{

//    //*****************************************************************************
//    /// <summary>
//    /// Wrapper class for subpage reference list.  Contains helpful methods to get at the various signal groupings
//    /// and to get individual signals using an index and a join.
//    /// </summary>
//    public class SourceListSubpageReferenceList : SubpageReferenceList
//    {
//        public const uint SmartObjectJoin = 3801;

//        Action<uint> SourceSelectCallback;

//        EssentialsRoom CurrentRoom;

//        public SourceListSubpageReferenceList(BasicTriListWithSmartObject tl, 
//            Action<uint> sourceSelectCallback)
//            : base(tl, SmartObjectJoin, 3, 1, 3)
//        {
//            SourceSelectCallback = sourceSelectCallback;
//        }

//        void SetSourceList(Dictionary<uint, SourceListItem> dict)
//        {
//            // Iterate all positions, including ones missing from the dict.
//            var max = dict.Keys.Max();
//            for (uint i = 1; i <= max; i++)
//            {
//                // Add the source if it's in the dict
//                if (dict.ContainsKey(i))
//                {
//                    Items.Add(new SourceListSubpageReferenceListItem(i, dict[i], this, SourceSelectCallback));
//                    // Plug the callback function into the buttons
//                }
//                // Blank the line
//                else
//                    Items.Add(new SourceListSubpageReferenceListItem(i, null, 
//                        this, SourceSelectCallback));
//            }
//            Count = (ushort)max;
//        }

//        /// <summary>
//        /// Links the SRL to the Room's PresentationSourceChange event for updating of the UI
//        /// </summary>
//        /// <param name="room"></param>
//        public void AttachToRoom(EssentialsRoom room)
//        {
//            CurrentRoom = room;
//            SetSourceList(room.Sources);
//            CurrentRoom.PresentationSourceChange -= CurrentRoom_PresentationSourceChange;
//            CurrentRoom.PresentationSourceChange += CurrentRoom_PresentationSourceChange;
//            SetPresentationSourceFb(CurrentRoom.CurrentPresentationSource);
//        }

//        /// <summary>
//        /// Disconnects the SRL from a Room's PresentationSourceChange
//        /// </summary>
//        public void DetachFromCurrentRoom()
//        {
//            ClearPresentationSourceFb(CurrentRoom.CurrentPresentationSource);
//            if(CurrentRoom != null)
//                CurrentRoom.PresentationSourceChange -= CurrentRoom_PresentationSourceChange;
//            CurrentRoom = null;
//        }

//        // Handler to route source changes into list feedback
//        void CurrentRoom_PresentationSourceChange(object sender, EssentialsRoomSourceChangeEventArgs args)
//        {
//            Debug.Console(2, "SRL received source change");
//            ClearPresentationSourceFb(args.OldSource);
//            SetPresentationSourceFb(args.NewSource);
//        }

//        void ClearPresentationSourceFb(IPresentationSource source)
//        {
//            if (source == null) return;
//            var oldSourceItem = (SourceListSubpageReferenceListItem)Items.FirstOrDefault(
//                i => ((SourceListSubpageReferenceListItem)i).SourceDevice == source);
//            if (oldSourceItem != null)
//                oldSourceItem.ClearFeedback();
//        }

//        void SetPresentationSourceFb(SourceListItem source)
//        {
//            if (source == null) return;
//            // Now set the new source to light up
//            var newSourceItem = (SourceListSubpageReferenceListItem)Items.FirstOrDefault(
//                i => ((SourceListSubpageReferenceListItem)i).SourceDevice == source);
//            if (newSourceItem != null)
//                newSourceItem.SetFeedback();
//        }
//    }

//    public class SourceListSubpageReferenceListItem : SubpageReferenceListItem
//    {
//        public readonly IPresentationSource SourceDevice;

//        public const uint ButtonPressJoin = 1;
//        public const uint SelectedFeedbackJoin = 2;
//        public const uint ButtonTextJoin = 1;
//        public const uint IconNameJoin = 2;

//        public SourceListSubpageReferenceListItem(uint index, SourceListItem srcDeviceItem, 
//            SubpageReferenceList owner, Action<uint> sourceSelectCallback)
//            : base(index, owner)
//        {
//            if (srcDeviceItem == null) throw new ArgumentNullException("srcDeviceItem");
//            if (owner == null) throw new ArgumentNullException("owner");
//            if (sourceSelectCallback == null) throw new ArgumentNullException("sourceSelectCallback");


//            SourceDevice = srcDeviceItem;
//            var nameSig = owner.StringInputSig(index, ButtonTextJoin);
//            // Should be able to see if there is not enough buttons right here
//            if (nameSig == null)
//            {
//                Debug.Console(0, "ERROR: Item {0} does not exist on source list SRL", index);
//                return;
//            }
//            nameSig.StringValue = srcDeviceItem.Name;
//            owner.StringInputSig(index, IconNameJoin).StringValue = srcDeviceItem.Icon;

//            // Assign a source selection action to the appropriate button's UserObject - on release
//            owner.GetBoolFeedbackSig(index, ButtonPressJoin).UserObject = new Action<bool>(b => 
//                { if (!b) sourceSelectCallback(index); });
			
//            // hook up the video icon
//            var videoDev = srcDeviceItem as IAttachVideoStatus;
//            if (videoDev != null)
//            {
//                var status = videoDev.GetVideoStatuses();
//                if (status != null)
//                {
//                    Debug.Console(1, "Linking {0} video status to SRL", videoDev.Key);
//                    videoDev.GetVideoStatuses().VideoSyncFeedback.LinkInputSig(owner.BoolInputSig(index, 3));
//                }
//            }
//        }

//        public void SetFeedback()
//        {
//            Owner.BoolInputSig(Index, SelectedFeedbackJoin).BoolValue = true;
//        }

//        public void ClearFeedback()
//        {
//            Owner.BoolInputSig(Index, SelectedFeedbackJoin).BoolValue = false;
//        }
//    }
//}