namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines mute control methods and properties with feedback
  /// </summary>
  public interface IHasMuteControlWithFeedback : IHasMuteControl
  {
    /// <summary>
    /// MuteFeedback property
    /// </summary>
    BoolFeedback MuteFeedback { get; }

    /// <summary>
    /// MuteOn method
    /// </summary>
    void MuteOn();

    /// <summary>
    /// MuteOff method
    /// </summary>
    void MuteOff();
  }
}