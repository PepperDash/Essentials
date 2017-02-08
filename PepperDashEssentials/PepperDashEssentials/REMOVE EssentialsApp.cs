//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Crestron.SimplSharp;
//using Crestron.SimplSharp.CrestronIO;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
//using Crestron.SimplSharpPro.CrestronThread;
//using Crestron.SimplSharpPro.Diagnostics;
//using Crestron.SimplSharpPro.EthernetCommunication;
//using Crestron.SimplSharpPro.UI;

//using Crestron.SimplSharpPro.DM;
//using Crestron.SimplSharpPro.DM.Cards;
//using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Core.Devices;
////using PepperDash.Essentials.Core.Devices.Dm;

//using PepperDash.Essentials.Displays;

////using PepperDash.Essentials.Core.Http;
//using PepperDash.Core;


//namespace PepperDash.Essentials
//{
//    public class EssentialsApp
//    {
//        public string ConfigPath { get; set; }

//        public Dictionary<string, Room> Rooms { get; private set; }

//        //EssentialsHttpApiHandler ApiHandler; // MOVE ???????????????????

//        public EssentialsApp(CrestronControlSystem cs)
//        {
//            // Use a fake license manager for now
//            Global.LicenseManager = PepperDash.Essentials.License.MockEssentialsLicenseManager.Manager;
			
			
//            // ---------------------------------- Make this configurable
//            //var server = Global.HttpConfigServer;
//            //server.Start(8081, "HttpConfigServer");
//            //ConfigPath = string.Format(@"\NVRAM\Program{0}\EssentialsConfiguration.json",
//            //    InitialParametersClass.ApplicationNumber);
//            //ApiHandler = new EssentialsHttpApiHandler(server, ConfigPath, @"\HTML\presets\lists\");

//            Debug.Console(0, "\r\r--------------------CONFIG BEGIN--------------------\r");
//            Configuration.Initialize(cs);
//            Configuration.ReadConfiguration(ConfigPath);
//            Debug.Console(0, "\r--------------------CONFIG END----------------------\r\r");
//        }
//    }

//    public class ResponseToken
//    {
//        public string Token { get; private set; }
//        public DateTime Expires { get; private set; }
//        public bool IsExpired { get { return Expires < DateTime.Now; } }

//        public ResponseToken(int timeoutMinutes)
//        {
//            Expires = DateTime.Now.AddMinutes(timeoutMinutes);
//            Token = Guid.NewGuid().ToString();
//        }
//    }
//}