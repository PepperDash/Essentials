//using System;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.UI;

//using Newtonsoft.Json.Linq;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Devices;
//using PepperDash.Core;

//namespace PepperDash.Essentials
//{
//    public class TouchpanelFactory
//    {
//        public static Device Create(JToken devToken)
//        {
//            SmartGraphicsTouchpanelControllerBase dev = null;
//            try
//            {
//                var devType = devToken.Value<string>("type");
//                var devKey = devToken.Value<string>("key");
//                var devName = devToken.Value<string>("name");
//                var props = devToken["properties"];
//                if (devType.Equals("Tsw1052", StringComparison.OrdinalIgnoreCase))
//                {
//                    uint ipId = Convert.ToUInt32(props.Value<string>("ipId"), 16);
//                    var hw = new Tsw1052(ipId, Global.ControlSystem);
//                    dev = TouchpanelControllerFactory.Create(devKey, devName, hw, props.Value<string>("sgdPath"));
//                    dev.UsesSplashPage = props.Value<bool>("usesSplashPage");
//                    dev.ShowDate = props.Value<bool>("showDate");
//                    dev.ShowTime = props.Value<bool>("showTime");

//                    // This plugs the system key into the tp, but it won't be linked up until later
//                    dev.AddPostActivationAction(() =>
//                        {
//                            var defaultSystemKey = props.Value<string>("defaultSystemKey");
//                            dev.SetCurrentRoom((EssentialsRoom)DeviceManager.GetDeviceForKey(defaultSystemKey));
//                        });
//                }
//            }
//            catch (Exception e)
//            {
//                FactoryHelper.HandleDeviceCreationError(devToken, e);
//            }
//            return dev;
//        }
//    }
//}