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
	public class EssentialDM : PepperDash.Core.Device {
		public EssentialDMProperties Properties;
		public List<BridgeApiEisc> BridgeApiEiscs;
		private PepperDash.Essentials.DM.DmChassisController DmSwitch;
		private EssentialDMApiMap ApiMap = new EssentialDMApiMap();
		public EssentialDM(string key, string name, JToken properties)
			: base(key, name) {
			Properties = JsonConvert.DeserializeObject<EssentialDMProperties>(properties.ToString());


			}
		public override bool CustomActivate() {
			// Create EiscApis 
			try {
				foreach (var device in DeviceManager.AllDevices) {
					if (device.Key == this.Properties.connectionDeviceKey) {
						Debug.Console(0, "deviceKey {0} Matches", device.Key);
						DmSwitch = DeviceManager.GetDeviceForKey(device.Key) as PepperDash.Essentials.DM.DmChassisController;

						} else {
						Debug.Console(0, "deviceKey {0} doesn't match", device.Key);
						}
					}
				if (Properties.EiscApiIpids != null) {
					foreach (string Ipid in Properties.EiscApiIpids) {
						var ApiEisc = new BridgeApiEisc(Ipid);
						// BridgeApiEiscs.Add(ApiEisc);
						//Essentials.Core.CommFactory.CreateCommForConfig();

						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputVideoRoutes[1], u => DmSwitch.ExecuteSwitch(u, 1, eRoutingSignalType.Video));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputVideoRoutes[2], u => DmSwitch.ExecuteSwitch(u, 2, eRoutingSignalType.Video));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputVideoRoutes[3], u => DmSwitch.ExecuteSwitch(u, 3, eRoutingSignalType.Video));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputVideoRoutes[4], u => DmSwitch.ExecuteSwitch(u, 4, eRoutingSignalType.Video));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputVideoRoutes[5], u => DmSwitch.ExecuteSwitch(u, 5, eRoutingSignalType.Video));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputVideoRoutes[6], u => DmSwitch.ExecuteSwitch(u, 6, eRoutingSignalType.Video));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputVideoRoutes[7], u => DmSwitch.ExecuteSwitch(u, 7, eRoutingSignalType.Video));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputVideoRoutes[8], u => DmSwitch.ExecuteSwitch(u, 8, eRoutingSignalType.Video));

						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputAudioRoutes[1], u => DmSwitch.ExecuteSwitch(u, 1, eRoutingSignalType.Audio));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputAudioRoutes[2], u => DmSwitch.ExecuteSwitch(u, 2, eRoutingSignalType.Audio));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputAudioRoutes[3], u => DmSwitch.ExecuteSwitch(u, 3, eRoutingSignalType.Audio));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputAudioRoutes[4], u => DmSwitch.ExecuteSwitch(u, 4, eRoutingSignalType.Audio));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputAudioRoutes[5], u => DmSwitch.ExecuteSwitch(u, 5, eRoutingSignalType.Audio));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputAudioRoutes[6], u => DmSwitch.ExecuteSwitch(u, 6, eRoutingSignalType.Audio));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputAudioRoutes[7], u => DmSwitch.ExecuteSwitch(u, 7, eRoutingSignalType.Audio));
						ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputAudioRoutes[8], u => DmSwitch.ExecuteSwitch(u, 8, eRoutingSignalType.Audio));

						foreach (var output in DmSwitch.Chassis.Outputs) {
							Debug.Console(0, "Creating EiscActions {0}", output.Number);

							DmSwitch.InputEndpointOnlineFeedbacks[(ushort)output.Number].LinkInputSig(ApiEisc.Eisc.BooleanInput[(ushort)(output.Number + 300)]);

							
							 /* This wont work...routes to 8 every time i tried for loops, forweach. For some reason the test number keeps going to max value of the loop
							 * always routing testNum to MaxLoopValue, the above works though.*/

							for (uint testNum = 1; testNum < 8; testNum++) {
								uint num = testNum;
								ApiEisc.Eisc.SetUShortSigAction((ushort)(output.Number + 300), u => DmSwitch.ExecuteSwitch(u, num, eRoutingSignalType.Audio));
								ApiEisc.Eisc.SetUShortSigAction((ushort)(output.Number + 100), u => DmSwitch.ExecuteSwitch(u, num, eRoutingSignalType.Video));
								}
							 
							DmSwitch.OutputRouteFeedbacks[(ushort)output.Number].LinkInputSig(ApiEisc.Eisc.UShortInput[ApiMap.OutputVideoRoutes[(int)output.Number]]);
							}
						DmSwitch.IsOnline.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.ChassisOnline]);
						ApiEisc.Eisc.Register();
						}
					}



				Debug.Console(0, "Name {0} Activated", this.Name);
				return true;
				} catch (Exception e) {
				Debug.Console(0, "BRidge {0}", e);
				return false;
				}
			}
		}
	public class EssentialDMProperties {
		public string connectionDeviceKey;
		public string[] EiscApiIpids;


		}


	public class EssentialDMApiMap {
		public ushort ChassisOnline = 11;
		public Dictionary<int, ushort> OutputVideoRoutes;
		public Dictionary<int, ushort> OutputAudioRoutes;

		public EssentialDMApiMap() {
			OutputVideoRoutes = new Dictionary<int, ushort>();
			OutputAudioRoutes = new Dictionary<int, ushort>();

			for (int x = 1; x <= 200; x++) {
				// Debug.Console(0, "Init Value {0}", x);
				OutputVideoRoutes[x] = (ushort)(x + 100);
				OutputAudioRoutes[x] = (ushort)(x + 300);
				}
			}
		}
	}
	