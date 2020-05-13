using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Core;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Bridges;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.EthernetCommunication;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Responsible for loading all of the device types for this library
    /// </summary>
    public class BridgeFactory
    {
        public BridgeFactory()
        {
            var eiscApiAdvancedFactory = new EiscApiAdvancedFactory() as IDeviceFactory;
            eiscApiAdvancedFactory.LoadTypeFactories();

            var eiscApiFactory = new EiscApiFactory() as IDeviceFactory;
            eiscApiFactory.LoadTypeFactories();
        }

    }

    public class CommBridge : Device
    {
        public CommBridgeProperties Properties { get; private set; }

        public List<IBasicCommunication> CommDevices { get; private set; }

        public CommBridge(string key, string name, JToken properties)
            : base(key, name)
        {
            Properties = JsonConvert.DeserializeObject<CommBridgeProperties>(properties.ToString());
        }

        public override bool CustomActivate()
        {
            // Create EiscApis 
            if (Properties.Eiscs != null)
            {
                foreach (var eisc in Properties.Eiscs)
                {
                    var ApiEisc = new BridgeApiEisc(eisc.IpId, eisc.Hostname);
                }
            }

            foreach (var deviceKey in Properties.CommDevices)
            {
                var device = DeviceManager.GetDeviceForKey(deviceKey);

                if (device != null)
                {
                    Debug.Console(0, "deviceKey {0} Found in Device Manager", device.Key);
                    CommDevices.Add(device as IBasicCommunication);
                }
                else
                {
                    Debug.Console(0, "deviceKey {0} Not Found in Device Manager", deviceKey);
                }
            }

            // Iterate through all the CommDevices and link up their Actions and Feedbacks

            Debug.Console(0, "Bridge {0} Activated", this.Name);

            return true;
        }
    }


    public class EiscBridgeProperties
    {
        public string ParentDeviceKey { get; set; }
        public eApiType ApiType { get; set; }
        public List<EiscProperties> Eiscs { get; set; }
        public string ApiOverrideFilePath { get; set; }

        public class EiscProperties
        {
            public string IpId { get; set; }
            public string Hostname { get; set; }
        }
    }

    public class CommBridgeProperties : EiscBridgeProperties
    {
        public List<string> CommDevices { get; set; }
    }

    public enum eApiType { Eisc = 0 }

    public class BridgeApiEisc
    {
        public uint Ipid { get; private set; }
        public ThreeSeriesTcpIpEthernetIntersystemCommunications Eisc { get; private set; }

        public BridgeApiEisc(string ipid, string hostname)
        {
            Ipid = (UInt32)int.Parse(ipid, System.Globalization.NumberStyles.HexNumber);
            Eisc = new ThreeSeriesTcpIpEthernetIntersystemCommunications(Ipid, hostname, Global.ControlSystem);
            Eisc.Register();
            Eisc.SigChange += Eisc_SigChange;
            Debug.Console(0, "BridgeApiEisc Created at Ipid {0}", ipid);
        }
        void Eisc_SigChange(object currentDevice, Crestron.SimplSharpPro.SigEventArgs args)
        {
            if (Debug.Level >= 1)
                Debug.Console(1, "BridgeApiEisc change: {0} {1}={2}", args.Sig.Type, args.Sig.Number, args.Sig.StringValue);
            var uo = args.Sig.UserObject;
            if (uo is Action<bool>)
                (uo as Action<bool>)(args.Sig.BoolValue);
            else if (uo is Action<ushort>)
                (uo as Action<ushort>)(args.Sig.UShortValue);
            else if (uo is Action<string>)
                (uo as Action<string>)(args.Sig.StringValue);
        }
    }
}
