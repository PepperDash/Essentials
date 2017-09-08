using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Defines minimal volume control methods
	/// </summary>
	public interface IBasicVolumeControls
	{
		void VolumeUp(bool pressRelease);
		void VolumeDown(bool pressRelease);
		void MuteToggle();
	}

	/// <summary>
	/// Adds feedback and direct volume level set to IBasicVolumeControls
	/// </summary>
	public interface IBasicVolumeWithFeedback : IBasicVolumeControls
	{
		void SetVolume(ushort level);
		void MuteOn();
		void MuteOff();
		IntFeedback VolumeLevelFeedback { get; }
		BoolFeedback MuteFeedback { get; }
	}


	/// <summary>
	/// 
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
    /// Defines minimum volume controls for a codec device with dialing capabilities
    /// </summary>
    public interface ICodecAudio
    {
        // ICodecReceiveVolume
        // Break this out into 3 interfaces
        void SetReceiveVolume(ushort level);
        void ReceiveMuteOn();
        void ReceiveMuteOff();
        void ReceiveMuteToggle();
        IntFeedback ReceiveLevelFeedback { get; }
        BoolFeedback ReceiveMuteIsOnFeedback { get; }

        // ICodecTransmitVolume
        void SetTransmitVolume(ushort level);
        void TransmitMuteOn();
        void TransmitMuteOff();
        void TransmitMuteToggle();
        IntFeedback TransmitLevelFeedback { get; }
        BoolFeedback TransmitMuteIsOnFeedback { get; }

        // ICodecPrivacy
        void PrivacyModeOn();
        void PrivacyModeOff();
        void PrivacyModeToggle();
        BoolFeedback PrivacyModeIsOnFeedback { get; }
    }



	/// <summary>
	/// A class that implements this, contains a reference to an IBasicVolumeControls device.
	/// For example, speakers attached to an audio zone.  The speakers can provide reference
	/// to their linked volume control.
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