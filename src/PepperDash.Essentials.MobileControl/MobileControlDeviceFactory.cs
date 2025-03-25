using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Room.MobileControl;
using System;
using System.Collections.Generic;
using System.Linq;


namespace PepperDash.Essentials
{
    public class MobileControlDeviceFactory : EssentialsDeviceFactory<MobileControlSystemController>
    {
        public MobileControlDeviceFactory()
        {            
            TypeNames = new List<string> { "appserver", "mobilecontrol", "webserver" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            try
            {
                var props = dc.Properties.ToObject<MobileControlConfig>();
                return new MobileControlSystemController(dc.Key, dc.Name, props);
            }
            catch (Exception e)
            {
                Debug.LogMessage(e, "Error building Mobile Control System Controller");
                return null;
            }
        }
    }

    public class MobileControlSimplFactory : EssentialsDeviceFactory<MobileControlSIMPLRoomBridge>
    {
        public MobileControlSimplFactory()
        {            
            TypeNames = new List<string> { "mobilecontrolbridge-ddvc01", "mobilecontrolbridge-simpl" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var comm = CommFactory.GetControlPropertiesConfig(dc);

            var bridge = new MobileControlSIMPLRoomBridge(dc.Key, dc.Name, comm.IpIdInt);

            bridge.AddPreActivationAction(() =>
            {
                var parent = GetMobileControlDevice();

                if (parent == null)
                {
                    Debug.Console(0, bridge, "ERROR: Cannot connect bridge. System controller not present");
                    return;
                }
                Debug.Console(0, bridge, "Linking to parent controller");

                /*bridge.AddParent(parent);
                parent.AddBridge(bridge);*/

                parent.AddDeviceMessenger(bridge);
            });

            return bridge;
        }

        private static MobileControlSystemController GetMobileControlDevice()
        {
            var mobileControlList = DeviceManager.AllDevices.OfType<MobileControlSystemController>().ToList();

            if (mobileControlList.Count > 1)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Warning,
                    "Multiple instances of Mobile Control Server found.");
                return null;
            }

            if (mobileControlList.Count > 0)
            {
                return mobileControlList[0];
            }

            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Mobile Control not enabled for this system");
            return null;
        }
    }
}