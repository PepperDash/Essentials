using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM;

namespace PepperDash.Essentials.Bridges
{
    public static class DmRmcControllerApiExtensions
    {
        public static void LinkToApi(this DmRmcControllerBase rmc, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as DmRmcControllerJoinMap;

            if (joinMap == null)
                joinMap = new DmRmcControllerJoinMap();

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, rmc, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            rmc.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            if(rmc.VideoOutputResolutionFeedback != null)
                rmc.VideoOutputResolutionFeedback.LinkInputSig(trilist.StringInput[joinMap.CurrentOutputResolution]);
            if(rmc.EdidManufacturerFeedback != null)
                rmc.EdidManufacturerFeedback.LinkInputSig(trilist.StringInput[joinMap.EdidManufacturer]);
            if(rmc.EdidNameFeedback != null)
                rmc.EdidNameFeedback.LinkInputSig(trilist.StringInput[joinMap.EdidName]);
            if(rmc.EdidPreferredTimingFeedback != null)
                rmc.EdidPreferredTimingFeedback.LinkInputSig(trilist.StringInput[joinMap.EdidPrefferedTiming]);
            if(rmc.EdidSerialNumberFeedback != null)
                rmc.EdidSerialNumberFeedback.LinkInputSig(trilist.StringInput[joinMap.EdidSerialNumber]);
        }

        public class DmRmcControllerJoinMap : JoinMapBase
        {
            public uint IsOnline { get; set; }
            public uint CurrentOutputResolution { get; set; }
            public uint EdidManufacturer { get; set; }
            public uint EdidName { get; set; }
            public uint EdidPrefferedTiming { get; set; }
            public uint EdidSerialNumber { get; set; }

            public DmRmcControllerJoinMap()
            {
                // Digital
                IsOnline = 1;

                // Serial
                CurrentOutputResolution = 1;
                EdidManufacturer = 2;
                EdidName = 3;
                EdidPrefferedTiming = 4;
                EdidSerialNumber = 5;
            }

            public override void OffsetJoinNumbers(uint joinStart)
            {
                var joinOffset = joinStart - 1;

                IsOnline = IsOnline + joinOffset;
                CurrentOutputResolution = CurrentOutputResolution + joinOffset;
                EdidManufacturer = EdidManufacturer + joinOffset;
                EdidName = EdidName + joinOffset;
                EdidPrefferedTiming = EdidPrefferedTiming + joinOffset;
                EdidSerialNumber = EdidSerialNumber + joinOffset;
            }
        }
    }
}