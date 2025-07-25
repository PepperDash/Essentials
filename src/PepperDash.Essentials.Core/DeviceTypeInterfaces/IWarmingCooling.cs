namespace PepperDash.Essentials.Core
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