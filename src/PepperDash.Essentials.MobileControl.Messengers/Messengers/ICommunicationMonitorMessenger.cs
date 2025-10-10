using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a ICommunicationMonitorMessenger
    /// </summary>
    public class ICommunicationMonitorMessenger : MessengerBase
    {
        private readonly ICommunicationMonitor _communicationMonitor;

        public ICommunicationMonitorMessenger(string key, string messagePath, ICommunicationMonitor device) : base(key, messagePath, device as IKeyName)
        {
            _communicationMonitor = device;
        }

        protected override void RegisterActions()
        {
            base.RegisterActions();

            AddAction("/fullStatus", (id, content) =>
            {
                SendFullStatus(id);
            });

            AddAction("/commStatus", (id, content) =>
            {
                SendFullStatus(id);
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

        private void SendFullStatus(string id = null)
        {
            PostStatusMessage(new CommunicationMonitorState
            {
                CommunicationMonitor = new CommunicationMonitorProps
                {
                    IsOnline = _communicationMonitor.CommunicationMonitor.IsOnline,
                    Status = _communicationMonitor.CommunicationMonitor.Status
                },
            }, id);
        }
    }

    /// <summary>
    /// Represents a CommunicationMonitorState
    /// </summary>
    public class CommunicationMonitorState : DeviceStateMessageBase
    {
        [JsonProperty("commMonitor", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the CommunicationMonitor
        /// </summary>
        public CommunicationMonitorProps CommunicationMonitor { get; set; }

    }

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
