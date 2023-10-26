using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core
{
    public static class ISetTopBoxControlsExtensions
    {
        public static void LinkButtons(this ISetTopBoxControls dev, BasicTriList triList)
        {
            triList.SetBoolSigAction(136, dev.DvrList);
            triList.SetBoolSigAction(152, dev.Replay);
        }

        public static void UnlinkButtons(this ISetTopBoxControls dev, BasicTriList triList)
        {
            triList.ClearBoolSigAction(136);
            triList.ClearBoolSigAction(152);
        }
    }
}