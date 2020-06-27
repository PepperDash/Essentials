using System.Collections.Generic;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Base class for rooms with more than a single display
    /// </summary>
    public abstract class EssentialsNDisplayRoomBase : EssentialsRoomBase, IHasMultipleDisplays
    {
        //public event SourceInfoChangeHandler CurrentSingleSourceChange;

        public Dictionary<eSourceListItemDestinationTypes, IRoutingSinkWithSwitching> Displays { get; protected set;}

        protected EssentialsNDisplayRoomBase(DeviceConfig config)
            : base (config)
        {
            Displays = new Dictionary<eSourceListItemDestinationTypes, IRoutingSinkWithSwitching>();

        }
    }
}