namespace PepperDash.Essentials.Core.Usage
{
	/// <summary>
	/// Defines a class that uses an InUseTracker
	/// </summary>
	public interface IInUseTracking
	{
		InUseTracking InUseTracker { get; }
	}
}