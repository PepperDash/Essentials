//using System;
//using Crestron.SimplSharpPro;

//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Devices;
//using PepperDash.Core;

//namespace PepperDash.Essentials
//{
//    public class CommFactory
//    {
//        public static IBasicCommunication CreateCommForDevice(JToken devToken)
//        {
//            var devKey = devToken.Value<string>("key");
//            IBasicCommunication comm = null;
//            try
//            {
//                var control = devToken["properties"]["control"];
//                var commMethod = control["method"].Value<string>();
//                if (commMethod == "com")
//                {
//                    var comConfig = JsonConvert.DeserializeObject<ComPortConfig>(
//                        control["comParams"].ToString(),
//                        new JsonSerializerSettings
//                        {
//                            // Needs ObjectCreationHandling to make the ComSpec struct populate
//                            ObjectCreationHandling = ObjectCreationHandling.Replace,
//                            Converters = new JsonConverter[] { new ComSpecJsonConverter() }
//                        });
//                    comm = new ComPortController(devKey + "-com", comConfig.GetComPort(), comConfig.ComSpec);
//                }
//                else if (commMethod == "tcpIp")
//                {
//                    var tcpConfig = JsonConvert.DeserializeObject<TcpIpConfig>(control["tcpParams"].ToString());
//                    comm = new GenericTcpIpClient(devKey + "-tcp", tcpConfig.Address, tcpConfig.Port, tcpConfig.BufferSize);
//                }
//            }
//            catch (Exception e)
//            {
//                Debug.Console(0, "Cannot create communication from JSON:\r{0}\r\rException:\r{1}", devToken.ToString(), e);
//            }

//            // put it in the device manager if it's the right flavor
//            var comDev = comm as Device;
//            if (comDev != null)
//                DeviceManager.AddDevice(comDev);

//            return comm;
//        }
//    }
//}