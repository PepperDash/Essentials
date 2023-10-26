using Crestron.SimplSharp;
using PepperDash.Core;

namespace PepperDash_Essentials_Core.Devices
{
    /// <summary>
    /// Interface for any device that contains a collection of IHasPowerReboot Devices
    /// </summary>
    public interface IHasControlledPowerOutlets : IKeyName
    {
        /// <summary>
        /// Collection of IPduOutlets
        /// </summary>
        ReadOnlyDictionary<int, IHasPowerCycle> PduOutlets { get; }

    }
}