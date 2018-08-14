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
						Debug.Console(2, "deviceKey {0} Matches", device.Key);
						DmSwitch = DeviceManager.GetDeviceForKey(device.Key) as PepperDash.Essentials.DM.DmChassisController;

						} 
					

					else 	{
						Debug.Console(2, "deviceKey {0} doesn't match", device.Key);
						}
					}
				if (Properties.EiscApiIpids != null) {

					
					foreach (string Ipid in Properties.EiscApiIpids) {
						var ApiEisc = new BridgeApiEisc(Ipid);
						for (uint x = 1; x <= DmSwitch.Chassis.NumberOfInputs;x++ ) {
							uint tempX = x;
							Debug.Console(2, "Creating EiscActions {0}", tempX);


							ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputVideoRoutes[tempX], u => DmSwitch.ExecuteSwitch(u, tempX, eRoutingSignalType.Video));
							ApiEisc.Eisc.SetUShortSigAction(ApiMap.OutputAudioRoutes[tempX], u => DmSwitch.ExecuteSwitch(u, tempX, eRoutingSignalType.Audio));


                            if (DmSwitch.TxDictionary.ContainsKey(tempX)) {
								Debug.Console(2, "Creating Tx Feedbacks {0}", tempX);
								var TxKey = DmSwitch.TxDictionary[tempX];
								var TxDevice = DeviceManager.GetDeviceForKey(TxKey) as DmTxControllerBase;
								TxDevice.IsOnline.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.TxOnlineStatus[tempX]]);
								TxDevice.AnyVideoInput.VideoStatus.VideoSyncFeedback.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.TxVideoSyncStatus[tempX]]);
								ApiEisc.Eisc.SetUShortSigAction((ApiMap.HdcpSupport[tempX]), u => TxDevice.SetHdcpSupportAll((ePdtHdcpSupport)(u)));
								TxDevice.HdcpSupportAllFeedback.LinkInputSig(ApiEisc.Eisc.UShortInput[ApiMap.HdcpSupport[tempX]]);
								ApiEisc.Eisc.UShortInput[ApiMap.HdcpSupportCapability[tempX]].UShortValue = TxDevice.HdcpSupportCapability;
								} 
							else {
								DmSwitch.VideoInputSyncFeedbacks[tempX].LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.TxVideoSyncStatus[tempX]]);
								}
							if (DmSwitch.RxDictionary.ContainsKey(tempX)) {
								Debug.Console(2, "Creating Rx Feedbacks {0}", tempX);
								var RxKey = DmSwitch.RxDictionary[tempX];
								var RxDevice = DeviceManager.GetDeviceForKey(RxKey) as DmRmcControllerBase;
								RxDevice.IsOnline.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.RxOnlineStatus[tempX]]);
								}
							// DmSwitch.InputEndpointOnlineFeedbacks[(ushort)tempOutputNum].LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.OutputVideoRoutes[tempOutputNum]]);
							DmSwitch.VideoOutputFeedbacks[(ushort)tempX].LinkInputSig(ApiEisc.Eisc.UShortInput[ApiMap.OutputVideoRoutes[tempX]]);
							DmSwitch.AudioOutputFeedbacks[(ushort)tempX].LinkInputSig(ApiEisc.Eisc.UShortInput[ApiMap.OutputAudioRoutes[tempX]]);
							DmSwitch.InputNameFeedbacks[(ushort)tempX].LinkInputSig(ApiEisc.Eisc.StringInput[ApiMap.InputNames[tempX]]);
							DmSwitch.OutputNameFeedbacks[(ushort)tempX].LinkInputSig(ApiEisc.Eisc.StringInput[ApiMap.OutputNames[tempX]]);
							DmSwitch.OutputRouteNameFeedbacks[(ushort)tempX].LinkInputSig(ApiEisc.Eisc.StringInput[ApiMap.OutputRouteNames[tempX]]);
							}
						DmSwitch.IsOnline.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.ChassisOnline]);
						ApiEisc.Eisc.Register();
						}
					}



				Debug.Console(2, "Name {0} Activated", this.Name);
				return true;
				}
			catch (Exception e) {
				Debug.Console(2, "BRidge {0}", e);
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
		public Dictionary<uint, ushort> OutputVideoRoutes;
		public Dictionary<uint, ushort> OutputAudioRoutes;
		public Dictionary<uint, ushort> TxOnlineStatus;
		public Dictionary<uint, ushort> RxOnlineStatus;
		public Dictionary<uint, ushort> TxVideoSyncStatus;
		public Dictionary<uint, ushort> InputNames;
		public Dictionary<uint, ushort> OutputNames;
		public Dictionary<uint, ushort> OutputRouteNames;
		public Dictionary<uint, ushort> HdcpSupport;
		public Dictionary<uint, ushort> HdcpSupportCapability;

		public EssentialDMApiMap() {
			OutputVideoRoutes = new Dictionary<uint, ushort>();
			OutputAudioRoutes = new Dictionary<uint, ushort>();
			TxOnlineStatus = new Dictionary<uint, ushort>();
			RxOnlineStatus = new Dictionary<uint, ushort>();
			TxVideoSyncStatus = new Dictionary<uint, ushort>();
			InputNames = new Dictionary<uint, ushort>();
			OutputNames = new Dictionary<uint, ushort>();
			OutputRouteNames = new Dictionary<uint, ushort>();
			HdcpSupport = new Dictionary<uint, ushort>();
			HdcpSupportCapability = new Dictionary<uint, ushort>();

			for (uint x = 1; x <= 200; x++) {
				// Debug.Console(0, "Init Value {0}", x);
				uint tempNum = x;
				HdcpSupportCapability[tempNum] = (ushort)(tempNum + 1200);
				HdcpSupport[tempNum] = (ushort)(tempNum + 900);
				OutputVideoRoutes[tempNum] = (ushort)(tempNum + 100);
				OutputAudioRoutes[tempNum] = (ushort)(tempNum + 300);
				TxOnlineStatus[tempNum] = (ushort)(tempNum + 500);
				RxOnlineStatus[tempNum] = (ushort)(tempNum + 700);
				TxVideoSyncStatus[tempNum] = (ushort)(tempNum + 100);
				InputNames[tempNum] = (ushort)(tempNum + 100);
				OutputNames[tempNum] = (ushort)(tempNum + 300);
				OutputRouteNames[tempNum] = (ushort)(tempNum + 2000);
				}
			}
		}
	}
	