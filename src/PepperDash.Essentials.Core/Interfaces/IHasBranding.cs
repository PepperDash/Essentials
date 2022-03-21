namespace PepperDash.Essentials.Core.Interfaces
{
    public interface IHasBranding
    {
        bool BrandingEnabled { get; }
        void InitializeBranding(string roomKey);
    }
}
