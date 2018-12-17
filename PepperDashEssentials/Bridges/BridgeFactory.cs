using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Core;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Bridges;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.EthernetCommunication;

namespace PepperDash.Essentials 
{
	public class BridgeFactory 
    {
		public static IKeyed GetDevice(DeviceConfig dc) 
        {
			// ? why is this static JTA 2018-06-13? 
			
			var key = dc.Key;
			var name = dc.Name;
			var type = dc.Type;
			var properties = dc.Properties;
			var propAnon = new { };
			
			var typeName = dc.Type.ToLower();
			var groupName = dc.Group.ToLower();

			//Debug.Console(2, "Name {0}, Key {1}, Type {2}, Properties {3}", name, key, type, properties.ToString());

			if (typeName == "eiscapi") 
            {
			    return new EiscApi(dc);
			}

		    return null;
		}
	}

	public class DmBridge : Device 
    {
        public EiscBridgeProperties Properties { get; private set; }

        public PepperDash.Essentials.DM.DmChassisController DmSwitch { get; private set; }

		public DmBridge(string key, string name, JToken properties) : base(key, name) 
        {
	        Properties = JsonConvert.DeserializeObject<EiscBridgeProperties>(properties.ToString());
	    }

		public override bool CustomActivate() 
        {
			// Create EiscApis 
			if (Properties.Eiscs != null) 
            {
				foreach (var eisc in Properties.Eiscs) 
                {
					var ApiEisc = new BridgeApiEisc(eisc.IpId, eisc.Hostname);

					ApiEisc.Eisc.SetUShortSigAction(101, u => DmSwitch.ExecuteSwitch(u,1, eRoutingSignalType.Video));
					ApiEisc.Eisc.SetUShortSigAction(102, u => DmSwitch.ExecuteSwitch(u,2, eRoutingSignalType.Video));								
				}
			}

			foreach (var device in DeviceManager.AllDevices) 
            {
				if (device.Key == this.Properties.ParentDeviceKey) 
                {
					Debug.Console(0, "deviceKey {0} Matches", device.Key);
					DmSwitch = DeviceManager.GetDeviceForKey(device.Key) as PepperDash.Essentials.DM.DmChassisController;
				} 
				else 
                {
					Debug.Console(0, "deviceKey {0} doesn't match", device.Key);
				}
			}

			Debug.Console(0, "Bridge {0} Activated", this.Name);
			return true;
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