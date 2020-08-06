using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Describes a MobileControlSystemController
    /// </summary>
    public interface IMobileControl : IKeyed
    {
        void CreateMobileControlRoomBridge(EssentialsRoomBase room);

        void LinkSystemMonitorToAppServer();
    }

    /// <summary>
    /// Describes a MobileControl Room Bridge
    /// </summary>
    public interface IMobileControlRoomBridge : IKeyed
    {
        event EventHandler<EventArgs> UserCodeChanged;

        IMobileControl Parent { get; }

        string UserCode { get; }

        string QrCodeUrl { get; }

        string McServerUrl { get; }

        string RoomName { get; }
    }
}