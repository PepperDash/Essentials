using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge between mobile control clients and Essentials devices.
    /// This abstract base class handles message routing, action registration, and status updates.
    /// </summary>
    public abstract class MessengerBase : EssentialsDevice, IMobileControlMessenger
    {
        /// <summary>
        /// The device that this messenger is associated with
        /// </summary>
        protected IKeyName _device;

        /// <summary>
        /// List of interfaces implemented by the associated device
        /// </summary>
        private readonly List<string> _deviceInterfaces;

        /// <summary>
        /// Dictionary of registered actions, keyed by path
        /// </summary>
        private readonly Dictionary<string, Action<string, JToken>> _actions = new Dictionary<string, Action<string, JToken>>();

        /// <summary>
        /// Gets the key of the associated device
        /// </summary>
        public string DeviceKey => _device?.Key ?? "";

        /// <summary>
        /// Gets the mobile control app server controller
        /// </summary>
        public IMobileControl AppServerController { get; private set; }

        /// <summary>
        /// Gets the message path for this messenger
        /// </summary>
        public string MessagePath { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MessengerBase class
        /// </summary>
        /// <param name="key">The unique key for this messenger</param>
        /// <param name="messagePath">The message path for routing messages</param>
        /// <exception cref="ArgumentException">Thrown when messagePath is null or empty</exception>
        protected MessengerBase(string key, string messagePath)
            : base(key)
        {
            Key = key;

            if (string.IsNullOrEmpty(messagePath))
                throw new ArgumentException("messagePath must not be empty or null");

            MessagePath = messagePath;
        }

        /// <summary>
        /// Initializes a new instance of the MessengerBase class with an associated device
        /// </summary>
        /// <param name="key">The unique key for this messenger</param>
        /// <param name="messagePath">The message path for routing messages</param>
        /// <param name="device">The device to associate with this messenger</param>
        protected MessengerBase(string key, string messagePath, IKeyName device)
            : this(key, messagePath)
        {
            _device = device;

            _deviceInterfaces = GetInterfaces(_device as Device);
        }

        /// <summary>
        /// Gets the interfaces implented on the device
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

        private void HandleMessage(string path, string id, JToken content)
        {
            // replace base path with empty string. Should leave something like /fullStatus
            var route = path.Replace(MessagePath, string.Empty);

            if (!_actions.TryGetValue(route, out var action))
            {
                return;
            }

            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Executing action for path {path}", this, path);

            action(id, content);
        }

        /// <summary>
        /// Adds an action to be executed when a message is received at the specified path
        /// </summary>
        /// <param name="path">The path to register the action for</param>
        /// <param name="action">The action to execute when the path is matched</param>
        protected void AddAction(string path, Action<string, JToken> action)
        {
            if (_actions.ContainsKey(path))
            {
                //Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, $"Messenger {Key} already has action registered at {path}", this);
                return;
            }

            _actions.Add(path, action);
        }

        /// <summary>
        /// Gets a list of all registered action paths
        /// </summary>
        /// <returns>A list of action paths</returns>
        public List<string> GetActionPaths()
        {
            return _actions.Keys.ToList();
        }

        /// <summary>
        /// Removes an action from the specified path
        /// </summary>
        /// <param name="path">The path to remove the action from</param>
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
        /// Helper for posting status message
        /// </summary>
        /// <param name="message">The device state message to post</param>
        /// <param name="clientId">Optional client ID to send the message to a specific client</param>
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
                this.LogError(ex, "Exception posting status message for {messagePath} to {clientId}", MessagePath, clientId ?? "all clients");
            }
        }

        /// <summary>
        /// Posts a status message with a specific message type
        /// </summary>
        /// <param name="type">The message type to send</param>
        /// <param name="deviceState">The device state message to post</param>
        /// <param name="clientId">Optional client ID to send the message to a specific client</param>
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
                this.LogError(ex, "Exception posting status message for {type} to {clientId}", type, clientId ?? "all clients");
            }
        }

        /// <summary>
        /// Posts a status message with JSON content
        /// </summary>
        /// <param name="content">The JSON content to send</param>
        /// <param name="type">The message type (defaults to MessagePath if empty)</param>
        /// <param name="clientId">Optional client ID to send the message to a specific client</param>
        protected void PostStatusMessage(JToken content, string type = "", string clientId = null)
        {
            try
            {
                AppServerController?.SendMessageObject(new MobileControlMessage { Type = !string.IsNullOrEmpty(type) ? type : MessagePath, ClientId = clientId, Content = content });
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception posting status message", this);
            }
        }

        /// <summary>
        /// Posts an event message for the device
        /// </summary>
        /// <param name="message">The device event message to post</param>
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
        /// Posts an event message with a specific event type
        /// </summary>
        /// <param name="message">The device event message to post</param>
        /// <param name="eventType">The event type to use</param>
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
        /// Posts an event message with only an event type
        /// </summary>
        /// <param name="eventType">The event type to post</param>
        protected void PostEventMessage(string eventType)
        {
            AppServerController?.SendMessageObject(new MobileControlMessage
            {
                Type = $"/event{MessagePath}/{eventType}",
                Content = JToken.FromObject(new { }),
            });
        }

    }

    /// <summary>
    /// Base class for device messages containing common properties like key, name, and message type
    /// </summary>
    public abstract class DeviceMessageBase
    {
        /// <summary>
        /// The device key
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// The device name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The type of the message class
        /// </summary>
        [JsonProperty("messageType")]
        public string MessageType => GetType().Name;

        /// <summary>
        /// Gets or sets the base path for the message
        /// </summary>
        [JsonProperty("messageBasePath")]
        public string MessageBasePath { get; set; }
    }

    /// <summary>
    /// Base class for state messages that includes the type of message and the implemented interfaces
    /// </summary>
    public class DeviceStateMessageBase : DeviceMessageBase
    {
        /// <summary>
        /// The interfaces implented by the device sending the messsage
        /// </summary>
        [JsonProperty("interfaces")]
        public List<string> Interfaces { get; private set; }

        /// <summary>
        /// Sets the interfaces implemented by the device
        /// </summary>
        /// <param name="interfaces">List of interface names to set</param>
        public void SetInterfaces(List<string> interfaces)
        {
            Interfaces = interfaces;
        }
    }

    /// <summary>
    /// Base class for event messages that include the type of message and an event type
    /// </summary>
    public abstract class DeviceEventMessageBase : DeviceMessageBase
    {
        /// <summary>
        /// The event type
        /// </summary>
        [JsonProperty("eventType")]
        public string EventType { get; set; }
    }

}