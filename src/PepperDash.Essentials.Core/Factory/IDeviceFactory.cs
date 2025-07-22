namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IDeviceFactory
    /// </summary>
    public interface IDeviceFactory
    {
        /// <summary>
        /// Loads all the types to the DeviceFactory
        /// </summary>
        void LoadTypeFactories();
    }
}