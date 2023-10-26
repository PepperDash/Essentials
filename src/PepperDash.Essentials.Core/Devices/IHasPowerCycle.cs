using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_Core.Devices
{
    /// <summary>
    /// Interface for any device that is able to control it'spower and has a configurable reboot time
    /// </summary>
    public interface IHasPowerCycle : IKeyName, IHasPowerControlWithFeedback
    {
        /// <summary>
        /// Delay between power off and power on for reboot
        /// </summary>
        int PowerCycleTimeMs { get;}

        /// <summary>
        /// Reboot outlet
        /// </summary>
        void PowerCycle();
    }
}