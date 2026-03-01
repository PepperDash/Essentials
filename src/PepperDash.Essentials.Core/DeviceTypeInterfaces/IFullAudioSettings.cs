namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines the contract for IFullAudioSettings
  /// </summary>
  public interface IFullAudioSettings : IBasicVolumeWithFeedback
  {
    /// <summary>
    /// SetBalance method
    /// </summary>
    /// <param name="level">level to set</param>
    void SetBalance(ushort level);

    /// <summary>
    /// BalanceLeft method
    /// </summary>
    /// <param name="pressRelease">determines if the button is pressed or released</param>
    void BalanceLeft(bool pressRelease);

    /// <summary>
    /// BalanceRight method
    /// </summary>
    /// <param name="pressRelease">determines if the button is pressed or released</param>
    void BalanceRight(bool pressRelease);

    /// <summary>
    /// SetBass method
    /// </summary>
    /// <param name="level">level to set</param>
    void SetBass(ushort level);

    /// <summary>
    /// BassUp method
    /// </summary>
    /// <param name="pressRelease">determines if the button is pressed or released</param>
    void BassUp(bool pressRelease);

    /// <summary>
    /// BassDown method
    /// </summary>
    /// <param name="pressRelease">determines if the button is pressed or released</param>
    void BassDown(bool pressRelease);

    /// <summary>
    /// SetTreble method
    /// </summary>
    /// <param name="level">level to set</param>
    void SetTreble(ushort level);

    /// <summary>
    /// TrebleUp method
    /// </summary>
    /// <param name="pressRelease">determines if the button is pressed or released</param>
    void TrebleUp(bool pressRelease);

    /// <summary>
    /// TrebleDown method
    /// </summary>
    /// <param name="pressRelease">determines if the button is pressed or released</param>
    void TrebleDown(bool pressRelease);

    /// <summary>
    /// hasMaxVolume property
    /// </summary>
    bool hasMaxVolume { get; }

    /// <summary>
    /// SetMaxVolume method
    /// </summary>
    /// <param name="level">level to set</param>
    void SetMaxVolume(ushort level);

    /// <summary>
    /// MaxVolumeUp method
    /// </summary>
    /// <param name="pressRelease">determines if the button is pressed or released</param>
    void MaxVolumeUp(bool pressRelease);

    /// <summary>
    /// MaxVolumeDown method
    /// </summary>
    /// <param name="pressRelease">determines if the button is pressed or released</param>
    void MaxVolumeDown(bool pressRelease);

    /// <summary>
    /// hasDefaultVolume property
    /// </summary>
    bool hasDefaultVolume { get; }

    /// <summary>
    /// SetDefaultVolume method
    /// </summary>
    /// <param name="level">level to set</param>
    void SetDefaultVolume(ushort level);

    /// <summary>
    /// DefaultVolumeUp method
    /// </summary>
    /// <param name="pressRelease">determines if the button is pressed or released</param>
    void DefaultVolumeUp(bool pressRelease);

    /// <summary>
    /// DefaultVolumeDown method
    /// </summary>
    /// <param name="pressRelease">determines if the button is pressed or released</param>
    void DefaultVolumeDown(bool pressRelease);

    /// <summary>
    /// LoudnessToggle method
    /// </summary>
    void LoudnessToggle();

    /// <summary>
    /// MonoToggle method
    /// </summary>
    void MonoToggle();

    /// <summary>
    /// LoudnessFeedback property
    /// </summary>
    BoolFeedback LoudnessFeedback { get; }

    /// <summary>
    /// MonoFeedback property
    /// </summary>
    BoolFeedback MonoFeedback { get; }

    /// <summary>
    /// BalanceFeedback property
    /// </summary>
    IntFeedback BalanceFeedback { get; }

    /// <summary>
    /// BassFeedback property
    /// </summary>
    IntFeedback BassFeedback { get; }

    /// <summary>
    /// TrebleFeedback property
    /// </summary>
    IntFeedback TrebleFeedback { get; }

    /// <summary>
    /// MaxVolumeFeedback property
    /// </summary>
    IntFeedback MaxVolumeFeedback { get; }

    /// <summary>
    /// DefaultVolumeFeedback property
    /// </summary>
    IntFeedback DefaultVolumeFeedback { get; }
  }
}