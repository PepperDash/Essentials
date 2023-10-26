using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.PageManagers
{
    /// <summary>
    /// A simple class that hides and shows the default subpage for a given source type
    /// </summary>
    public class DefaultPageManager : PageManager
    {
        BasicTriList TriList;
        uint BackingPageJoin;

        public DefaultPageManager(IUiDisplayInfo device, BasicTriList trilist)
        {
            TriList         = trilist;
            BackingPageJoin = GetOffsetJoin(device.DisplayUiType) + 1;
        }

        public DefaultPageManager(uint join, BasicTriList trilist)
        {
            TriList         = trilist;
            BackingPageJoin = join;
        }

        public override void Show()
        {
            TriList.BooleanInput[BackingPageJoin].BoolValue = true;
        }

        public override void Hide()
        {
            TriList.BooleanInput[BackingPageJoin].BoolValue = false;
        }
    }
}