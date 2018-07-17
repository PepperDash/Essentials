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
using Crestron.SimplSharpPro.CrestronThread;

namespace PepperDash.Essentials {
	public class EssentialCommConfig {
		public string[] EiscApiIpids;
		public EssentialCommCommConnectionConfigs[] CommConnections;
		}
	public class EssentialCommCommConnectionConfigs {
		public uint joinNumber {get; set; }
		public EssentialsControlPropertiesConfig control { get; set; }
		}

	public class EssentialCommsPort {
		public IBasicCommunication Comm;
		public IntFeedback StatusFeedback;
		public BoolFeedback ConnectedFeedback;
		public List<EssentialComApiMap> Outputs = new List<EssentialComApiMap>();
		public String RxBuffer;
		public EssentialCommsPort(EssentialsControlPropertiesConfig config, string keyPrefix) {
			Comm = CommFactory.CreateCommForConfig(config, keyPrefix);
			// var PortGather = new CommunicationGather(Comm, config.EndOfLineChar);
			Comm.TextReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(Communication_TextReceived);

			var socket = Comm as ISocketStatus;
			StatusFeedback = new IntFeedback(() => { return (int)socket.ClientStatus; });
			ConnectedFeedback = new BoolFeedback(() => { return Comm.IsConnected; });

			if (socket != null) {
				socket.ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
				} else {
				}

			}
		void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e) {
			StatusFeedback.FireUpdate();
			ConnectedFeedback.FireUpdate();
			if (e.Client.IsConnected) {
				// Tasks on connect
				} else {
				// Cleanup items from this session
				}
			}
		void Communication_TextReceived(object sender, GenericCommMethodReceiveTextArgs args) {
			try {
				foreach (var Output in Outputs) {
					Output.Api.Eisc.StringInput[Output.Join].StringValue = args.Text;
					}

				}
			catch (Exception) {
				throw new FormatException(string.Format("ERROR:{0}"));
				}
			}
		}
		
	public class EssentialComm : Device {
		public EssentialCommConfig Properties;
		
		public CommunicationGather PortGather { get; private set; }
		public List<BridgeApiEisc> Apis {get; set;}
		public Dictionary<string, StringFeedback> CommFeedbacks {get; private set; }
		public StatusMonitorBase CommunicationMonitor { get; private set; }
		public Dictionary<uint, EssentialCommsPort> CommDictionary { get; private set; }

		public EssentialComm(string key, string name, JToken properties) : base(key, name) {
			Properties = JsonConvert.DeserializeObject<EssentialCommConfig>(properties.ToString());
			CommFeedbacks = new Dictionary<string, StringFeedback>();
			CommDictionary = new Dictionary<uint, EssentialCommsPort>();
			Apis = new List<BridgeApiEisc>();
							int commNumber = 1;
				foreach (var commConfig in Properties.CommConnections) {
					var commPort = new EssentialCommsPort(commConfig.control, string.Format("{0}-{1}", this.Key, commConfig.joinNumber));
					CommDictionary.Add(commConfig.joinNumber, commPort);

					commNumber++;
					}

				foreach (var Ipid in Properties.EiscApiIpids) {
					var ApiEisc = new BridgeApiEisc(Ipid);
					Apis.Add(ApiEisc);
					foreach (var commConnection in CommDictionary) {
						Debug.Console(0, "Joining Api{0} to comm {1}", Ipid, commConnection.Key);
						var tempComm = commConnection.Value;
						var tempJoin = (uint)commConnection.Key;
						EssentialComApiMap ApiMap = new EssentialComApiMap(ApiEisc, (uint)tempJoin);

						tempComm.Outputs.Add(ApiMap);
						// Check for ApiMap Overide Values here

						ApiEisc.Eisc.SetBoolSigAction(tempJoin, b => {if (b) { tempComm.Comm.Connect(); } else { tempComm.Comm.Disconnect(); }});
						ApiEisc.Eisc.SetStringSigAction(tempJoin, s => tempComm.Comm.SendText(s));

						tempComm.StatusFeedback.LinkInputSig(ApiEisc.Eisc.UShortInput[tempJoin]);
						tempComm.ConnectedFeedback.LinkInputSig(ApiEisc.Eisc.BooleanInput[tempJoin]);



						}
					ApiEisc.Eisc.Register();
					}
		    }



        public override bool CustomActivate()
        {
			try {


					
				Debug.Console(0, "Name {0} Activated", this.Name);
				return true;
				}
			catch (Exception e) {
				Debug.Console(0, "BRidge {0}", e);
				return false;
				}
        }


		}
	public class EssentialComApiMap {
		public uint Join;
		public BridgeApiEisc Api;
		public uint connectJoin; 
		public  EssentialComApiMap(BridgeApiEisc api, uint join) {
			Join = join;
			Api = api;
			}
		}
	}