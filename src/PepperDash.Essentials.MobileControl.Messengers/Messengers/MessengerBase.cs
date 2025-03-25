using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides a messaging bridge
    /// </summary>
#if SERIES4
    public abstract class MessengerBase : EssentialsDevice, IMobileControlMessenger
#else
    public abstract class MessengerBase: EssentialsDevice
#endif
    {
        protected IKeyName _device;

        private readonly List<string> _deviceInterfaces;

        private readonly Dictionary<string, Action<string, JToken>> _actions = new Dictionary<string, Action<string, JToken>>();

        public string DeviceKey => _device?.Key ?? "";

        /// <summary>
        /// 
        /// </summary>
#if SERIES4
        public IMobileControl AppServerController { get; private set; }
#else
        public MobileControlSystemController AppServerController { get; private set; }
#endif

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
#if SERIES4
        public void RegisterWithAppServer(IMobileControl appServerController)
#else
        public void RegisterWithAppServer(MobileControlSystemController appServerController)
#endif
        {
            AppServerController = appServerController ?? throw new ArgumentNullException("appServerController");

            AppServerController.AddAction(this, HandleMessage);

            RegisterActions();
        }

        private void HandleMessage(string path, string id, JToken content)
        {
            // replace base path with empty string. Should leave something like /fullStatus
            var route = path.Replace(MessagePath, string.Empty); 

            if(!_actions.TryGetValue(route, out var action)) {                
                return;
            }

            Debug.LogMessage(Serilog.Events.LogEventLevel.Debug, "Executing action for path {path}", this, path);

            action(id, content);
        }

        protected void AddAction(string path, Action<string, JToken> action)
        {
            if (_actions.ContainsKey(path))
            {
                //Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, $"Messenger {Key} already has action registered at {path}", this);
                return;
            }

            _actions.Add(path, action);
        }

        public List<string> GetActionPaths()
        {
            return _actions.Keys.ToList();
        }

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
        /// <param name="appServerController"></param>
#if SERIES4
        protected virtual void RegisterActions()
#else
        protected virtual void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
#endif
        {

        }

        /// <summary>
        /// Helper for posting status message
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        protected void PostStatusMessage(DeviceStateMessageBase message, string clientId = null)
        {
            try
            {
                if(message == null)
                {
                    throw new ArgumentNullException("message");
                }

                if(_device == null)
                {
                    throw new ArgumentNullException("device");
                }

                message.SetInterfaces(_deviceInterfaces);

                message.Key = _device.Key;

                message.Name = _device.Name;

                PostStatusMessage(JToken.FromObject(message), MessagePath, clientId);
            }
            catch (Exception ex) {
                Debug.LogMessage(ex, "Exception posting status message", this);
            }
        }

#if SERIES4 
        protected void PostStatusMessage(string type, DeviceStateMessageBase deviceState, string clientId = null)
        {
            try
            {
                //Debug.Console(2, this, "*********************Setting DeviceStateMessageProperties on MobileControlResponseMessage");
                deviceState.SetInterfaces(_deviceInterfaces);

                deviceState.Key = _device.Key;

                deviceState.Name = _device.Name;

                deviceState.MessageBasePath = MessagePath;

                PostStatusMessage(JToken.FromObject(deviceState), type, clientId);
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception posting status message", this);
            }
        }
#endif
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

        protected void PostEventMessage(string eventType)
        {
            AppServerController?.SendMessageObject(new MobileControlMessage
            {
                Type = $"/event{MessagePath}/{eventType}",
                Content = JToken.FromObject(new { }),
            });
        }

    }

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

        [JsonProperty("messageBasePath")]
        public string MessageBasePath { get; set; }
    }

    /// <summary>
    /// Base class for state messages that includes the type of message and the implmented interfaces
    /// </summary>
    public class DeviceStateMessageBase : DeviceMessageBase
    {
        /// <summary>
        /// The interfaces implmented by the device sending the messsage
        /// </summary>
        [JsonProperty("interfaces")]
        public List<string> Interfaces { get; private set; }

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