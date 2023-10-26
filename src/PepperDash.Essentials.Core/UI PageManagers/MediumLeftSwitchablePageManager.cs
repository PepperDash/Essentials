using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.PageManagers
{
    /// <summary>
    /// A page manager for a page with backing panel and a switchable side panel
    /// </summary>
    public abstract class MediumLeftSwitchablePageManager : PageManager
    {
        protected BasicTriListWithSmartObject TriList;
        protected uint LeftSubpageJoin;
        protected uint BackingPageJoin;
        protected uint[] AllLeftSubpages;
        protected uint DisplayUiType;

        protected MediumLeftSwitchablePageManager(uint displayUiType)
        {
            DisplayUiType = displayUiType;
        }

        protected void InterlockLeftSubpage(uint join)
        {
            join = join + GetOffsetJoin();
            ClearLeftInterlock();
            TriList.BooleanInput[join].BoolValue = true;
            LeftSubpageJoin                      = join;
        }

        protected void ClearLeftInterlock()
        {
            foreach (var p in AllLeftSubpages)
                TriList.BooleanInput[GetOffsetJoin() + p].BoolValue = false;
        }

        protected uint GetOffsetJoin()
        {
            return GetOffsetJoin(DisplayUiType);
        }
    }
}