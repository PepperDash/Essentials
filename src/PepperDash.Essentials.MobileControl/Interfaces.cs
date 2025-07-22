using System;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Defines the contract for IDelayedConfiguration
    /// </summary>
    public interface IDelayedConfiguration
    {


        event EventHandler<EventArgs> ConfigurationIsReady;
    }
}