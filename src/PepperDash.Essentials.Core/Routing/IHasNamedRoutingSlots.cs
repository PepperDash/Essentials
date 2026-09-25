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
/// Optional per-endpoint status for a named input slot, surfaced to mobile control (online state,
/// video sync detection, and the backing transmitter device key). A slot may implement this in
/// addition to <see cref="IRoutingSlotInfo"/>; the mobile-control matrix messenger emits these
/// fields when present so the UI can show source availability/sync. Endpoint status is a
/// plugin/device concern, so it is kept off the bare <see cref="IRoutingSlotInfo"/> contract.
/// </summary>
public interface IRoutingInputSlotInfo : IRoutingSlotInfo
{
    /// <summary>Key of the transmitter device feeding this input slot, empty if none/unknown.</summary>
    string TxDeviceKey { get; }

    /// <summary>Online feedback for the backing endpoint.</summary>
    BoolFeedback IsOnline { get; }

    /// <summary>Whether the input endpoint currently detects a video sync/signal.</summary>
    bool VideoSyncDetected { get; }

    /// <summary>Raised when <see cref="VideoSyncDetected"/> changes.</summary>
    event EventHandler VideoSyncChanged;
}

/// <summary>
/// Optional per-endpoint status for a named output slot, surfaced to mobile control (online state
/// and the backing receiver device key).
/// </summary>
public interface IRoutingOutputSlotStatus : IRoutingOutputSlotInfo
{
    /// <summary>Key of the receiver device fed by this output slot, empty if none/unknown.</summary>
    string RxDeviceKey { get; }

    /// <summary>Online feedback for the backing endpoint.</summary>
    BoolFeedback IsOnline { get; }
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
