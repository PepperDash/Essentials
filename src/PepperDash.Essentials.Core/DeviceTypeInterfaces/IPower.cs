using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials.Core.Feedbacks;
using PepperDash.Essentials.Core.Touchpanels;


namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{

    /// <summary>
    /// Adds feedback for current power state
    /// </summary>
    public interface IHasPowerControlWithFeedback : IHasPowerControl
    {
        BoolFeedback PowerIsOnFeedback { get; }
    }

    /// <summary>
    /// Defines the ability to power a device on and off
    /// </summary>
    public interface IHasPowerControl
    {
        void PowerOn();
        void PowerOff();
        void PowerToggle();
    }

	/// <summary>
	/// 
	/// </summary>
	public static class IHasPowerControlExtensions
	{
        /// <summary>
        /// LinkButtons method
        /// </summary>
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

        /// <summary>
        /// UnlinkButtons method
        /// </summary>
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