using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.Devices
{
    // These two interfaces duplicate the ones in PowerInterfaces.cs (namespace PepperDash.Essentials.Core),
    // which IHasPowerCycleWithBattery already builds on. The compiler treats the two copies as unrelated
    // types, so a device implementing this copy is not matched by an `is` or OfType<> check against the
    // other. The PepperDash.Essentials.Core copies are canonical; these remain for one release so plugins
    // built against them keep loading, and will then be removed. See issue #1525.

    /// <summary>
    /// Obsolete duplicate of <see cref="PepperDash.Essentials.Core.IHasPowerCycle"/>. Implement that one instead.
    /// </summary>
    [Obsolete("Duplicate of PepperDash.Essentials.Core.IHasPowerCycle; implement that interface instead. This copy will be removed in a future release.")]
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

    /// <summary>
    /// Obsolete duplicate of <see cref="PepperDash.Essentials.Core.IHasControlledPowerOutlets"/>. Implement that one instead.
    /// </summary>
    [Obsolete("Duplicate of PepperDash.Essentials.Core.IHasControlledPowerOutlets; implement that interface instead. This copy will be removed in a future release.")]
    public interface IHasControlledPowerOutlets : IKeyName
    {
        /// <summary>
        /// Collection of IPduOutlets
        /// </summary>
#pragma warning disable CS0618 // Kept pointing at the obsolete sibling so existing implementers still compile
        ReadOnlyDictionary<int, IHasPowerCycle> PduOutlets { get; }
#pragma warning restore CS0618
    }
}
