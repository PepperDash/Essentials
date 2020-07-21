using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Fusion;

namespace PepperDash_Essentials_Core.Fusion
{
    public class EssentialsHuddleSpaceFusionController:EssentialsFusionSystemControllerBase
    {
        private EssentialsHuddleSpaceRoom _room;

        public EssentialsHuddleSpaceFusionController(EssentialsHuddleSpaceRoom room, uint ipId) : base(room, ipId)
        {
            _room = room;
            Initialize();
        }
    }
}