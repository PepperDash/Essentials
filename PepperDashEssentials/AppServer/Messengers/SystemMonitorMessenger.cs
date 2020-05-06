using System;
using Crestron.SimplSharp;
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

            SysMon.SystemMonitorPropertiesChanged += SysMon_SystemMonitorPropertiesChanged;

            foreach (var p in SysMon.ProgramStatusFeedbackCollection)
            {
                p.Value.ProgramInfoChanged += ProgramInfoChanged;
            }

            CrestronConsole.AddNewConsoleCommand(s => SendFullStatusMessage(), "SendFullSysMonStatus", "Sends the full System Monitor Status", ConsoleAccessLevelEnum.AccessOperator);
        }

        /// <summary>
        /// Posts the program information message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ProgramInfoChanged(object sender, ProgramInfoEventArgs e)
        {
            if (e.ProgramInfo != null)
            {
                //Debug.Console(1, "Posting Status Message: {0}", e.ProgramInfo.ToString());
                PostStatusMessage(e.ProgramInfo);
            }
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
                PostStatusMessage(p.Value.ProgramInfo);               
            }
        }

        void SendSystemMonitorStatusMessage()
        {
            Debug.Console(1, "Posting System Monitor Status Message.");

            // This takes a while, launch a new thread
            CrestronInvoke.BeginInvoke(o => PostStatusMessage(new
            {
                timeZone = SysMon.TimeZoneFeedback.IntValue,
                timeZoneName = SysMon.TimeZoneTextFeedback.StringValue,
                ioControllerVersion = SysMon.IoControllerVersionFeedback.StringValue,
                snmpVersion = SysMon.SnmpVersionFeedback.StringValue,
                bacnetVersion = SysMon.BaCnetAppVersionFeedback.StringValue,
                controllerVersion = SysMon.ControllerVersionFeedback.StringValue
            }));
        }

        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
        {
            AppServerController.AddAction(MessagePath + "/fullStatus", new Action(SendFullStatusMessage));
        }

    }
}