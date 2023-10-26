extern alias Full;
using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;
using Full::Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials
{
    public class EssentialsTouchpanelControllerFactory : EssentialsDeviceFactory<EssentialsTouchpanelController>
    {
        public EssentialsTouchpanelControllerFactory()
        {
            TypeNames = new List<string>() { "crestronapp", "tsw550", "tsw750", "tsw1050", "tsw560", "tsw760", "tsw1060", "tsw570", "tsw770", "ts770", "tsw1070", "ts1070", "xpanel" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var comm  = CommFactory.GetControlPropertiesConfig(dc);
            var props = JsonConvert.DeserializeObject<CrestronTouchpanelPropertiesConfig>(dc.Properties.ToString());

            var panel = GetPanelForType(dc.Type, comm.IpIdInt, props.ProjectName);

            if (panel == null)
            {
                Debug.Console(0, "Unable to create Touchpanel for type {0}. Touchpanel Controller WILL NOT function correctly", dc.Type);
            }

            Debug.Console(1, "Factory Attempting to create new EssentialsTouchpanelController");

            var panelController = new EssentialsTouchpanelController(dc.Key, dc.Name, panel, props);

            return panelController;
        }

        private BasicTriListWithSmartObject GetPanelForType(string type, uint id, string projectName)
        {
            type = type.ToLower();
            try
            {
                if (type == "crestronapp")
                {
                    var app = new CrestronApp(id, Global.ControlSystem);
                    app.ParameterProjectName.Value = projectName;
                    return app;
                }
                else if (type == "xpanel")
                    return new XpanelForSmartGraphics(id, Global.ControlSystem);
                else if (type == "tsw550")
                    return new Tsw550(id, Global.ControlSystem);
                else if (type == "tsw552")
                    return new Tsw552(id, Global.ControlSystem);
                else if (type == "tsw560")
                    return new Tsw560(id, Global.ControlSystem);
                else if (type == "tsw750")
                    return new Tsw750(id, Global.ControlSystem);
                else if (type == "tsw752")
                    return new Tsw752(id, Global.ControlSystem);
                else if (type == "tsw760")
                    return new Tsw760(id, Global.ControlSystem);
                else if (type == "tsw1050")
                    return new Tsw1050(id, Global.ControlSystem);
                else if (type == "tsw1052")
                    return new Tsw1052(id, Global.ControlSystem);
                else if (type == "tsw1060")
                    return new Tsw1060(id, Global.ControlSystem);
                else if (type == "tsw570")
                    return new Tsw570(id, Global.ControlSystem);
                else if (type == "tsw770")
                    return new Tsw770(id, Global.ControlSystem);
                else if (type == "ts770")
                    return new Ts770(id, Global.ControlSystem);
                else if (type == "tsw1070")
                    return new Tsw1070(id, Global.ControlSystem);
                else if (type == "ts1070")
                    return new Ts1070(id, Global.ControlSystem);
                else
                {
                    Debug.Console(0, Debug.ErrorLogLevel.Notice, "WARNING: Cannot create TSW controller with type '{0}'", type);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "WARNING: Cannot create TSW base class. Panel will not function: {0}", e.Message);
                return null;
            }
        }
    }
}