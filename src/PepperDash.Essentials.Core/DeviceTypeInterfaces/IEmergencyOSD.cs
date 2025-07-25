namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for IEmergencyOSD
    /// </summary>
    public interface IEmergencyOSD
    {
        void ShowEmergencyMessage(string url);
        void HideEmergencyMessage();
    }
}
