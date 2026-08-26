using System;
using System.Collections.Generic;
using PepperDash.Core;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Describes a single named routing slot (input or output) on a device implementing
/// <see cref="IHasNamedRoutingSlots"/>.
/// </summary>
public interface IRoutingSlotInfo : IKeyName
{
    /// <summary>Matrix slot number.</summary>
    int SlotNumber { get; }

    /// <summary>Signal types this slot can carry.</summary>
    eRoutingSignalType SupportedSignalTypes { get; }
}

/// <summary>
/// Describes a named output slot, adding per-signal-type current-route feedback (keyed by the
/// routed input slot's key) that <see cref="IRoutingMidpointWithFeedback.CurrentRoutes"/> does not
/// carry.
/// </summary>
public interface IRoutingOutputSlotInfo : IRoutingSlotInfo
{
    /// <summary>The key of the input slot currently routed to this output, per signal type.</summary>
    IReadOnlyDictionary<eRoutingSignalType, string> CurrentRouteInputKeys { get; }

    /// <summary>Raised when the routed input on this output changes.</summary>
    event EventHandler OutputSlotChanged;
}

/// <summary>
/// Optional extension to <see cref="IRoutingMidpointWithFeedback"/> for matrix-style routing
/// devices that track named input/output slots (with per-signal-type routing feedback) internally,
/// e.g. plugin-local slot abstractions that replaced the removed core <c>IRoutingInputSlot</c>/
/// <c>IRoutingOutputSlot</c> during the v3 routing refactor. Devices implementing this get a richer
/// mobile-control matrix routing message (names + per-signal-type feedback) instead of the bare
/// key-only ports from <see cref="IRoutingMidpointWithFeedback"/> alone.
/// </summary>
public interface IHasNamedRoutingSlots : IRoutingMidpointWithFeedback
{
    /// <summary>Named input slots, keyed by slot key.</summary>
    IReadOnlyDictionary<string, IRoutingSlotInfo> InputSlots { get; }

    /// <summary>Named output slots, keyed by slot key.</summary>
    IReadOnlyDictionary<string, IRoutingOutputSlotInfo> OutputSlots { get; }
}
