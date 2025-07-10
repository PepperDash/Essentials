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
    /// Provides messaging capabilities for system monitoring operations.
    /// Handles system performance metrics, program status reporting, and monitoring data communication.
    /// </summary>
    public class SystemMonitorMessenger : MessengerBase
    {
        private readonly SystemMonitorController systemMonitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemMonitorMessenger"/> class.
        /// </summary>
        /// <param name="key">The unique identifier for this messenger instance.</param>
        /// <param name="sysMon">The system monitor controller for monitoring operations.</param>
        /// <param name="messagePath">The message path for system monitor messages.</param>
        /// <exception cref="ArgumentNullException">Thrown when sysMon is null.</exception>
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

        /// <summary>
        /// Registers actions for handling system monitor operations.
        /// Includes full status reporting for system monitoring data.
        /// </summary>
        protected override void RegisterActions()
        {
            AddAction("/fullStatus", (id, content) => SendFullStatusMessage());
        }
    }

    /// <summary>
    /// Represents the system monitor state message containing system information and version details.
    /// </summary>
    public class SystemMonitorStateMessage
    {
        /// <summary>
        /// Gets or sets the system time zone offset.
        /// </summary>
        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public int TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the system time zone name.
        /// </summary>
        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public string TimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the IO controller version information.
        /// </summary>
        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public string IoControllerVersion { get; set; }

        /// <summary>
        /// Gets or sets the SNMP version information.
        /// </summary>
        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public string SnmpVersion { get; set; }

        /// <summary>
        /// Gets or sets the BACnet version information.
        /// </summary>
        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public string BacnetVersion { get; set; }

        /// <summary>
        /// Gets or sets the controller version information.
        /// </summary>
        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public string ControllerVersion { get; set; }
    }
}