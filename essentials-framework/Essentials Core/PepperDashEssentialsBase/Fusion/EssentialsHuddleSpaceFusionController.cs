using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Fusion;

namespace PepperDash_Essentials_Core.Fusion
{
    public class EssentialsHuddleSpaceFusionController:EssentialsHuddleSpaceFusionSystemControllerBase
    {

        public EssentialsHuddleSpaceFusionController(EssentialsHuddleSpaceRoom room, uint ipId) : base(room, ipId)
        {
        }
    }
}