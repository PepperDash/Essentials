using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Defines the contract for IUiDisplayInfo
	/// </summary>
	public interface IUiDisplayInfo : IKeyed
	{
		/// <summary>
		/// Display UI Type
		/// </summary>
		uint DisplayUiType { get; }
	}
}