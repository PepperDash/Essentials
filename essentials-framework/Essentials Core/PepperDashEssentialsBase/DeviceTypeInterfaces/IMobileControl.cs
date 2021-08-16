using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Describes a MobileControlSystemController
    /// </summary>
    public interface IMobileControl : IKeyed
    {
        void CreateMobileControlRoomBridge(EssentialsRoomBase room, IMobileControl parent);

        void LinkSystemMonitorToAppServer();
    }

    /// <summary>
    /// Describes a MobileSystemController that accepts IEssentialsRoom
    /// </summary>
    public interface IMobileControl3 : IMobileControl
    {
        void CreateMobileControlRoomBridge(IEssentialsRoom room, IMobileControl parent);
    }

    /// <summary>
    /// Describes a MobileControl Room Bridge
    /// </summary>
    public interface IMobileControlRoomBridge : IKeyed
    {
        event EventHandler<EventArgs> UserCodeChanged;

        event EventHandler<EventArgs> UserPromptedForCode;

        event EventHandler<EventArgs> ClientJoined;

        string UserCode { get; }

        string QrCodeUrl { get; }

        string QrCodeChecksum { get; }

        string McServerUrl { get; }

        string RoomName { get; }
    }
}