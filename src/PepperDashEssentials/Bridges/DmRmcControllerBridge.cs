﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Bridges
{
    public static class DmRmcControllerApiExtensions
    {
        public static void LinkToApi(this DmRmcControllerBase rmc, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            DmRmcControllerJoinMap joinMap = new DmRmcControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmRmcControllerJoinMap>(joinMapSerialized);

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
    }
}