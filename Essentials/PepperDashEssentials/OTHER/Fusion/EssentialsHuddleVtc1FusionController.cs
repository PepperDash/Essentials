using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.Devices.Common.Occupancy;

namespace PepperDash.Essentials.Fusion
{
    public class EssentialsHuddleVtc1FusionController : EssentialsHuddleSpaceFusionSystemControllerBase
    {
        public EssentialsHuddleVtc1FusionController(EssentialsHuddleSpaceRoom room, uint ipId)
            : base(room, ipId)
        {


        }
    }
}