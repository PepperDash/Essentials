namespace PepperDash.Essentials.Core.Lighting
{
    /// <summary>
    /// Requirements for a device that implements master raise/lower
    /// </summary>
    public interface ILightingMasterRaiseLower
    {
        void MasterRaise();
        void MasterLower();
        void MasterRaiseLowerStop();
    }
}