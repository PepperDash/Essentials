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
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.EthernetCommunication;

namespace PepperDash.Essentials {
	public class BridgeFactory {
		public static IKeyed GetDevice(PepperDash.Essentials.Core.Config.DeviceConfig dc) {
			// ? why is this static JTA 2018-06-13? 

			var key = dc.Key;
			var name = dc.Name;
			var type = dc.Type;
			var properties = dc.Properties;
			var propAnon = new { };
			JsonConvert.DeserializeAnonymousType(dc.Properties.ToString(), propAnon);

			var typeName = dc.Type.ToLower();
			var groupName = dc.Group.ToLower();

			Debug.Console(0, "Name {0}, Key {1}, Type {2}, Properties {3}", name, key, type, properties.ToString());
			if (typeName == "essentialdm") 
			{
				return new EssentialDM(key, name, properties);
			} 
			else if (typeName == "essentialcomm") 
			{
				Debug.Console(0, "Launch Essential Comm");
				return new EssentialComm(key, name, properties);
			}
			else if (typeName == "essentialdsp")
			{
				Debug.Console(0, "Launch EssentialDsp");
				return new EssentialDsp(key, name, properties);
			}
			else if (typeName == "essentialstvone")
			{
				Debug.Console(0, "Launch essentialstvone");
				return new EssentialsTVOne(key, name, properties);
			}
			return null;
			}
		}
	public class BridgeApiEisc {
		public uint Ipid;
		public ThreeSeriesTcpIpEthernetIntersystemCommunications Eisc; 
		public BridgeApiEisc(string ipid) {
			Ipid = (UInt32)int.Parse(ipid, System.Globalization.NumberStyles.HexNumber);
			Eisc = new ThreeSeriesTcpIpEthernetIntersystemCommunications(Ipid, "127.0.0.2", Global.ControlSystem);
			Eisc.Register();
			Eisc.SigChange += Eisc_SigChange;
			Debug.Console(0, "BridgeApiEisc Created at Ipid {0}", ipid);
			}
		void Eisc_SigChange(object currentDevice, Crestron.SimplSharpPro.SigEventArgs args) {
			if (Debug.Level >= 1)
				Debug.Console(2, "DDVC EISC change: {0} {1}={2}", args.Sig.Type, args.Sig.Number, args.Sig.StringValue);
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


	
	