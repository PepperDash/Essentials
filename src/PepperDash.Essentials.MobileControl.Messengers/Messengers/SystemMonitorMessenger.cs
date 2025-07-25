using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.Monitoring;
using System;
using System.Threading.Tasks;

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

        private void SendFullStatusMessage()
        {
            SendSystemMonitorStatusMessage();

            foreach (var p in systemMonitor.ProgramStatusFeedbackCollection)
            {
                PostStatusMessage(JToken.FromObject(p.Value.ProgramInfo));
            }
        }

        private void SendSystemMonitorStatusMessage()
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
            })
            ));
        }

        protected override void RegisterActions()
        {
            AddAction("/fullStatus", (id, content) => SendFullStatusMessage());
        }
    }

    /// <summary>
    /// Represents a SystemMonitorStateMessage
    /// </summary>
    public class SystemMonitorStateMessage
    {
        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the TimeZone
        /// </summary>
        public int TimeZone { get; set; }

        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the TimeZoneName
        /// </summary>
        public string TimeZoneName { get; set; }

        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the IoControllerVersion
        /// </summary>
        public string IoControllerVersion { get; set; }

        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the SnmpVersion
        /// </summary>
        public string SnmpVersion { get; set; }

        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the BacnetVersion
        /// </summary>
        public string BacnetVersion { get; set; }

        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the ControllerVersion
        /// </summary>
        public string ControllerVersion { get; set; }
    }
}