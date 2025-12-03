using System;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.Monitoring;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a SystemMonitorMessenger
    /// </summary>
    public class SystemMonitorMessenger : MessengerBase
    {
        private readonly SystemMonitorController systemMonitor;

        public SystemMonitorMessenger(string key, SystemMonitorController sysMon, string messagePath)
            : base(key, messagePath, sysMon)
        {
            systemMonitor = sysMon ?? throw new ArgumentNullException("sysMon");

            systemMonitor.SystemMonitorPropertiesChanged += SysMon_SystemMonitorPropertiesChanged;

            foreach (var p in systemMonitor.ProgramStatusFeedbackCollection)
            {
                p.Value.ProgramInfoChanged += ProgramInfoChanged;
            }

            CrestronConsole.AddNewConsoleCommand(s => SendFullStatusMessage(), "SendFullSysMonStatus",
                "Sends the full System Monitor Status", ConsoleAccessLevelEnum.AccessOperator);
        }

        /// <summary>
        /// Posts the program information message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgramInfoChanged(object sender, ProgramInfoEventArgs e)
        {
            if (e.ProgramInfo != null)
            {
                //Debug.Console(1, "Posting Status Message: {0}", e.ProgramInfo.ToString());
                PostStatusMessage(JToken.FromObject(e.ProgramInfo)
                );
            }
        }

        /// <summary>
        /// Posts the system monitor properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SysMon_SystemMonitorPropertiesChanged(object sender, EventArgs e)
        {
            SendSystemMonitorStatusMessage();
        }

        private void SendFullStatusMessage(string id = null)
        {
            SendSystemMonitorStatusMessage(id);

            foreach (var p in systemMonitor.ProgramStatusFeedbackCollection)
            {
                PostStatusMessage(JToken.FromObject(p.Value.ProgramInfo), id);
            }
        }

        private void SendSystemMonitorStatusMessage(string id = null)
        {
            // This takes a while, launch a new thread

            Task.Run(() => PostStatusMessage(JToken.FromObject(new SystemMonitorStateMessage
            {
                TimeZone = systemMonitor.TimeZoneFeedback.IntValue,
                TimeZoneName = systemMonitor.TimeZoneTextFeedback.StringValue,
                IoControllerVersion = systemMonitor.IoControllerVersionFeedback.StringValue,
                SnmpVersion = systemMonitor.SnmpVersionFeedback.StringValue,
                BacnetVersion = systemMonitor.BaCnetAppVersionFeedback.StringValue,
                ControllerVersion = systemMonitor.ControllerVersionFeedback.StringValue
            }), id
            ));
        }

        protected override void RegisterActions()
        {
            AddAction("/fullStatus", (id, content) => SendFullStatusMessage(id));

            AddAction("/systemStatus", (id, content) => SendFullStatusMessage(id));
        }
    }

    /// <summary>
    /// Represents a SystemMonitorStateMessage
    /// </summary>
    public class SystemMonitorStateMessage
    {
        /// <summary>
        /// Gets or sets the TimeZone
        /// </summary>
        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public int TimeZone { get; set; }


        /// <summary>
        /// Gets or sets the TimeZoneName
        /// </summary>
        [JsonProperty("timeZoneName", NullValueHandling = NullValueHandling.Ignore)]
        public string TimeZoneName { get; set; }


        /// <summary>
        /// Gets or sets the IoControllerVersion
        /// </summary>
        [JsonProperty("ioControllerVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string IoControllerVersion { get; set; }


        /// <summary>
        /// Gets or sets the SnmpVersion
        /// </summary>
        [JsonProperty("snmpVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string SnmpVersion { get; set; }


        /// <summary>
        /// Gets or sets the BacnetVersion
        /// </summary>
        [JsonProperty("bacnetVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string BacnetVersion { get; set; }


        /// <summary>
        /// Gets or sets the ControllerVersion
        /// </summary>
        [JsonProperty("controllerVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string ControllerVersion { get; set; }
    }
}