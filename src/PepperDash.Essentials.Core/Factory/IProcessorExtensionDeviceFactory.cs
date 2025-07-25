namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IProcessorExtensionDeviceFactory
    /// </summary>
    public interface IProcessorExtensionDeviceFactory
    {
        /// <summary>
        /// Loads all the extension factories to the ProcessorExtensionDeviceFactory
        /// </summary>
        void LoadFactories();
    }
}
