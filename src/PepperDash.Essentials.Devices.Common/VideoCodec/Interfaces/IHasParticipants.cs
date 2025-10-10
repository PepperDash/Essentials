namespace PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces
{
	/// <summary>
	/// Describes a device that has call participants
	/// </summary>
	public interface IHasParticipants
	{
		/// <summary>
		/// Gets the collection of participants
		/// </summary>
		CodecParticipants Participants { get; }

		/// <summary>
		/// Removes the participant from the meeting
		/// </summary>
		/// <param name="userId"></param>
		void RemoveParticipant(int userId);

		/// <summary>
		/// Sets the participant as the new host
		/// </summary>
		/// <param name="userId"></param>
		void SetParticipantAsHost(int userId);

		/// <summary>
		/// Admits a participant from the waiting room
		/// </summary>
		/// <param name="userId"></param>
		void AdmitParticipantFromWaitingRoom(int userId);
	}
}