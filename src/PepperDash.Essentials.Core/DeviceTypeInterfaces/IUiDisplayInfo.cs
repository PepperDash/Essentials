namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
 /// <summary>
 /// Defines the contract for IUiDisplayInfo
 /// </summary>
	public interface IUiDisplayInfo : IKeyed
	{
		uint DisplayUiType { get; }
	}
}