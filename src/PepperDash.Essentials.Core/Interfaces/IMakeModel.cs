using PepperDash.Core;

namespace PepperDash.Essentials.Core.Interfaces
{
    public interface IMakeModel : IKeyed
	{
		string DeviceMake { get; }
		string DeviceModel { get; }
	}
}