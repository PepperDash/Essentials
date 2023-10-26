using System;

namespace PepperDash.Essentials.Core.Fusion
{
    public class FusionAsset
    {
        public uint SlotNumber { get; set; }
        public string Name { get; set; }
        public string Type { get;  set; }
        public string InstanceId { get;set; }

        public FusionAsset()
        {

        }

        public FusionAsset(uint slotNum, string assetName, string type, string instanceId)
        {
            SlotNumber = slotNum;
            Name       = assetName;
            Type       = type;
            if (string.IsNullOrEmpty(instanceId))
            {
                InstanceId = Guid.NewGuid().ToString();
            }
            else
            {
                InstanceId = instanceId;
            }
        }
    }
}