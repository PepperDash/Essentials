namespace PepperDash_Essentials_Core.DeviceTypeInterfaces
{
    public interface IHasBranding
    {
        bool BrandingEnabled { get; }
        void InitializeBranding(string roomKey);
    }
}