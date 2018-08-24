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

namespace PepperDash.Essentials {
	public class EssentialDsp : PepperDash.Core.Device {
		public EssentialDspProperties Properties;
		public List<BridgeApiEisc> BridgeApiEiscs;
		private PepperDash.Essentials.Devices.Common.DSP.QscDsp Dsp;
		private EssentialDspApiMap ApiMap = new EssentialDspApiMap();
		public EssentialDsp(string key, string name, JToken properties)
			: base(key, name) {
			Properties = JsonConvert.DeserializeObject<EssentialDspProperties>(properties.ToString());


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
						Dsp = DeviceManager.GetDeviceForKey(device.Key) as PepperDash.Essentials.Devices.Common.DSP.QscDsp;
						break;
					} 
					else 	
					{
						Debug.Console(2, "deviceKey {0} doesn't match", device.Key);
						
					}
				}
				if (Properties.EiscApiIpids != null && Dsp != null)
				{
					foreach (string Ipid in Properties.EiscApiIpids)
					{
						var ApiEisc = new BridgeApiEisc(Ipid);
						Debug.Console(2, "Connecting EiscApi {0} to {1}", ApiEisc.Ipid, Dsp.Name);
						ushort x = 1;
						foreach (var channel in Dsp.LevelControlPoints)
						{
							//var QscChannel = channel.Value as PepperDash.Essentials.Devices.Common.DSP.QscDspLevelControl;
							Debug.Console(2, "QscChannel {0} connect", x);
							
							var QscChannel = channel.Value as IBasicVolumeWithFeedback;
							QscChannel.MuteFeedback.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.channelMuteToggle[x]]);
							QscChannel.VolumeLevelFeedback.LinkInputSig(ApiEisc.Eisc.UShortInput[ApiMap.channelVolume[x]]);

							ApiEisc.Eisc.SetSigTrueAction(ApiMap.channelMuteToggle[x], () =>  QscChannel.MuteToggle());
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.channelMuteOn[x], () => QscChannel.MuteOn());
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.channelMuteOff[x], () => QscChannel.MuteOff());

							ApiEisc.Eisc.SetBoolSigAction(ApiMap.channelVolumeUp[x], b => QscChannel.VolumeUp(b));
							ApiEisc.Eisc.SetBoolSigAction(ApiMap.channelVolumeDown[x], b => QscChannel.VolumeDown(b));

							ApiEisc.Eisc.SetUShortSigAction(ApiMap.channelVolume[x], u => QscChannel.SetVolume(u));
							ApiEisc.Eisc.SetStringSigAction(ApiMap.presetString, s => Dsp.RunPreset(s));
							x++;

						}
						x = 1;
						foreach (var preset in Dsp.PresetList)
						{
							ApiEisc.Eisc.StringInput[ApiMap.presets[x]].StringValue = preset.label;
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.presets[x], () => Dsp.RunPresetNumber(x));
							x++;
						}

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
	public class EssentialDspProperties {
		public string connectionDeviceKey;
		public string[] EiscApiIpids;


		}


	public class EssentialDspApiMap {
		public ushort presetString = 2000;
		public Dictionary<uint, ushort> channelMuteToggle;
		public Dictionary<uint, ushort> channelMuteOn;
		public Dictionary<uint, ushort> channelMuteOff;
		public Dictionary<uint, ushort> channelVolume;
		public Dictionary<uint, ushort> channelVolumeUp;
		public Dictionary<uint, ushort> channelVolumeDown;
		public Dictionary<uint, ushort> presets;

		public EssentialDspApiMap() {
			channelMuteToggle = new Dictionary<uint, ushort>();
			channelMuteOn = new Dictionary<uint, ushort>();
			channelMuteOff = new Dictionary<uint, ushort>();
			channelVolume = new Dictionary<uint, ushort>();
			presets = new Dictionary<uint, ushort>();
			channelVolumeUp = new Dictionary<uint, ushort>();
			channelVolumeDown = new Dictionary<uint, ushort>();
			for (uint x = 1; x <= 100; x++) {
				uint tempNum = x;
				presets[tempNum] = (ushort)(tempNum + 100);
				channelMuteToggle[tempNum] = (ushort)(tempNum + 400);
				channelMuteOn[tempNum] = (ushort)(tempNum + 600);
				channelMuteOff[tempNum] = (ushort)(tempNum + 800);
				channelVolume[tempNum] = (ushort)(tempNum + 200);
				channelVolumeUp[tempNum] = (ushort)(tempNum + 1000);
				channelVolumeDown[tempNum] = (ushort)(tempNum + 1200);
				}
			}
		}
	}
	