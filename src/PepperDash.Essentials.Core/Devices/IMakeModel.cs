using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public interface IMakeModel : IKeyed
    {
        string DeviceMake { get; }
        string DeviceModel { get; }
    }
}