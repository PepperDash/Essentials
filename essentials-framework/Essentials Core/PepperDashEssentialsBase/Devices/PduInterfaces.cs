using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_Core.Devices
{
    /// <summary>
    /// Interface for any device that is able to control it'spower and has a configurable reboot time
    /// </summary>
    public interface IHasPowerReboot : IKeyName, IHasPowerControlWithFeedback
    {
        /// <summary>
        /// Delay between power off and power on for reboot
        /// </summary>
        int PowerRebootTimeMs { get;}

        /// <summary>
        /// Reboot outlet
        /// </summary>
        void PowerReboot();
    }

    /// <summary>
    /// Interface for any device that contains a collection of IHasPowerReboot Devices
    /// </summary>
    public interface IHasControlledPowerOutlets : IKeyName
    {
        /// <summary>
        /// Collection of IPduOutlets
        /// </summary>
        Dictionary<int, IHasPowerReboot> PduOutlets { get; }
 
        /// <summary>
        /// Count of PduOutlets
        /// </summary>
        int PduOutletsCount { get; }

    }
}