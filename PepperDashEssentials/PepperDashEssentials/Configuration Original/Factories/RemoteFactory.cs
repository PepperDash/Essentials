//using System;
//using System.Collections.Generic;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.Remotes;
//using Crestron.SimplSharpPro.UI;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Devices;
////using PepperDash.Essentials.Remotes;
//using PepperDash.Core;

//namespace PepperDash.Essentials
//{
//    //public class RemoteFactory
//    //{
//    //    public static Device Create(JToken devToken)
//    //    {
//    //        Hr150Controller dev = null;
//    //        try
//    //        {
//    //            var devType = devToken.Value<string>("type");
//    //            var devKey = devToken.Value<string>("key");
//    //            var devName = devToken.Value<string>("name");
//    //            var props = devToken["properties"];

//    //            if (devType.Equals("hr150", StringComparison.OrdinalIgnoreCase))
//    //            {
//    //                uint id = Convert.ToUInt32(props.Value<string>("rfId"), 16);
//    //                var parent = props.Value<string>("rfGateway");
//    //                RFExGateway rf = GetGateway(parent);

//    //                var hw = new Hr150(id, rf);
//    //                dev = new Hr150Controller(devKey, devName, hw);

//    //                // Have to add the buttons and default source after all devices are spun up
//    //                dev.AddPostActivationAction(() =>
//    //                    {
//    //                        var defaultSystemKey = props.Value<string>("defaultSystemKey");
//    //                        dev.SetCurrentRoom((EssentialsRoom)DeviceManager.GetDeviceForKey(defaultSystemKey));
							
//    //                        // Link custom buttons
//    //                        var buttonProps = JsonConvert.DeserializeObject<Dictionary<uint, string>>(props["buttons"].ToString());
//    //                        foreach (var kvp in buttonProps)
//    //                        {
//    //                            var split = kvp.Value.Split(':');
//    //                            if (split[0].Equals("source"))
//    //                            {
//    //                                var src = DeviceManager.GetDeviceForKey(split[1]) as IPresentationSource;
//    //                                if (src == null)
//    //                                {
//    //                                    Debug.Console(0, dev, "Error: Cannot add source key '{0}'", split[1]);
//    //                                    continue;
//    //                                }
//    //                                dev.SetCustomButtonAsSource(kvp.Key, src);
//    //                            }
//    //                            else if (split[0] == "room")
//    //                            {
//    //                                if (split[1] == "off")
//    //                                    dev.SetCustomButtonAsRoomOff(kvp.Key);
//    //                            }
//    //                        }
//    //                    });
//    //            }
//    //            else if (devType.Equals("tsr302", StringComparison.OrdinalIgnoreCase))
//    //            {
//    //                uint id = Convert.ToUInt32(props.Value<string>("rfId"), 16);
//    //                var parent = props.Value<string>("rfGateway");
//    //                RFExGateway rf = GetGateway(parent);
//    //                var sgd = props.Value<string>("sgdPath");

//    //                var hw = new Tsr302(id, rf);

//    //                //dev = new Hr150Controller(devKey, devName, hw);

//    //                //// Have to add the buttons and default source after all devices are spun up
//    //                //dev.AddPostActivationAction(() =>
//    //                //{
//    //                //    var defaultSystemKey = props.Value<string>("defaultSystemKey");
//    //                //    dev.SetCurrentRoom((EssentialsRoom)DeviceManager.GetDeviceForKey(defaultSystemKey));

//    //                //    // Link custom buttons
//    //                //    var buttonProps = JsonConvert.DeserializeObject<Dictionary<uint, string>>(props["buttons"].ToString());
//    //                //    foreach (var kvp in buttonProps)
//    //                //    {
//    //                //        var split = kvp.Value.Split(':');
//    //                //        if (split[0].Equals("source"))
//    //                //        {
//    //                //            var src = DeviceManager.GetDeviceForKey(split[1]) as IPresentationSource;
//    //                //            if (src == null)
//    //                //            {
//    //                //                Debug.Console(0, dev, "Error: Cannot add source key '{0}'", split[1]);
//    //                //                continue;
//    //                //            }
//    //                //            dev.SetCustomButtonAsSource(kvp.Key, src);
//    //                //        }
//    //                //        else if (split[0] == "room")
//    //                //        {
//    //                //            if (split[1] == "off")
//    //                //                dev.SetCustomButtonAsRoomOff(kvp.Key);
//    //                //        }
//    //                //    }
//    //                //});
//    //            }
//    //        }
//    //        catch (Exception e)
//    //        {
//    //            FactoryHelper.HandleDeviceCreationError(devToken, e);
//    //        }
//    //        return dev;
//    //    }

//    //    public static RFExGateway GetGateway(string parent)
//    //    {
//    //        if (parent == null)
//    //            parent = "controlSystem";
//    //        RFExGateway rf = null;
//    //        if (parent.Equals("controlSystem", StringComparison.OrdinalIgnoreCase)
//    //            || parent.Equals("processor", StringComparison.OrdinalIgnoreCase))
//    //        {
//    //            rf = Global.ControlSystem.ControllerRFGatewayDevice;
//    //        }
//    //        return rf;
//    //    }
//    //}
//}