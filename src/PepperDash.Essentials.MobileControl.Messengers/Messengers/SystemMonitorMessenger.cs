using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Monitoring;
using System;
using System.Threading.Tasks;

namespace PepperDash.Essentials.AppServer.Messengers
{
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
                PostStatusMessage(JToken.FromObject(p.Value.ProgramInfo)
                );
            }
        }

        private void SendSystemMonitorStatusMessage()
        {
            Debug.Console(1, "Posting System Monitor Status Message.");

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

#if SERIES4
        protected override void RegisterActions()
#else
        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
#endif
        {
            AddAction("/fullStatus", (id, content) => SendFullStatusMessage());
        }
    }

    public class SystemMonitorStateMessage
    {
        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public int TimeZone { get; set; }

        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public string TimeZoneName { get; set; }

        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public string IoControllerVersion { get; set; }

        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public string SnmpVersion { get; set; }

        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public string BacnetVersion { get; set; }

        [JsonProperty("timeZone", NullValueHandling = NullValueHandling.Ignore)]
        public string ControllerVersion { get; set; }
    }
}