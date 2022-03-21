using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// 
    /// </summary>
    public static class IChannelExtensions
    {
        public static void LinkButtons(this IChannel dev, BasicTriList triList)
        {
            triList.SetBoolSigAction(123, dev.ChannelUp);
            triList.SetBoolSigAction(124, dev.ChannelDown);
            triList.SetBoolSigAction(125, dev.LastChannel);
            triList.SetBoolSigAction(137, dev.Guide);
            triList.SetBoolSigAction(129, dev.Info);
            triList.SetBoolSigAction(134, dev.Exit);
        }

        public static void UnlinkButtons(this IChannel dev, BasicTriList triList)
        {
            triList.ClearBoolSigAction(123);
            triList.ClearBoolSigAction(124);
            triList.ClearBoolSigAction(125);
            triList.ClearBoolSigAction(137);
            triList.ClearBoolSigAction(129);
            triList.ClearBoolSigAction(134);
        }
    }
}