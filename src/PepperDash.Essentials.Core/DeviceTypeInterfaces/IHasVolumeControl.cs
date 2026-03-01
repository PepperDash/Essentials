namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines the contract for IHasVolumeControl
  /// </summary>
  public interface IHasVolumeControl
  {
    /// <summary>
    /// VolumeUp method
    /// </summary>
    /// <param name="pressRelease">determines if the volume up command is a press or release action</param>
    void VolumeUp(bool pressRelease);

    /// <summary>
    /// VolumeDown method
    /// </summary>
    /// <param name="pressRelease">determines if the volume down command is a press or release action</param>
    void VolumeDown(bool pressRelease);
  }
}