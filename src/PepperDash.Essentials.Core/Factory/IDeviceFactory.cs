namespace PepperDash.Essentials.Core.Factory
{
    /// <summary>
    /// Defines a class that is capable of loading device types
    /// </summary>
    public interface IDeviceFactory
    {
        /// <summary>
        /// Loads all the types to the DeviceFactory
        /// </summary>
        void LoadTypeFactories();
    }
}