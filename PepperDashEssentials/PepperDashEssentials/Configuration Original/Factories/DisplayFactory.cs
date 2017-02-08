//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;

//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Devices;
//using PepperDash.Essentials.Displays;
//using PepperDash.Core;

//namespace PepperDash.Essentials
//{
//    public class DisplayFactory
//    {
//        public static DisplayBase CreateDisplay(JToken devToken)
//        {
//            DisplayBase dev = null;
//            try
//            {
//                var devType = devToken.Value<string>("type");
//                var devKey = devToken.Value<string>("key");
//                var devName = devToken.Value<string>("name");
//                var properties = devToken["properties"];

//                if (devType.Equals("MockDisplay", StringComparison.OrdinalIgnoreCase))
//                    dev = new MockDisplay(devKey, devName);

//                else if (devType.Equals("NecMPSX", StringComparison.OrdinalIgnoreCase))
//                {
//                    var comm = CommFactory.CreateCommForDevice(devToken);
//                    if (comm == null) return null;
//                    dev = new NecPSXMDisplayCom(devKey, devName, comm);
					


//                    //var commMethod = properties["control"]["method"].Value<string>();

//                    //// Helper-ize this?
//                    //if(commMethod == "com")
//                    //{
//                    //    // Move some of this up above???
//                    //    var comConfig = JsonConvert.DeserializeObject<ComPortConfig>(
//                    //        properties["control"]["comParams"].ToString(),
//                    //        new JsonSerializerSettings { 
//                    //            // Needs ObjectCreationHandling to make the ComSpec struct populate
//                    //            ObjectCreationHandling = ObjectCreationHandling.Replace,
//                    //            Converters = new JsonConverter[] { new ComSpecJsonConverter() }
//                    //        });
//                    //    dev = new NecPSXMDisplayCom(devKey, devName, comConfig.GetComPort(), comConfig.ComSpec);
//                    //}
//                    //else if (commMethod == "tcpIp")
//                    //{
//                    //    var spec = properties["control"]["tcpSpec"];
//                    //    var host = spec["address"].Value<string>();
//                    //    var port = spec["port"].Value<int>();
//                    //    dev = new NecPSXMDisplayCom(devKey, devName, host, port);
//                    //}



//                }

//                else if (devType.Equals("NecNpPa550", StringComparison.OrdinalIgnoreCase))
//                {
//                    var proj = new NecPaSeriesProjector(devKey, devName);
//                    var comm = CreateCommunicationFromPropertiesToken(
//                        devKey + "-comm", properties, 3000);
//                    proj.CommunicationMethod = comm;
//                    dev = proj;
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

//        public static IBasicCommunication CreateCommunicationFromPropertiesToken(
//            string commKey, JToken properties, int bufferSize)
//        {
//            Debug.Console(2, "Building port from: {0}", properties.ToString());

//            var tcpToken = properties["TcpIp"];
//            if (tcpToken != null)
//            {
//                // Convert the Tcp property
//                var spec = JsonConvert.DeserializeObject<TcpIpConfig>(tcpToken.ToString());

//                var tcp = new GenericTcpIpClient(commKey, spec.Address, spec.Port, bufferSize);
//                DeviceManager.AddDevice(tcp);
//                return tcp;
//            }

//            var com = properties["Com"];
//            if (com != null)
//            {
//                // Make the interim config object
//                var comConfig = JsonConvert.DeserializeObject<ComPortConfig>(com.ToString(),
//                    new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });

//                // Get the IComPorts hardware device from the Device or Control System
//                var comDev = comConfig.GetIComPortsDeviceFromManagedDevice();
//                if (comDev != null)
//                {
//                    var controller = new ComPortController(commKey, comDev.ComPorts[comConfig.ComPortNumber], comConfig.ComSpec);
//                    DeviceManager.AddDevice(controller);
//                    return controller;
//                }
//            }
//            Debug.Console(0, "No Tcp or Com port information for port {0}", commKey);
//            return null;
//        }

//    }
//}