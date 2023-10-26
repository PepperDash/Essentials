namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Describes a MobileSystemController that accepts IEssentialsRoom
    /// </summary>
    public interface IMobileControl3 : IMobileControl
    {
        void CreateMobileControlRoomBridge(IEssentialsRoom room, IMobileControl parent);
    }
}