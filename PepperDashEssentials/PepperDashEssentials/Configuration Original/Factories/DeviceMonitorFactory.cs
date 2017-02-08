//using System;
//using Crestron.SimplSharpPro;

//using Newtonsoft.Json.Linq;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Core.Devices;
//using PepperDash.Core;

//namespace PepperDash.Essentials
//{
//    public class DeviceManagerFactory
//    {
//        public static Device Create(JToken devToken)
//        {
//            Device dev = null;
//            try
//            {
//                var devType = devToken.Value<string>("type");
//                var devKey = devToken.Value<string>("key");
//                var devName = devToken.Value<string>("name");
//                if (devType.Equals("DeviceMonitor", StringComparison.OrdinalIgnoreCase))
//                {
//                    var comm = CommFactory.CreateCommForDevice(devToken);
//                    if (comm == null) return null;
//                    dev = new GenericCommunicationMonitoredDevice(devKey, devName, comm, devToken["properties"]["pollString"].Value<string>());
//                }
//                else
//                    FactoryHelper.HandleUnknownType(devToken, devType);
//            }
//            catch (Exception e)
//            {
//                FactoryHelper.HandleDeviceCreationError(devToken, e);
//            }
//            return dev;
//        }
//    }
//}