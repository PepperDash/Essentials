using System;
using Newtonsoft.Json;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Describes a MobileControlSystemController
    /// </summary>
    public interface IMobileControl : IKeyed
    {
        void CreateMobileControlRoomBridge(IEssentialsRoom room, IMobileControl parent);

        void LinkSystemMonitorToAppServer();
    }

    /// <summary>
    /// Describes a MobileSystemController that accepts IEssentialsRoom
    /// </summary>
    public interface IMobileControl3 : IMobileControl
    {
        void CreateMobileControlRoomBridge(IEssentialsRoom room, IMobileControl parent);

        void SendMessageObject(object o);

        void AddAction(string key, object action);

        void RemoveAction(string key);

        void AddDeviceMessenger(IMobileControlMessenger messenger);

        bool CheckForDeviceMessenger(string key);
    }

    /// <summary>
    /// Describes a mobile control messenger
    /// </summary>
    public interface IMobileControlMessenger: IKeyed
    {
        IMobileControl3 AppServerController { get; }
        string MessagePath { get; }
        void RegisterWithAppServer(IMobileControl3 appServerController);
    }

    public interface IMobileControlResponseMessage
    {
        [JsonProperty("type")]
        string Type { get; }

        [JsonProperty("clientId")]
        object ClientId { get; }

        [JsonProperty("content")]
        object Content { get; }

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