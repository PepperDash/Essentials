using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core.Monitoring;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class SystemMonitorMessenger : MessengerBase
    {
        public SystemMonitorController SysMon { get; private set; }

        public SystemMonitorMessenger(string key, SystemMonitorController sysMon, string messagePath)
            : base(key, messagePath)
        {
            if (sysMon == null)
                throw new ArgumentNullException("sysMon");

            SysMon = sysMon;

            SysMon.SystemMonitorPropertiesChanged += new EventHandler<EventArgs>(SysMon_SystemMonitorPropertiesChanged);

            foreach (var p in SysMon.ProgramStatusFeedbackCollection)
            {
                p.Value.AggregatedProgramInfoFeedback.OutputChange += new EventHandler<PepperDash.Essentials.Core.FeedbackEventArgs>(AggregatedProgramInfoFeedback_OutputChange);
            }
        }

        /// <summary>
        /// Posts the program information message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AggregatedProgramInfoFeedback_OutputChange(object sender, PepperDash.Essentials.Core.FeedbackEventArgs e)
        {
            SendProgramInfoStatusMessage(e.StringValue);
        }

        // Deserializes the program info into an object that can be setn in a status message
        void SendProgramInfoStatusMessage(string serializedProgramInfo)
        {
            var programInfo = JsonConvert.DeserializeObject<ProgramInfo>(serializedProgramInfo);

            Debug.Console(2, "Posting Status Message: {0}", programInfo.ToString());

            PostStatusMessage(programInfo);
        }

        /// <summary>
        /// Posts the system monitor properties
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SysMon_SystemMonitorPropertiesChanged(object sender, EventArgs e)
        {
            SendSystemMonitorStatusMessage();
        }

        void SendFullStatusMessage()
        {
            SendSystemMonitorStatusMessage();

            foreach (var p in SysMon.ProgramStatusFeedbackCollection)
            {
                SendProgramInfoStatusMessage(p.Value.AggregatedProgramInfoFeedback.StringValue);
            }
        }

        void SendSystemMonitorStatusMessage()
        {
            Debug.Console(2, "Posting System Monitor Status Message.");

            // This takes a while, launch a new thread
            CrestronInvoke.BeginInvoke((o) =>
            {
                PostStatusMessage(new
                {
                    timeZone = SysMon.TimeZoneFeedback.IntValue,
                    timeZoneName = SysMon.TimeZoneTextFeedback.StringValue,
                    ioControllerVersion = SysMon.IOControllerVersionFeedback.StringValue,
                    snmpVersion = SysMon.SnmpVersionFeedback.StringValue,
                    bacnetVersion = SysMon.BACnetAppVersionFeedback.StringValue,
                    controllerVersion = SysMon.ControllerVersionFeedback.StringValue
                });
            });
        }

        protected override void CustomRegisterWithAppServer(CotijaSystemController appServerController)
        {
            AppServerController.AddAction(MessagePath + "/fullStatus", new Action(SendFullStatusMessage));
        }

    }
}