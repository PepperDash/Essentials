using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
	/// <summary>
	/// Defines a class that has warm up and cool down
	/// </summary>
	public interface IWarmingCooling
	{
		BoolFeedback IsWarmingUpFeedback { get; }
		BoolFeedback IsCoolingDownFeedback { get; }
	}
}