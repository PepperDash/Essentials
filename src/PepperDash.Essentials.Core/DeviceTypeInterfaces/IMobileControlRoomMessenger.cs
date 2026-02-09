using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
  /// <summary>
  /// Defines the contract for IMobileControlRoomMessenger
  /// </summary>
  public interface IMobileControlRoomMessenger : IKeyed
  {
    /// <summary>
    /// Raised when the user code changes
    /// </summary>
    event EventHandler<EventArgs> UserCodeChanged;

    /// <summary>
    /// Raised when the user is prompted for the code
    /// </summary>
    event EventHandler<EventArgs> UserPromptedForCode;

    /// <summary>
    /// Raised when a client joins the room
    /// </summary>
    event EventHandler<EventArgs> ClientJoined;

    /// <summary>
    /// Raised when the app url changes
    /// </summary>
    event EventHandler<EventArgs> AppUrlChanged;

    /// <summary>
    /// The user code for joining the room
    /// </summary>
    string UserCode { get; }

    /// <summary>
    /// The QR code URL for joining the room
    /// </summary>
    string QrCodeUrl { get; }

    /// <summary>
    /// The QR code checksum
    /// </summary>
    string QrCodeChecksum { get; }

    /// <summary>
    /// The Mobile Control server URL
    /// </summary>
    string McServerUrl { get; }

    /// <summary>
    /// The name of the room
    /// </summary>
    string RoomName { get; }

    /// <summary>
    /// The Mobile Control app URL
    /// </summary>
    string AppUrl { get; }

    /// <summary>
    /// Updates the url of the Mobile Control app 
    /// </summary>
    /// <param name="url">The new URL of the Mobile Control app</param>
    void UpdateAppUrl(string url);
  }
}