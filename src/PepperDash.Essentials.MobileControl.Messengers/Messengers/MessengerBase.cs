using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp.Net;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge
    /// </summary>
    public abstract class MessengerBase : EssentialsDevice, IMobileControlMessengerWithSubscriptions
    {
        /// <summary>
        /// The device this messenger is associated with
        /// </summary>
        protected IKeyName _device;

        /// <summary>
        /// Enable subscriptions
        /// </summary>
        protected bool enableMessengerSubscriptions;

        /// <summary>
        /// List of clients subscribed to this messenger
        /// </summary>
        /// <remarks>
        /// Unsoliciited feedback from a device in a messenger will ONLY be sent to devices in this subscription list. When a client disconnects, it's ID will be removed from the collection.
        /// </remarks>
        private readonly HashSet<string> subscriberIds = new HashSet<string>();

        /// <summary>
        /// Lock object for thread-safe access to SubscriberIds
        /// </summary>
        private readonly object _subscriberLock = new object();

        private readonly List<string> _deviceInterfaces;

        private readonly Dictionary<string, Action<string, JToken>> _actions = new Dictionary<string, Action<string, JToken>>();

        /// <summary>
        /// Gets the DeviceKey
        /// </summary>
        public string DeviceKey => _device?.Key ?? "";


        /// <summary>
        /// Gets or sets the AppServerController
        /// </summary>
        public IMobileControl AppServerController { get; private set; }

        /// <summary>
        /// Gets or sets the MessagePath
        /// </summary>
        public string MessagePath { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        protected MessengerBase(string key, string messagePath)
            : base(key)
        {
            Key = key;

            if (string.IsNullOrEmpty(messagePath))
                throw new ArgumentException("messagePath must not be empty or null");

            MessagePath = messagePath;
        }

        /// <summary>
        /// Constructor for a messenger associated with a device
        /// </summary>
        /// <param name="key"></param>
        /// <param name="messagePath"></param>
        /// <param name="device"></param>
        protected MessengerBase(string key, string messagePath, IKeyName device)
            : this(key, messagePath)
        {
            _device = device;

            _deviceInterfaces = GetInterfaces(_device as Device);
        }

        /// <summary>
        /// Gets the interfaces implmented on the device
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private List<string> GetInterfaces(Device device)
        {
            return device?.GetType().GetInterfaces().Select((i) => i.Name).ToList() ?? new List<string>();
        }

        /// <summary>
        /// Registers this messenger with appserver controller
        /// </summary>
        /// <param name="appServerController"></param>
        public void RegisterWithAppServer(IMobileControl appServerController)
        {
            AppServerController = appServerController ?? throw new ArgumentNullException("appServerController");

            AppServerController.AddAction(this, HandleMessage);

            RegisterActions();
        }

        /// <summary>
        /// Register this messenger with appserver controller
        /// </summary>
        /// <param name="appServerController">Parent controller for this messenger</param>
        /// <param name="enableMessengerSubscriptions">Enable subscriptions</param>
        public void RegisterWithAppServer(IMobileControl appServerController, bool enableMessengerSubscriptions)
        {
            this.enableMessengerSubscriptions = enableMessengerSubscriptions;
            AppServerController = appServerController ?? throw new ArgumentNullException("appServerController");

            AppServerController.AddAction(this, HandleMessage);

            RegisterActions();
        }

        private void HandleMessage(string path, string id, JToken content)
        {
            // replace base path with empty string. Should leave something like /fullStatus
            var route = path.Replace(MessagePath, string.Empty);

            if (!_actions.TryGetValue(route, out var action))
            {
                return;
            }

            this.LogDebug("Executing action for path {path}", path);

            action(id, content);
        }

        /// <summary>
        /// Adds an action for a given path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="action"></param>
        protected void AddAction(string path, Action<string, JToken> action)
        {
            if (_actions.ContainsKey(path))
            {
                return;
            }

            _actions.Add(path, action);
        }

        /// <summary>
        /// GetActionPaths method
        /// </summary>
        public List<string> GetActionPaths()
        {
            return _actions.Keys.ToList();
        }

        /// <summary>
        /// Removes an action for a given path
        /// </summary>
        /// <param name="path"></param>
        protected void RemoveAction(string path)
        {
            if (!_actions.ContainsKey(path))
            {
                return;
            }

            _actions.Remove(path);
        }

        /// <summary>
        /// Implemented in extending classes. Wire up API calls and feedback here
        /// </summary>
        protected virtual void RegisterActions()
        {

        }

        /// <summary>
        /// Add client to the susbscription list for unsolicited feedback
        /// </summary>
        /// <param name="clientId">Client ID to add</param>
        protected void SubscribeClient(string clientId)
        {
            if (!enableMessengerSubscriptions)
            {
                return;
            }

            lock (_subscriberLock)
            {
                if (!subscriberIds.Add(clientId))
                {
                    this.LogVerbose("Client {clientId} already subscribed", clientId);
                    return;
                }
            }

            this.LogDebug("Client {clientId} subscribed", clientId);
        }

        /// <summary>
        /// Remove a client from the subscription list
        /// </summary>
        /// <param name="clientId">Client ID to remove</param>
        public void UnsubscribeClient(string clientId)
        {
            if (!enableMessengerSubscriptions)
            {
                return;
            }

            bool wasSubscribed;
            lock (_subscriberLock)
            {
                wasSubscribed = subscriberIds.Contains(clientId);
                if (wasSubscribed)
                {
                    subscriberIds.Remove(clientId);
                }
            }

            if (!wasSubscribed)
            {
                this.LogVerbose("Client with ID {clientId} is not subscribed", clientId);
                return;
            }

            this.LogDebug("Client with ID {clientId} unsubscribed", clientId);
        }

        /// <summary>
        /// Helper for posting status message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="clientId">Optional client id that will direct the message back to only that client</param>
        protected void PostStatusMessage(DeviceStateMessageBase message, string clientId = null)
        {
            try
            {
                if (message == null)
                {
                    throw new ArgumentNullException("message");
                }

                if (_device == null)
                {
                    throw new ArgumentNullException("device");
                }

                message.SetInterfaces(_deviceInterfaces);

                message.Key = _device.Key;

                message.Name = _device.Name;

                var token = JToken.FromObject(message);

                PostStatusMessage(token, MessagePath, clientId);
            }
            catch (Exception ex)
            {
                this.LogError("Exception posting status message for {messagePath} to {clientId}: {message}", MessagePath, clientId ?? "all clients", ex.Message);
                this.LogDebug(ex, "Stack trace: ");
            }
        }

        /// <summary>
        /// Helper for posting status message
        /// </summary>
        /// <param name="type"></param>
        /// <param name="deviceState"></param>
        /// <param name="clientId">Optional client id that will direct the message back to only that client</param>
        protected void PostStatusMessage(string type, DeviceStateMessageBase deviceState, string clientId = null)
        {
            try
            {
                //Debug.Console(2, this, "*********************Setting DeviceStateMessageProperties on MobileControlResponseMessage");
                deviceState.SetInterfaces(_deviceInterfaces);

                deviceState.Key = _device.Key;

                deviceState.Name = _device.Name;

                deviceState.MessageBasePath = MessagePath;

                var token = JToken.FromObject(deviceState);

                PostStatusMessage(token, type, clientId);
            }
            catch (Exception ex)
            {
                this.LogError("Exception posting status message for {type} to {clientId}: {message}", type, clientId ?? "all clients", ex.Message);
                this.LogDebug(ex, "Stack trace: ");
            }
        }

        /// <summary>
        /// Helper for posting status message
        /// </summary>
        /// <param name="content"></param>
        /// <param name="type"></param>
        /// <param name="clientId">Optional client id that will direct the message back to only that client</param>
        protected void PostStatusMessage(JToken content, string type = "", string clientId = null)
        {
            try
            {
                // Allow for legacy method to continue without subscriptions
                if (!enableMessengerSubscriptions)
                {
                    AppServerController?.SendMessageObject(new MobileControlMessage { Type = !string.IsNullOrEmpty(type) ? type : MessagePath, ClientId = clientId, Content = content });
                    return;
                }

                // handle subscription feedback
                // If client is null or empty, this message is unsolicited feedback. Iterate through the subscriber list and send to all interested parties
                if (string.IsNullOrEmpty(clientId))
                {
                    // Create a snapshot of subscribers to avoid collection modification during iteration
                    List<string> subscriberSnapshot;
                    lock (_subscriberLock)
                    {
                        subscriberSnapshot = new List<string>(subscriberIds);
                    }

                    foreach (var client in subscriberSnapshot)
                    {
                        AppServerController?.SendMessageObject(new MobileControlMessage { Type = !string.IsNullOrEmpty(type) ? type : MessagePath, ClientId = client, Content = content });
                    }

                    return;
                }

                SubscribeClient(clientId);

                AppServerController?.SendMessageObject(new MobileControlMessage { Type = !string.IsNullOrEmpty(type) ? type : MessagePath, ClientId = clientId, Content = content });
            }
            catch (Exception ex)
            {
                this.LogError("Exception posting status message: {message}", ex.Message);
                this.LogDebug(ex, "Stack Trace: ");
            }
        }

        /// <summary>
        /// Helper for posting event message
        /// </summary>
        /// <param name="message"></param>
        protected void PostEventMessage(DeviceEventMessageBase message)
        {
            message.Key = _device.Key;

            message.Name = _device.Name;

            AppServerController?.SendMessageObject(new MobileControlMessage
            {
                Type = $"/event{MessagePath}/{message.EventType}",
                Content = JToken.FromObject(message),
            });
        }

        /// <summary>
        /// Helper for posting event message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventType"></param>
        protected void PostEventMessage(DeviceEventMessageBase message, string eventType)
        {
            message.Key = _device.Key;

            message.Name = _device.Name;

            message.EventType = eventType;

            AppServerController?.SendMessageObject(new MobileControlMessage
            {
                Type = $"/event{MessagePath}/{eventType}",
                Content = JToken.FromObject(message),
            });
        }

        /// <summary>
        /// Helper for posting event message with no content
        /// </summary>
        /// <param name="eventType"></param>
        protected void PostEventMessage(string eventType)
        {
            AppServerController?.SendMessageObject(new MobileControlMessage
            {
                Type = $"/event{MessagePath}/{eventType}",
                Content = JToken.FromObject(new { }),
            });
        }

    }
}
