using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
 /// <summary>
 /// Defines the contract for IOnline
 /// </summary>
	public interface IOnline
	{
		BoolFeedback IsOnline { get; }
	}

 /// <summary>
 /// Defines the contract for IAttachVideoStatus
 /// </summary>
	public interface IAttachVideoStatus : IKeyed
	{
		// Extension methods will depend on this
	}

	/// <summary>
	/// For display classes that can provide usage data
	/// </summary>
	public interface IDisplayUsage
	{
		IntFeedback LampHours { get; }
	}

	public interface IMakeModel : IKeyed
	{
		string DeviceMake { get; }
		string DeviceModel { get; }
	}
}