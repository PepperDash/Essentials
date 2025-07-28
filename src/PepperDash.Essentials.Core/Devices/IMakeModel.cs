using PepperDash.Core;


namespace PepperDash.Essentials.Core
{

	/// <summary>
	/// Defines the contract for device make and model information
	/// </summary>
	public interface IMakeModel : IKeyed
	{
		/// <summary>
		/// Gets the make of the device
		/// </summary>
		string DeviceMake { get; }

		/// <summary>
		/// Gets the model of the device
		/// </summary>
		string DeviceModel { get; }
	}
}