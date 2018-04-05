using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Lighting;

namespace PepperDash.Essentials.Devices.Common.Environment.Lutron
{
    public class LutronQuantumArea : LightingBase, ILightingMasterRaiseLower, ICommunicationMonitor
    {
        public IBasicCommunication Communication { get; private set; }
        public CommunicationGather PortGather { get; private set; }
        public StatusMonitorBase CommunicationMonitor { get; private set; }

        public int IntegrationId;
        public string Password;

        const string Delimiter = "\x0d\x0a";
        const string Set = "#";
        const string Get = "?";

        public LutronQuantumArea(string key, string name, IBasicCommunication comm, LutronQuantumPropertiesConfig props)
            : base(key, name)
        {
            Communication = comm;

            IntegrationId = props.IntegrationId;

            Password = props.Password;

            LightingScenes = props.Scenes;

            var socket = comm as ISocketStatus;
            if (socket != null)
            {
                // IP Control
                socket.ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
            }
            else
            {
                // RS-232 Control
            }

            PortGather = new CommunicationGather(Communication, Delimiter);
            PortGather.LineReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(PortGather_LineReceived);

            if (props.CommunicationMonitorProperties != null)
            {
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, props.CommunicationMonitorProperties);
            }
            else
            {
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 120000, 120000, 300000, "?SYSTEM,1\x0d\x0a");
            }
        }

        public override bool CustomActivate()
        {
            Communication.Connect();
            CommunicationMonitor.StatusChange += (o, a) => { Debug.Console(2, this, "Communication monitor state: {0}", CommunicationMonitor.Status); };
            CommunicationMonitor.Start();

            return true;
        }

        void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e)
        {
            Debug.Console(2, this, "Socket Status Change: {0}", e.Client.ClientStatus.ToString());

            if (e.Client.IsConnected)
            {
                // Tasks on connect
            }
        }

        void PortGather_LineReceived(object sender, GenericCommMethodReceiveTextArgs args)
        {
            try
            {
                if (args.Text.IndexOf("login:") > -1)
                {
                    // Login
                    SendLine(Password);
                }
                else if (args.Text.IndexOf("~AREA") > -1)
                {
                    var response = args.Text.Split(',');

                    var integrationId = Int32.Parse(response[1]);

                    if (integrationId != IntegrationId)
                    {
                        Debug.Console(2, this, "Response is not for correct Integration ID");
                        return;
                    }
                    else
                    {
                        var action = Int32.Parse(response[2]);

                        switch (action)
                        {
                            case (int)eAction.Scene:
                                {
                                    var scene = Int32.Parse(response[3]);
                                    CurrentLightingScene = LightingScenes.FirstOrDefault(s => s.ID.Equals(scene));

                                    var handler = LightingSceneChange;
                                    if (handler != null)
                                    {
                                        handler(this, new LightingSceneChangeEventArgs(CurrentLightingScene));
                                    }
                                    
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Console(2, this, "Error parsing response:\n{0}", e);
            }
        }

        /// <summary>
        /// Recalls the specified scene
        /// </summary>
        /// <param name="scene"></param>
        public void SelectScene(LightingScene scene)
        {
            SendLine(string.Format("{0}AREA,{1},{2},{3}", Set, IntegrationId, (int)eAction.Scene, (int)scene.ID));
        }

        /// <summary>
        /// Begins raising the lights in the area
        /// </summary>
        public void MasterRaise()
        {
            SendLine(string.Format("{0}AREA,{1},{2}", Set, IntegrationId, (int)eAction.Raise));
        }

        /// <summary>
        /// Begins lowering the lights in the area
        /// </summary>
        public void MasterLower()
        {
            SendLine(string.Format("{0}AREA,{1},{2}", Set, IntegrationId, (int)eAction.Lower));
        }

        /// <summary>
        /// Stops the current raise/lower action
        /// </summary>
        public void MasterRaiseLowerStop()
        {
            SendLine(string.Format("{0}AREA,{1},{2}", Set, IntegrationId, (int)eAction.Stop));
        }

        /// <summary>
        /// Appends the delimiter and sends the string
        /// </summary>
        /// <param name="s"></param>
        public void SendLine(string s)
        {
            Debug.Console(1, this, "TX: '{0}'", s);
            Communication.SendText(s + Delimiter);
        }
    }

    public enum eAction
    {
        SetLevel = 1,
        Raise = 2,
        Lower = 3,
        Stop = 4,
        Scene = 6,
        DaylightMode = 7,
        OccupancyState = 8,
        OccupancyMode = 9,
        OccupiedLevelOrScene = 12,
        UnoccupiedLevelOrScene = 13,
        HyperionShaddowSensorOverrideState = 26,
        HyperionBrightnessSensorOverrideStatue = 27
    }

    public class LutronQuantumPropertiesConfig
    {
        public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }
        public ControlPropertiesConfig Control { get; set; }

        public int IntegrationId { get; set; }
        public List<LightingScene> Scenes{ get; set; }
        
        public string Password { get; set; }
    }
}