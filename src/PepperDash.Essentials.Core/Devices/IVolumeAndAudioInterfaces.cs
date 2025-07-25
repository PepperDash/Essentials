using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Defines minimal volume and mute control methods
	/// </summary>
    public interface IBasicVolumeControls 
	{
		void VolumeUp(bool pressRelease);
		void VolumeDown(bool pressRelease);
		void MuteToggle();
	}

    /// <summary>
    /// Defines the contract for IHasVolumeControl
    /// </summary>
    public interface IHasVolumeControl
    {
        void VolumeUp(bool pressRelease);
        void VolumeDown(bool pressRelease);
    }

    /// <summary>
    /// Defines volume control methods and properties with feedback
    /// </summary>
    public interface IHasVolumeControlWithFeedback : IHasVolumeControl
    {
        void SetVolume(ushort level);
        IntFeedback VolumeLevelFeedback { get; }
    }

    /// <summary>
    /// Defines basic mute control methods
    /// </summary>
    public interface IHasMuteControl
    {
        void MuteToggle();
    }

    /// <summary>
    /// Defines mute control methods and properties with feedback
    /// </summary>
    public interface IHasMuteControlWithFeedback : IHasMuteControl
    {
        BoolFeedback MuteFeedback { get; }
        void MuteOn();
        void MuteOff();
    }

    /// <summary>
    /// Defines the contract for IBasicVolumeWithFeedback
    /// </summary>
    public interface IBasicVolumeWithFeedback : IBasicVolumeControls
	{
		BoolFeedback MuteFeedback { get; }
        void MuteOn();
        void MuteOff();
		void SetVolume(ushort level);
		IntFeedback VolumeLevelFeedback { get; }
	}

 /// <summary>
 /// Defines the contract for IBasicVolumeWithFeedbackAdvanced
 /// </summary>
	public interface IBasicVolumeWithFeedbackAdvanced : IBasicVolumeWithFeedback
	{
		int RawVolumeLevel { get; }

		eVolumeLevelUnits Units { get; }
	}

    public enum eVolumeLevelUnits
    {
        Decibels,
        Percent,
        Relative,
        Absolute
    }

    /// <summary>
    /// Defines the contract for IHasCurrentVolumeControls
    /// </summary>
    public interface IHasCurrentVolumeControls
    {
        IBasicVolumeControls CurrentVolumeControls { get; }
		event EventHandler<VolumeDeviceChangeEventArgs> CurrentVolumeDeviceChange;

        void SetDefaultLevels();

        bool ZeroVolumeWhenSwtichingVolumeDevices { get; }
	}


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

 /// <summary>
 /// Defines the contract for IHasVolumeDevice
 /// </summary>
	public interface IHasVolumeDevice
	{
		IBasicVolumeControls VolumeDevice { get; }
	}

	/// <summary>
	/// Identifies a device that contains audio zones
	/// </summary>
	public interface IAudioZones : IRouting
	{
		Dictionary<uint, IAudioZone> Zone { get; }
	}

	/// <summary>
	/// Defines minimum functionality for an audio zone
	/// </summary>
	public interface IAudioZone : IBasicVolumeWithFeedback
	{
		void SelectInput(ushort input);
	}
}