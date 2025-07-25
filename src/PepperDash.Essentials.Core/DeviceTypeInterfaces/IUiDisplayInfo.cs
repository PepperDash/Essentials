using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
 /// <summary>
 /// Defines the contract for IUiDisplayInfo
 /// </summary>
	public interface IUiDisplayInfo : IKeyed
	{
		uint DisplayUiType { get; }
	}
}