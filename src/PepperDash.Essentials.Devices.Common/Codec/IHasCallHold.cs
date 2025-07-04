
namespace PepperDash.Essentials.Devices.Common.Codec;

/// <summary>
/// Defines the contract for devices that have call hold functionality
/// </summary>
public interface IHasCallHold
{
    /// <summary>
    /// Put the specified call on hold
    /// </summary>
    /// <param name="activeCall">The call to put on hold</param>
    void HoldCall(CodecActiveCallItem activeCall);

    /// <summary>
    /// Resume the specified call
    /// </summary>
    /// <param name="activeCall">The call to resume</param>
    void ResumeCall(CodecActiveCallItem activeCall);
}