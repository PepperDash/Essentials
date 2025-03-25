using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.AppServer.Messengers
{
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
                PostStatusMessage(new CommunicationMonitorState
                {
                    CommunicationMonitor = new CommunicationMonitorProps
                    {
                        IsOnline = _communicationMonitor.CommunicationMonitor.IsOnline,
                        Status = _communicationMonitor.CommunicationMonitor.Status
                    }
                }); 
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
        [JsonProperty("commMonitor", NullValueHandling = NullValueHandling.Ignore)]
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
