using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// 
    /// </summary>
    public static class IHasPowerControlExtensions
    {
        public static void LinkButtons(this IHasPowerControl dev, BasicTriList triList)
        {
            triList.SetSigFalseAction(101, dev.PowerOn);
            triList.SetSigFalseAction(102, dev.PowerOff);
            triList.SetSigFalseAction(103, dev.PowerToggle);

            var fbdev = dev as IHasPowerControlWithFeedback;
            if (fbdev != null)
            {
                fbdev.PowerIsOnFeedback.LinkInputSig(triList.BooleanInput[101]);
            }
        }

        public static void UnlinkButtons(this IHasPowerControl dev, BasicTriList triList)
        {
            triList.ClearBoolSigAction(101);
            triList.ClearBoolSigAction(102);
            triList.ClearBoolSigAction(103);

            var fbdev = dev as IHasPowerControlWithFeedback;
            if (fbdev != null)
            {
                fbdev.PowerIsOnFeedback.UnlinkInputSig(triList.BooleanInput[101]);
            }
        }
    }
}