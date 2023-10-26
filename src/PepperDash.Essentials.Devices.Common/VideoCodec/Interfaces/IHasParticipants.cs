using PepperDash.Core;

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
}