using System;
using System.Collections.Generic;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.Recording
{
	/// <summary>
	/// Defines the contract for court session management including
	/// session lifecycle, recording control, case management, and status feedback.
	/// </summary>
	public interface ICourtSession
	{
		#region Session Lifecycle

		/// <summary>
		/// Create a new court session with the given title.
		/// </summary>
		bool CreateCourtSession(string title);

		/// <summary>
		/// Get the active court session payload and cache it locally.
		/// </summary>
		bool GetActiveCourtSession();

		/// <summary>
		/// Check whether an active court session exists.
		/// </summary>
		bool HasActiveCourtSession();

		/// <summary>
		/// Edit the active court session title.
		/// </summary>
		bool EditCourtSessionTitle(string title);

		/// <summary>
		/// Close the active court session.
		/// </summary>
		bool CloseCourtSession();

		/// <summary>
		/// Force close the active court session.
		/// </summary>
		bool ForceCloseCourtSession();

		#endregion

		#region Recording Control

		/// <summary>
		/// Start recording the active court session.
		/// </summary>
		bool StartRecording();

		/// <summary>
		/// Stop the current recording.
		/// </summary>
		bool StopRecording();

		/// <summary>
		/// Seal the current recording (protect/read-only).
		/// </summary>
		bool Seal();

		/// <summary>
		/// Unseal the current recording.
		/// </summary>
		bool Unseal();

		/// <summary>
		/// Get the current recording status from the API.
		/// </summary>
		bool GetRecordingStatus();

		/// <summary>
		/// Get the current seal status from the API.
		/// </summary>
		bool GetSealStatus();

		#endregion

	


		#region Status

		/// <summary>
		/// Get current session information (case ID and name).
		/// </summary>
		Dictionary<string, string> GetCurrentSessionInfo();


		/// <summary>
		/// Get the activation status from the API.
		/// </summary>
		bool GetActivationStatus();

		#endregion

		#region Feedbacks

		/// <summary>
		/// True when currently recording.
		/// </summary>
		BoolFeedback IsRecording { get; }

		/// <summary>
		/// True when the recording is sealed.
		/// </summary>
		BoolFeedback IsSealed { get; }

		/// <summary>
		/// Recording duration in milliseconds.
		/// </summary>
		StringFeedback RecordingDurationMs { get; }

		/// <summary>
		/// Recording duration formatted as HH:MM:SS.
		/// </summary>
		StringFeedback RecordingDurationFormatted { get; }

		/// <summary>
		/// Current case ID for the active session.
		/// </summary>
		StringFeedback CurrentCaseId { get; }

		/// <summary>
		/// Current case name for the active session.
		/// </summary>
		StringFeedback CurrentCaseName { get; }

		/// <summary>
		/// Current recording session state (Idle, Recording, Sealed, Disconnected, Error).
		/// </summary>
		StringFeedback CurrentState { get; }

		/// <summary>
		/// Error message if session is in error state.
		/// </summary>
		StringFeedback ErrorMessage { get; }

		/// <summary>
		/// API heartbeat counter for diagnostics.
		/// </summary>
		IntFeedback HeartbeatCounter { get; }

		/// <summary>
		/// True when CourtCapture reports an active court session.
		/// </summary>
		BoolFeedback HasActiveCourtSessionFeedback { get; }

		/// <summary>
		/// Last command result envelope as JSON string for UI binding.
		/// </summary>
		StringFeedback LastCommandResult { get; }

		#endregion

		#region Events

		/// <summary>
		/// Raised when case notes are updated.
		/// </summary>
		event EventHandler CaseNotesUpdated;

		/// <summary>
		/// Raised when quick links are updated.
		/// </summary>
		event EventHandler QuickLinksUpdated;

		#endregion
	}
}
