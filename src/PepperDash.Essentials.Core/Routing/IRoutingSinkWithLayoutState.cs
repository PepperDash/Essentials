using System;

namespace PepperDash.Essentials.Core;

/// <summary>
/// Extends <see cref="IRoutingSinkWithLayouts"/> for devices that can report the current shape of
/// their multiview canvas, along with the position/size, stacking order and routed source of every
/// tile, in a generic, product-agnostic form. This is intentionally decoupled from the routing
/// graph (nodes/edges/tie-lines) built from <see cref="IRoutingSinkWithLayouts.WindowTileSinks"/> -
/// it exists to support a separate visualization concern: a mock-up of what is actually displayed
/// on the monitor fed by the decoder, suitable for a React UI or the developer tools Routing page.
/// </summary>
public interface IRoutingSinkWithLayoutState : IRoutingSinkWithLayouts
{
    /// <summary>
    /// Gets the current multiview canvas/tile layout, or null if no layout is currently active or
    /// applicable (e.g. the device is not currently in a multiview mode).
    /// </summary>
    MultiviewLayoutState CurrentLayout { get; }

    /// <summary>
    /// Raised whenever the canvas shape, a tile's geometry/stacking order, or a tile's routed source
    /// changes.
    /// </summary>
    event EventHandler<MultiviewLayoutStateEventArgs> LayoutChanged;
}

/// <summary>
/// Event arguments for <see cref="IRoutingSinkWithLayoutState.LayoutChanged"/>.
/// </summary>
public class MultiviewLayoutStateEventArgs : EventArgs
{
    /// <summary>
    /// The current multiview layout state at the time this event was raised, or null if no layout
    /// is currently active.
    /// </summary>
    public MultiviewLayoutState CurrentLayout { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiviewLayoutStateEventArgs"/> class.
    /// </summary>
    public MultiviewLayoutStateEventArgs(MultiviewLayoutState currentLayout)
    {
        CurrentLayout = currentLayout;
    }
}
