using System;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDashEssentials.UIDrivers.EssentialsDualDisplay;

namespace PepperDash.Essentials
{
    public class DualDisplaySimpleOrAdvancedRouting : PanelDriverBase
    {
        private readonly EssentialsDualDisplayPanelAvFunctionsDriver _parent;
        private EssentialsDualDisplayRoom _currentRoom;

        private SourceListItem PendingSource;

        public DualDisplaySimpleOrAdvancedRouting(EssentialsDualDisplayPanelAvFunctionsDriver parent)
            : base(parent.TriList)
        {
            _parent = parent;
            _currentRoom = _parent.CurrentRoom as EssentialsDualDisplayRoom;
        }

        
    }
}