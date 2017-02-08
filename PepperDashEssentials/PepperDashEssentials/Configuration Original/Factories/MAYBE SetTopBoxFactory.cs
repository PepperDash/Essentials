//using System;
//using Crestron.SimplSharpPro;

//using Newtonsoft.Json.Linq;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Devices;

//using PepperDash.Core;

//namespace PepperDash.Essentials
//{
//    public class SetTopBoxFactory
//    {
//        public static Device Create(JToken devToken)
//        {
//            Device dev = null;
//            try
//            {
//                var devType = devToken.Value<string>("type");
//                var devKey = devToken.Value<string>("key");
//                var devName = devToken.Value<string>("name");
//                var props = devToken["properties"];
//                var portConfig = FactoryHelper.GetIrPort(props);
//                if (portConfig != null)
//                {
//                    if (devType.EndsWith("-generic"))
//                    {
//                        var stb = new IrSetTopBoxBase(devKey, devName, portConfig.Port, portConfig.FileName);
//                        // Do this a better way?
//                        stb.HasDpad = props["hasDpad"].Value<bool>();
//                        stb.HasDvr = props["hasDvr"].Value<bool>();
//                        stb.HasNumbers = props["hasNumbers"].Value<bool>();
//                        stb.HasPreset = props["hasPresets"].Value<bool>();
//                        dev = stb;
//                    }
//                    else
//                        FactoryHelper.HandleUnknownType(devToken, devType);

//                    var preDev = dev as IHasSetTopBoxProperties;
//                    if(preDev.HasPreset)
//                        preDev.LoadPresets(props["presetListName"].Value<string>());					
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