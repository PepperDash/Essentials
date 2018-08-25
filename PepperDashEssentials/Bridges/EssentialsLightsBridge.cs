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
	public class EssentialsLightsBridge : PepperDash.Core.Device
	{
		public EssentialsLightsBridgeProperties Properties;
		public List<BridgeApiEisc> BridgeApiEiscs;
		private PepperDash.Essentials.Core.Lighting.LightingBase Lights;
		private EssentialsLightsBridgeApiMap ApiMap = new EssentialsLightsBridgeApiMap();
		public EssentialsLightsBridge(string key, string name, JToken properties)
			: base(key, name)
		{
			Properties = JsonConvert.DeserializeObject<EssentialsLightsBridgeProperties>(properties.ToString());


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
						Lights = DeviceManager.GetDeviceForKey(device.Key) as PepperDash.Essentials.Core.Lighting.LightingBase;
						break;
					}	
					else 	
					{
						Debug.Console(2, "deviceKey {0} doesn't match", device.Key);
						
					}
				}
				if (Properties.EiscApiIpids != null && Lights != null)
				{
					foreach (string Ipid in Properties.EiscApiIpids)
					{
						var ApiEisc = new BridgeApiEisc(Ipid);
						Debug.Console(2, "Connecting EiscApi {0} to {1}", ApiEisc.Ipid, TVOneCorio.Name);
						ushort x = 1;
						Lights.Is
						var tempDevice = Lights as PepperDash.Core.Device;
						tempDevice.
						.OnlineFeedback.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.Online]);

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
	public class EssentialsLightsBridgeProperties
	{
		public string connectionDeviceKey;
		public string[] EiscApiIpids;


	}


	public class EssentialsLightsBridgeApiMap
	{
		public ushort CallPreset = 1;
		public ushort PresetFeedback = 1;
		public ushort Online = 1;

		public EssentialsLightsBridgeApiMap()
		{


		}

	}
}