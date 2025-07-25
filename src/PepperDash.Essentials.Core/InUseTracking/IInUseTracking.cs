namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Defines a class that uses an InUseTracker
	/// </summary>
	public interface IInUseTracking
	{
		InUseTracking InUseTracker { get; }
	}
}