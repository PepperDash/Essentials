namespace PepperDash.Essentials.Core
{
  /// <summary>
  /// Defines the contract for IFullAudioSettings
  /// </summary>
  public interface IFullAudioSettings : IBasicVolumeWithFeedback
  {
    void SetBalance(ushort level);
    void BalanceLeft(bool pressRelease);
    void BalanceRight(bool pressRelease);

    void SetBass(ushort level);
    void BassUp(bool pressRelease);
    void BassDown(bool pressRelease);

    void SetTreble(ushort level);
    void TrebleUp(bool pressRelease);
    void TrebleDown(bool pressRelease);

    bool hasMaxVolume { get; }
    void SetMaxVolume(ushort level);
    void MaxVolumeUp(bool pressRelease);
    void MaxVolumeDown(bool pressRelease);

    bool hasDefaultVolume { get; }
    void SetDefaultVolume(ushort level);
    void DefaultVolumeUp(bool pressRelease);
    void DefaultVolumeDown(bool pressRelease);

    void LoudnessToggle();
    void MonoToggle();

    BoolFeedback LoudnessFeedback { get; }
    BoolFeedback MonoFeedback { get; }
    IntFeedback BalanceFeedback { get; }
    IntFeedback BassFeedback { get; }
    IntFeedback TrebleFeedback { get; }
    IntFeedback MaxVolumeFeedback { get; }
    IntFeedback DefaultVolumeFeedback { get; }
  }
}