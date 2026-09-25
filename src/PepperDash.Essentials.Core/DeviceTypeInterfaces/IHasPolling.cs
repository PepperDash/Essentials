namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// Defines the contract for a device that can be asked to read its state on demand.
/// </summary>
/// <remarks>
/// For devices that report nothing unless asked. Such a device normally polls on a timer, and the
/// interval is a compromise: short enough to notice a change, long enough not to flood the device.
/// That compromise is worst immediately after a command, when something is known to have changed
/// and the next scheduled read may be seconds away. A consumer that has just sent a command can
/// use this to close that gap without the device polling faster all the time.
/// </remarks>
public interface IHasPolling
{
    /// <summary>
    /// Reads the device now, rather than waiting for its next scheduled poll.
    /// </summary>
    /// <remarks>
    /// Expected to block on I/O and to be called from a worker thread. An implementation should not
    /// overlap itself; where a device shares one connection across its own polling and its
    /// commands, an overlapping read queues behind them and arrives later, not sooner.
    /// </remarks>
    void Poll();
}
