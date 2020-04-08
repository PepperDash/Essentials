using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_Core.DeviceTypeInterfaces
{
    public interface IMobileControl:IKeyed
    {
        void CreateMobileControlRoomBridge(EssentialsRoomBase room);
    }
}