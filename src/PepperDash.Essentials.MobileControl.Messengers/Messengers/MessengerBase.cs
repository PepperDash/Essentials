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
    /// Provides a messaging bridge
    /// </summary>
    public abstract class MessengerBase : EssentialsDevice, IMobileControlMessenger
    {
        /// <summary>
        /// The device this messenger is associated with
        /// </summary>
        protected IKeyName _device;

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
        /// Adds an action for a given path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="action"></param>
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
                this.LogError(ex, "Exception posting status message for {messagePath} to {clientId}", MessagePath, clientId ?? "all clients");
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
                this.LogError(ex, "Exception posting status message for {type} to {clientId}", type, clientId ?? "all clients");
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
                AppServerController?.SendMessageObject(new MobileControlMessage { Type = !string.IsNullOrEmpty(type) ? type : MessagePath, ClientId = clientId, Content = content });
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception posting status message", this);
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

    /// <summary>
    /// Base class for device messages that include the type of message
    /// </summary>
    public abstract class DeviceMessageBase
    {
        /// <summary>
        /// The device key
        /// </summary>
        [JsonProperty("key")]
        /// <summary>
        /// Gets or sets the Key
        /// </summary>
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
        /// Gets or sets the MessageBasePath
        /// </summary>
        [JsonProperty("messageBasePath")]

        public string MessageBasePath { get; set; }
    }

    /// <summary>
    /// Represents a DeviceStateMessageBase
    /// </summary>
    public class DeviceStateMessageBase : DeviceMessageBase
    {
        /// <summary>
        /// The interfaces implmented by the device sending the messsage
        /// </summary>
        [JsonProperty("interfaces")]
        public List<string> Interfaces { get; private set; }

        /// <summary>
        /// Sets the interfaces implemented by the device sending the message
        /// </summary>
        /// <param name="interfaces"></param>
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