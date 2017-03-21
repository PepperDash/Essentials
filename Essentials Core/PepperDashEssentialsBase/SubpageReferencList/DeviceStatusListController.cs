//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.UI;

//namespace PepperDash.Essentials.Core
//{
//    /// <summary>
//    /// Controls the device/tech status list - links in to the first 10, 1, 5 statuses in an item's StatusProperties
//    /// </summary>
//    public class DeviceStatusListController : SubpageReferenceListController
//    {
//        Dictionary<uint, Device> Items = new Dictionary<uint, Device>();

//        public DeviceStatusListController(SmartObject list)
//        {
//            TheList = new SubpageReferenceList(list, 10, 1, 5);
//        }

//        /// <summary>
//        /// Attaches an item's StatusProperties to the list item.
//        /// THIS METHOD MAY BE BETTER ABSORBED INTO SOME OTHER CONTROLLER CLASS AS A
//        /// PSIG -> LIST ITEM ADAPTER
//        /// </summary>
//        /// <param name="index">List position</param>
//        /// <param name="device"></param>
//        public void AddItem(uint listIndex, Device device)
//        {
//            if (device == null) throw new ArgumentNullException("device");
//            Items[listIndex] = device;
			
//            // Feedback - read the status properties and if there is room for them on the list sigs
//            // link them up.
//            //foreach (PValue statusPsig in device.StatusProperties)
//            //{
//            //    uint num = statusPsig.Number; 
//            //    Sig listSig = null;
//            //    switch (statusPsig.Type) // Switch on the PSig type and whether the PSig number is within the increment range 
//            //    {
//            //        case eSigType.Bool:
//            //            if (num > TheList.BoolIncrement) return;
//            //            listSig = TheList.BoolInputSig(listIndex, num); // Pull the appropriate list sig.
//            //            break;
//            //        case eSigType.String:
//            //            if (num > TheList.StringIncrement) return;
//            //            listSig = TheList.StringInputSig(listIndex, num);
//            //            break;
//            //        case eSigType.UShort:
//            //            if (num > TheList.UShortIncrement) return;
//            //            listSig = TheList.UShortInputSig(listIndex, num);
//            //            break;
//            //        default:
//            //            return;
//            //    }
//            //    if (listSig != null) // If we got a sig, plug it into the PSig for updates.
//            //        statusPsig.AddLinkedSig(listSig, true);
//            //}

//            // Press/other handlers - read the Commands and if there is room, add them as Sig handlers.
//            //foreach (var id in device.Commands.Keys)
//            //{
//            //    var pValueNumber = id.Number;
//            //    Sig listSig = null;
//            //    // Switch on type of a command and if it's in range, get it's list Sig.
//            //    switch (id.Type)
//            //    {
//            //        case eSigType.Bool:
//            //            if (pValueNumber > TheList.BoolIncrement) return;
//            //            listSig = TheList.BoolFeedbackSig(listIndex, pValueNumber);
//            //            break;
//            //        case eSigType.String:
//            //            if (pValueNumber > TheList.StringIncrement) return;
//            //            listSig = TheList.StringOutputSig(listIndex, pValueNumber);
//            //            break;
//            //        case eSigType.UShort:
//            //            if (pValueNumber > TheList.UShortIncrement) return;
//            //            listSig = TheList.UShortOutputSig(listIndex, pValueNumber);			
//            //            break;
//            //        default:
//            //            return;
//            //    }
//            //    if (listSig != null) // If we got a sig, add the command to its ChangeAction 
//            //        SigToAction.GetSigToActionUserObjectForSig(listSig).SigChangeAction += device.Commands[id]; 
//            //        // This will need to be undone when detached MAKE A HELPER!!!!
//            //}

//            // "Custom things" below
//            // Set the name on sig 1 - just an assignment. Don't
//            var nameSig = TheList.StringInputSig(listIndex, 1);
//            if (nameSig != null) 
//                nameSig.StringValue = device.Key;

//            // Map IsOnline bool to a 0 / 1 state analog icon
//            // Add an action to the online PValue that maps to a ushort sig on the list  POTENTIAL LEAK HERE IF
//            // this isn't cleaned up on disconnect
//            var onlineSig = TheList.UShortInputSig(listIndex, 1);
//            //var onlinePValue = device.StatusProperties[Device.JoinIsOnline];
//            //if (onlineSig != null && onlinePValue != null)
//            //    onlinePValue.AddChangeAction(pv => onlineSig.UShortValue = (ushort)(onlinePValue.BoolValue ? 1 : 0));
//            //    //OR onlinePValue.AddLinkedSig(onlineSig, true); 

//            // Set the list length based on largest key
//            TheList.Count = (ushort)Items.Keys.DefaultIfEmpty().Max(); // The count will be the largest key or 0			
//        }
//    }


//}