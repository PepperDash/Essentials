using System;
using Newtonsoft.Json.Linq;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{

    /// <summary>
    /// Defines the contract for IMobileControl
    /// </summary>
    public interface IMobileControl : IKeyed
    {
        /// <summary>
        /// Gets the Host
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Gets the Client App URL
        /// </summary>
        string ClientAppUrl { get; }

        /// <summary>
        /// Gets the System UUID
        /// </summary>
        string SystemUuid { get; }

        /// <summary>
        /// Gets the ApiOnlineAndAuthorized feedback
        /// </summary>
        BoolFeedback ApiOnlineAndAuthorized { get; }

        /// <summary>
        /// Sends the message object to the AppServer
        /// </summary>
        /// <param name="o">Message to send</param>
        void SendMessageObject(IMobileControlMessage o);

        /// <summary>
        /// Adds an action for a messenger
        /// </summary>
        /// <typeparam name="T">Messenger type. Must implement IMobileControlMessenger</typeparam>
        /// <param name="messenger">messenger to register</param>
        /// <param name="action">action to add</param>
        void AddAction<T>(T messenger, Action<string, string, JToken> action) where T : IMobileControlMessenger;

        /// <summary>
        /// Removes an action for a messenger
        /// </summary>
        /// <param name="key">key for action</param>
        void RemoveAction(string key);

        /// <summary>
        /// Adds a device messenger
        /// </summary>
        /// <param name="messenger">Messenger to add</param>
        void AddDeviceMessenger(IMobileControlMessenger messenger);

        /// <summary>
        /// Check if a device messenger exists
        /// </summary>
        /// <param name="key">Messenger key to find</param>
        bool CheckForDeviceMessenger(string key);

        /// <summary>
        /// Get a Room Messenger by key
        /// </summary>
        /// <param name="key">messenger key to find</param>
        /// <returns>Messenger if found, null otherwise</returns>
        IMobileControlRoomMessenger GetRoomMessenger(string key);
    }
}