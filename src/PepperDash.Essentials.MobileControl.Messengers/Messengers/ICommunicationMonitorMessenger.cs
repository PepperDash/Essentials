using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides messaging capabilities for communication monitoring operations.
    /// Handles communication status reporting and monitoring feedback.
    /// </summary>
    public class ICommunicationMonitorMessenger : MessengerBase
    {
        private readonly ICommunicationMonitor _communicationMonitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ICommunicationMonitorMessenger"/> class.
        /// </summary>
        /// <param name="key">The unique identifier for this messenger instance.</param>
        /// <param name="messagePath">The message path for communication monitor messages.</param>
        /// <param name="device">The device that provides communication monitoring functionality.</param>
        public ICommunicationMonitorMessenger(string key, string messagePath, ICommunicationMonitor device) : base(key, messagePath, device as IKeyName)
        {
            _communicationMonitor = device;
        }

        /// <summary>
        /// Registers actions for handling communication monitoring operations.
        /// Includes full status reporting for communication monitoring data.
        /// </summary>
        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) =>
            {
                PostStatusMessage(new CommunicationMonitorState
                {
                    CommunicationMonitor = new CommunicationMonitorProps
                    {
                        IsOnline = _communicationMonitor.CommunicationMonitor.IsOnline,
                        Status = _communicationMonitor.CommunicationMonitor.Status
                    }
                }, id);
            });

            _communicationMonitor.CommunicationMonitor.StatusChange += (sender, args) =>
            {
                PostStatusMessage(JToken.FromObject(new
                {
                    commMonitor = new CommunicationMonitorProps
                    {
                        IsOnline = _communicationMonitor.CommunicationMonitor.IsOnline,
                        Status = _communicationMonitor.CommunicationMonitor.Status
                    }
                }));
            };
        }
    }

    /// <summary>
    /// Represents the state of the communication monitor
    /// </summary>
    public class CommunicationMonitorState : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the communication monitor properties.
        /// </summary>
        [JsonProperty("commMonitor", NullValueHandling = NullValueHandling.Ignore)]
        public CommunicationMonitorProps CommunicationMonitor { get; set; }

    }

    /// <summary>
    /// Represents the properties of a communication monitor.
    /// </summary>
    public class CommunicationMonitorProps
    {        /// <summary>
             /// For devices that implement ICommunicationMonitor, reports the online status of the device
             /// </summary>
        [JsonProperty("isOnline", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsOnline { get; set; }

        /// <summary>
        /// For devices that implement ICommunicationMonitor, reports the online status of the device
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public MonitorStatus Status { get; set; }

    }

}
