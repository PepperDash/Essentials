namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// Defines a device (e.g. a multiview/UVC compositor) whose multiview output can be enabled and
/// whose active layout can be observed. Implementing this interface (instead of requiring consumers
/// to reference the hardware-specific plugin type directly) lets other plugins (e.g. a room plugin)
/// turn the multiview on and read its current layout without taking a compile-time dependency on the
/// specific hardware plugin that implements it. The layout is <em>set</em> via
/// <see cref="IHasScreensWithLayouts.ApplyLayout(uint, uint)"/>.
/// </summary>
public interface IHasMultiviewControl
{
    /// <summary>
    /// Enables or disables the multiview output.
    /// </summary>
    /// <param name="enabled">True to enable multiview, false to disable.</param>
    void SetMultiviewEnabled(bool enabled);

    /// <summary>
    /// Feedback for whether the multiview output is currently enabled.
    /// </summary>
    BoolFeedback MultiviewEnabled { get; }

    /// <summary>
    /// Feedback for the current multiview layout index.
    /// </summary>
    IntFeedback MultiviewLayout { get; }
}
