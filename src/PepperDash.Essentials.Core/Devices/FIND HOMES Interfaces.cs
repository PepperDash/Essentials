using PepperDash.Core;


namespace PepperDash.Essentials.Core
{
	public interface IOnline
	{
		BoolFeedback IsOnline { get; }
	}

	/// <summary>
	/// Describes a device that can have a video sync providing device attached to it
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