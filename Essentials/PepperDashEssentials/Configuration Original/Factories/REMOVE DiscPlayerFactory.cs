//using System;
//using Crestron.SimplSharpPro;

//using Newtonsoft.Json.Linq;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Devices;
//using PepperDash.Core;

//namespace PepperDash.Essentials
//{
//    public class DiscPlayerFactory
//    {
//        public static Device Create(JToken devToken)
//        {
//            Device dev = null;
//            try
//            {
//                var devType = devToken.Value<string>("type");
//                var devKey = devToken.Value<string>("key");
//                var devName = devToken.Value<string>("name");

//                // Filter out special (Pioneer
//                //(devType.Equals("genericIr", StringComparison.OrdinalIgnoreCase))

//                var props = devToken["properties"];
//                var portConfig = FactoryHelper.GetIrPort(props);
//                if (portConfig != null)
//                {
//                    if (devType.EndsWith("-generic"))
//                        dev = new IrDvdBase(devKey, devName, portConfig.Port, portConfig.FileName);
//                    else
//                        FactoryHelper.HandleUnknownType(devToken, devType);
//                }	

//                // NO PORT ERROR HERE??

//            }
//            catch (Exception e)
//            {
//                FactoryHelper.HandleDeviceCreationError(devToken, e);
//            }
//            return dev;
//        }
//    }
//}