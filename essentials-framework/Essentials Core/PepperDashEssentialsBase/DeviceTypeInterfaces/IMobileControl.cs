using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    public interface IMobileControl:IKeyed
    {
        void CreateMobileControlRoomBridge(EssentialsRoomBase room);

        void LinkSystemMonitorToAppServer();
    }
}