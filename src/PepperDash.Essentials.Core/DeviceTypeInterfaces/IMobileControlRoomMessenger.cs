using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
  /// <summary>
  /// Defines the contract for IMobileControlRoomMessenger
  /// </summary>
  public interface IMobileControlRoomMessenger : IKeyed
  {
    event EventHandler<EventArgs> UserCodeChanged;

    event EventHandler<EventArgs> UserPromptedForCode;

    event EventHandler<EventArgs> ClientJoined;

    event EventHandler<EventArgs> AppUrlChanged;

    string UserCode { get; }

    string QrCodeUrl { get; }

    string QrCodeChecksum { get; }

    string McServerUrl { get; }

    string RoomName { get; }

    string AppUrl { get; }

    void UpdateAppUrl(string url);
  }
}