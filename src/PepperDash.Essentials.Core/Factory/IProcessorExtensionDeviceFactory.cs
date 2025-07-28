namespace PepperDash.Essentials.Core.Factory
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
