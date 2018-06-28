//using System;
//using Crestron.SimplSharpPro;

//using Newtonsoft.Json.Linq;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Devices;
//using PepperDash.Core;

//namespace PepperDash.Essentials
//{
//    public class PcFactory
//    {
//        public static Device Create(JToken devToken)
//        {
//            Device dev = null;
//            //try
//            //{
//            //    var devType = devToken.Value<string>("type");
//            //    var devKey = devToken.Value<string>("key");
//            //    var devName = devToken.Value<string>("name");
//            //    if (devType.Equals("laptop", StringComparison.OrdinalIgnoreCase))
//            //        dev = new Laptop(devKey, devName);
//            //    else
//            //        FactoryHelper.HandleUnknownType(devToken, devType);
//            //}
//            //catch (Exception e)
//            //{
//            //    FactoryHelper.HandleDeviceCreationError(devToken, e);
//            //}
//            return dev;
//        }
//    }
//}