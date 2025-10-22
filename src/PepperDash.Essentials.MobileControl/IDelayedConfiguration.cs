using System;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Defines the contract for IDelayedConfiguration
    /// </summary>
    public interface IDelayedConfiguration
    {
        /// <summary>
        /// Event triggered when the configuration is ready. Used when Mobile Control is interacting with a SIMPL program.
        /// </summary>
        event EventHandler<EventArgs> ConfigurationIsReady;
    }
}