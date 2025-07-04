using PepperDash.Core;

namespace PepperDash.Essentials.Core;

	/// <summary>
	/// Describes things needed to show on UI
	/// </summary>
	public interface IUiDisplayInfo : IKeyed
	{
		uint DisplayUiType { get; }
	}