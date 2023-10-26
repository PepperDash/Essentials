using System;
using Crestron.SimplSharpPro.Fusion;

namespace PepperDash.Essentials.Core.Fusion
{
    public class FusionOccupancySensorAsset
    {
        // SlotNumber fixed at 4

        public uint SlotNumber { get { return 4; } }
        public string Name { get { return "Occupancy Sensor"; } }
        public eAssetType Type { get; set; }
        public string InstanceId { get; set; }

        public FusionOccupancySensorAsset()
        {
        }

        public FusionOccupancySensorAsset(eAssetType type)
        {
            Type = type;

            InstanceId = Guid.NewGuid().ToString();
        }
    }
}