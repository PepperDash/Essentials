using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        void SendMessageObject(IMobileControlMessage o);

        void AddAction(string key, Action<JToken> action);

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

    public interface IMobileControlMessage
    {
        [JsonProperty("type")]
        string Type { get; }

        [JsonProperty("clientId", NullValueHandling = NullValueHandling.Ignore)]
        string ClientId { get; }

        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        JToken Content { get; }

    }

    /// <summary>
    /// Describes a MobileControl Room Bridge
    /// </summary>
    public interface IMobileControlRoomBridge : IKeyed
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
    }
}