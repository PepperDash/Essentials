using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.Fusion;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Fusion
{
    // Helper Classes for GUIDs

    /// <summary>
    /// Stores GUIDs to be written to a file in NVRAM 
    /// </summary>
    public class FusionRoomGuids
    {
        public string RoomName { get; set; }
        public uint IpId { get; set; }
        public string RoomGuid { get; set; }
        public FusionOccupancySensorAsset OccupancyAsset { get; set; }
        public Dictionary<int, FusionAsset> StaticAssets { get; set; }

        public FusionRoomGuids()
        {
            StaticAssets = new Dictionary<int, FusionAsset>();
            OccupancyAsset = new FusionOccupancySensorAsset();
        }

        public FusionRoomGuids(string roomName, uint ipId, string roomGuid, Dictionary<int, FusionAsset> staticAssets)
        {
            RoomName = roomName;
            IpId = ipId;
            RoomGuid = roomGuid;

            StaticAssets = staticAssets;
            OccupancyAsset = new FusionOccupancySensorAsset();
        }

        public FusionRoomGuids(string roomName, uint ipId, string roomGuid, Dictionary<int, FusionAsset> staticAssets, FusionOccupancySensorAsset occAsset)
        {
            RoomName = roomName;
            IpId = ipId;
            RoomGuid = roomGuid;

            StaticAssets = staticAssets;
            OccupancyAsset = occAsset;
        }

        /// <summary>
        /// Generates a new room GUID prefixed by the program slot number and NIC MAC address
        /// </summary>
        /// <param name="progSlot"></param>
        /// <param name="mac"></param>
        public string GenerateNewRoomGuid(uint progSlot, string mac)
        {
            Guid roomGuid = Guid.NewGuid();

            return string.Format("{0}-{1}-{2}", progSlot, mac, roomGuid.ToString());
        }


        /// <summary>
        /// Adds an asset to the StaticAssets collection and returns the new asset
        /// </summary>
        /// <param name="room"></param>
        /// <param name="uid"></param>
        /// <param name="assetName"></param>
        /// <param name="type"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public FusionAsset AddStaticAsset(FusionRoom room, int uid, string assetName, string type, string instanceId)
        {
            var slotNum = GetNextAvailableAssetNumber(room);

            Debug.Console(2, "Adding Fusion Asset: {0} of Type: {1} at Slot Number: {2} with GUID: {3}", assetName, type, slotNum, instanceId);

            var tempAsset = new FusionAsset(slotNum, assetName, type, instanceId);

            StaticAssets.Add(uid, tempAsset);

            return tempAsset;
        }

        /// <summary>
        /// Returns the next available slot number in the Fusion UserConfigurableAssetDetails collection
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static uint GetNextAvailableAssetNumber(FusionRoom room)
        {
            uint slotNum = 0;

            foreach (var item in room.UserConfigurableAssetDetails)
            {
                if(item.Number > slotNum)
                    slotNum = item.Number;
            }

            if (slotNum < 5)
            {
                slotNum = 5;
            }
            else
                slotNum = slotNum + 1;

            Debug.Console(2, "#Next available fusion asset number is: {0}", slotNum);

            return slotNum;
        }

    }

    //***************************************************************************************************

    //****************************************************************************************************
    // Helper Classes for XML API


    //[XmlRoot(ElementName = "RequestAction")]

    //[XmlRoot(ElementName = "ActionResponse")]

    //[XmlRoot(ElementName = "Parameter")]

    ////[XmlRoot(ElementName = "Parameters")]
    //public class Parameters
    //{
    //    //[XmlElement(ElementName = "Parameter")]
    //    public List<Parameter> Parameter { get; set; }
    //}  

    //[XmlRoot(ElementName = "Event")]

    //[XmlRoot(ElementName = "Resources")]

    //[XmlRoot(ElementName = "Rooms")]

    //[XmlRoot(ElementName = "Room")]

    //[XmlRoot(ElementName = "Attendees")]

    //[XmlRoot(ElementName = "Required")]

    //[XmlRoot(ElementName = "Optional")]

    //[XmlRoot(ElementName = "MeetingType")]

    //[XmlRoot(ElementName = "MeetingTypes")]

    //[XmlRoot(ElementName = "LiveMeeting")]

    //[XmlRoot(ElementName = "LiveMeetingURL")]
}