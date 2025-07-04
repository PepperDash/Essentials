namespace PepperDash.Essentials.Core;

/// <summary>
/// Represents a routing device (typically a transmitter or source) that provides numeric feedback for its current route.
/// Extends <see cref="IRoutingNumeric"/>.
/// </summary>
public interface ITxRouting : IRoutingNumeric
{
    /// <summary>
    /// Feedback indicating the currently routed video source by its numeric identifier.
    /// </summary>
    IntFeedback VideoSourceNumericFeedback { get; }
    /// <summary>
    /// Feedback indicating the currently routed audio source by its numeric identifier.
    /// </summary>
    IntFeedback AudioSourceNumericFeedback { get; }
}