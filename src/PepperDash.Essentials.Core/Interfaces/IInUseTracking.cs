namespace PepperDash.Essentials.Core.Interfaces
{
    // TODO: potentially anemic model here
	/// <summary>
	/// Defines a class that uses an InUseTracker
	/// </summary>
	public interface IInUseTracking
	{
		InUseTracking InUseTracker { get; }
	}
}