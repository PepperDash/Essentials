using System;
using System.Linq;
using System.Collections.Generic;
using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
	/// <summary>
	/// Describes a device that has call participants
	/// </summary>
	public interface IHasParticipants
	{
		CodecParticipants Participants { get; }

        /// <summary>
        /// Removes the participant from the meeting
        /// </summary>
        /// <param name="participant"></param>
        void RemoveParticipant(int userId);

        /// <summary>
        /// Sets the participant as the new host
        /// </summary>
        /// <param name="participant"></param>
        void SetParticipantAsHost(int userId);

        /// <summary>
        /// Admits a participant from the waiting room
        /// </summary>
        /// <param name="userId"></param>
        void AdmitParticipantFromWaitingRoom(int userId);
	}

	/// <summary>
	/// Describes the ability to mute and unmute a participant's video in a meeting
	/// </summary>
	public interface IHasParticipantVideoMute : IHasParticipants
	{
		void MuteVideoForParticipant(int userId);
		void UnmuteVideoForParticipant(int userId);
		void ToggleVideoForParticipant(int userId);
	}

 /// <summary>
 /// Defines the contract for IHasParticipantAudioMute
 /// </summary>
	public interface IHasParticipantAudioMute : IHasParticipantVideoMute
	{
        /// <summary>
        /// Mute audio of all participants
        /// </summary>
        void MuteAudioForAllParticipants();

		void MuteAudioForParticipant(int userId);
		void UnmuteAudioForParticipant(int userId);
		void ToggleAudioForParticipant(int userId);
	}

 /// <summary>
 /// Defines the contract for IHasParticipantPinUnpin
 /// </summary>
	public interface IHasParticipantPinUnpin : IHasParticipants
	{
		IntFeedback NumberOfScreensFeedback { get; }
		int ScreenIndexToPinUserTo { get; }

		void PinParticipant(int userId, int screenIndex);
		void UnPinParticipant(int userId);
		void ToggleParticipantPinState(int userId, int screenIndex);
	}

 /// <summary>
 /// Represents a CodecParticipants
 /// </summary>
	public class CodecParticipants
	{
		private List<Participant> _currentParticipants;

		public List<Participant> CurrentParticipants
		{
			get { return _currentParticipants; }
			set
			{
				_currentParticipants = value;
				OnParticipantsChanged();
			}
		}

        public Participant Host
        {
            get
            {
                return _currentParticipants.FirstOrDefault(p => p.IsHost);
            }
        }

		public event EventHandler<EventArgs> ParticipantsListHasChanged;

		public CodecParticipants()
		{
			_currentParticipants = new List<Participant>();
		}

  /// <summary>
  /// OnParticipantsChanged method
  /// </summary>
		public void OnParticipantsChanged()
		{
			var handler = ParticipantsListHasChanged;

			if (handler == null) return;

			handler(this, new EventArgs());
		}
	}

 /// <summary>
 /// Represents a Participant
 /// </summary>
	public class Participant
	{
  /// <summary>
  /// Gets or sets the UserId
  /// </summary>
		public int UserId { get; set; }
  /// <summary>
  /// Gets or sets the IsHost
  /// </summary>
		public bool IsHost { get; set; }
        public bool IsMyself { get; set; }
		public string Name { get; set; }
		public bool CanMuteVideo { get; set; }
		public bool CanUnmuteVideo { get; set; }
		public bool VideoMuteFb { get; set; }
		public bool AudioMuteFb { get; set; }
  /// <summary>
  /// Gets or sets the HandIsRaisedFb
  /// </summary>
		public bool HandIsRaisedFb { get; set; }
  /// <summary>
  /// Gets or sets the IsPinnedFb
  /// </summary>
		public bool IsPinnedFb { get; set; }
  /// <summary>
  /// Gets or sets the ScreenIndexIsPinnedToFb
  /// </summary>
		public int ScreenIndexIsPinnedToFb { get; set; }

		public Participant()
		{
			// Initialize to -1 (no screen)
			ScreenIndexIsPinnedToFb = -1;
		}
	}
}