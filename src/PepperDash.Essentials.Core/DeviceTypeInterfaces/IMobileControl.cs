using System;
using System.Collections.ObjectModel;
using Crestron.SimplSharpPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Use this interface on a device or room if it uses custom Mobile Control messengers
    /// </summary>
    public interface ICustomMobileControl : IKeyed
    {
    }

    /*/// <summary>
    /// Describes a MobileControlSystemController
    /// </summary>
    public interface IMobileControl : IKeyed
    {
        void CreateMobileControlRoomBridge(IEssentialsRoom room, IMobileControl parent);

        void LinkSystemMonitorToAppServer();
    }*/

    /// <summary>
    /// Describes a MobileSystemController that accepts IEssentialsRoom
    /// </summary>
    public interface IMobileControl : IKeyed
    {
        string Host { get; }

        string ClientAppUrl { get; }

        string SystemUuid { get; }

        BoolFeedback ApiOnlineAndAuthorized { get; }

        void SendMessageObject(IMobileControlMessage o);

        void AddAction<T>(T messenger, Action<string, string, JToken> action) where T : IMobileControlMessenger;

        void RemoveAction(string key);

        void AddDeviceMessenger(IMobileControlMessenger messenger);

        bool CheckForDeviceMessenger(string key);

        IMobileControlRoomMessenger GetRoomMessenger(string key);

    }

    /// <summary>
    /// Describes a mobile control messenger
    /// </summary>
    public interface IMobileControlMessenger : IKeyed
    {
        IMobileControl AppServerController { get; }
        string MessagePath { get; }

        string DeviceKey { get; }
        void RegisterWithAppServer(IMobileControl appServerController);
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

    public interface IMobileControlAction
    {
        IMobileControlMessenger Messenger { get; }

        Action<string, string, JToken> Action { get; }
    }

    /// <summary>
    /// Describes a MobileControl Touchpanel Controller
    /// </summary>
    public interface IMobileControlTouchpanelController : IKeyed
    {
        /// <summary>
        /// The default room key for the controller
        /// </summary>
        string DefaultRoomKey { get; }

        /// <summary>
        /// Sets the application URL for the controller
        /// </summary>
        /// <param name="url">The application URL</param>
        void SetAppUrl(string url);

        /// <summary>
        /// Indicates whether the controller uses a direct server connection
        /// </summary>
        bool UseDirectServer { get; }

        /// <summary>
        /// Indicates whether the controller is a Zoom Room controller
        /// </summary>
        bool ZoomRoomController { get; }
    }

    /// <summary>
    /// Describes a MobileControl Crestron Touchpanel Controller
    /// This interface extends the IMobileControlTouchpanelController to include connected IP information
    /// </summary>
    public interface IMobileControlCrestronTouchpanelController : IMobileControlTouchpanelController
    {
        /// <summary>
        /// Gets a collection of connected IP information for the touchpanel controller
        /// </summary>
        ReadOnlyCollection<ConnectedIpInformation> ConnectedIps { get; }
    }
}