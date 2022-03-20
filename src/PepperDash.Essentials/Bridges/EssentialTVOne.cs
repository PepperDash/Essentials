using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.DM;
using PepperDash.Core;
using PepperDash.Essentials.Core.Routing;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.EthernetCommunication;
using Crestron.SimplSharpPro.DM;

namespace PepperDash.Essentials
{
	public class EssentialsTVOne : PepperDash.Core.Device
	{
		public EssentialTVOneProperties Properties;
		public List<BridgeApiEisc> BridgeApiEiscs;
		private PepperDash.Essentials.Devices.Common.TVOneCorio TVOneCorio;
		private EssentialsTVOneApiMap ApiMap = new EssentialsTVOneApiMap();
		public EssentialsTVOne(string key, string name, JToken properties)
			: base(key, name)
		{
			Properties = JsonConvert.DeserializeObject<EssentialTVOneProperties>(properties.ToString());


		}
		public override bool CustomActivate() {
			// Create EiscApis 
			try
			{
				foreach (var device in DeviceManager.AllDevices) 
				{
					if (device.Key == this.Properties.connectionDeviceKey) 
					{
						Debug.Console(2, "deviceKey {0} Matches", device.Key);
						TVOneCorio = DeviceManager.GetDeviceForKey(device.Key) as PepperDash.Essentials.Devices.Common.TVOneCorio;
						break;
					}	
					else 	
					{
						Debug.Console(2, "deviceKey {0} doesn't match", device.Key);
						
					}
				}
				if (Properties.EiscApiIpids != null && TVOneCorio != null)
				{
					foreach (string Ipid in Properties.EiscApiIpids)
					{
						var ApiEisc = new BridgeApiEisc(Ipid);
						Debug.Console(2, "Connecting EiscApi {0} to {1}", ApiEisc.Ipid, TVOneCorio.Name);
						ushort x = 1;
						TVOneCorio.OnlineFeedback.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.Online]);
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.CallPreset, u => TVOneCorio.CallPreset(u));
						TVOneCorio.PresetFeedback.LinkInputSig(ApiEisc.Eisc.UShortInput[ApiMap.PresetFeedback]);

					}
				}

				


				Debug.Console(2, "Name {0} Activated", this.Name);
				return true;
				}
			catch (Exception e) {
				Debug.Console(0, "Bridge {0}", e);
				return false;
				}
			}
	}
	public class EssentialTVOneProperties
	{
		public string connectionDeviceKey;
		public string[] EiscApiIpids;


	}


	public class EssentialsTVOneApiMap
	{
		public ushort CallPreset = 1;
		public ushort PresetFeedback = 1;
		public ushort Online = 1;

		public EssentialsTVOneApiMap()
		{


		}

	}
}