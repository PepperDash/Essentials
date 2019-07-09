using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using System.Text.RegularExpressions;
using Crestron.SimplSharp.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace PepperDash.Essentials.Devices.Common
{

    public class EvertzEndpoint : Device
    {



		public IBasicCommunication Communication { get; private set; }
		public CommunicationGather PortGather { get; private set; }
		public GenericCommunicationMonitor CommunicationMonitor { get; private set; }
		
		private GenericHttpClient Client;
		public string userName;
		public string password;
		public string Address;
		private bool OnlineStatus; 
		public BoolFeedback OnlineFeedback;
		public IntFeedback PresetFeedback;


        public bool isSubscribed;



        CrestronQueue CommandQueue;

		public Dictionary<string, EvertzEndpointPort> Ports;

		private string _ControllerKey;


		private EvertzEndpointStatusServer StatusServer;
		private String StatusServerId;

        /// <summary>
        /// Shows received lines as hex
        /// </summary>
        public bool ShowHexResponse { get; set; }

		public EvertzEndpoint(string key, string name, EvertzEndpointPropertiesConfig props, string type) :
            base(key, name)
        {

		
			this.Address = props.address;
			Client = new GenericHttpClient(string.Format("{0}-GenericWebClient", name), string.Format("{0}-GenericWebClient", name), this.Address);
			Client.ResponseRecived += new EventHandler<GenericHttpClientEventArgs>(Client_ResponseRecived);
			Ports = new Dictionary<string, EvertzEndpointPort>();
			if (type.ToLower() == "mma10g-trs4k")
			{
				//create port hdmi 01
				EvertzEndpointPort hdmi1 = new EvertzEndpointPort("HDMI01", "131.0@s", "136.0@s");
				EvertzEndpointPort hdmi2 = new EvertzEndpointPort("HDMI02", "131.1@s", "136.1@s");
				// add to dictionay with all keys
				addPortToDictionary(hdmi1);
				addPortToDictionary(hdmi2);
			}
			_ControllerKey = null;
			if (props.controllerKey != null)
			{
				_ControllerKey = props.controllerKey;
			}
			AddPostActivationAction( () => {PostActivation();});
			CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
			if (props.CommunicationMonitorProperties != null)
			{
				CommunicationMonitor = new GenericCommunicationMonitor(this, Client, props.CommunicationMonitorProperties);
			}
			else
			{
				CommunicationMonitor = new GenericCommunicationMonitor(this, Client, 40000, 120000, 300000, "v.api/apis/EV/SERVERSTATUS");
			}
			
 
        }
		 
		/// <summary>
		/// Helper method
		/// </summary>
		/// <param name="port"></param>
		private void addPortToDictionary(EvertzEndpointPort port)
		{
			Ports.Add(port.PortName, port);
			Ports.Add(port.ResolutionVarID, port);
			Ports.Add(port.SyncVarID, port);
			//PollForState(port.SyncVarID);
			//PollForState(port.ResolutionVarID);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        public override bool CustomActivate()
        {

			// Create Device -> Constructor fires 
			// PreActivations get called
			// CustomActivate Gets Called Anything that is involved with this single class Ex: Connection, Setup Feedback, Etc. 
			// After this point all devices are ready for interaction 
			// PostActivation gets called. Use this for interClass activation. 
			CommunicationMonitor.Start();
			OnlineFeedback = new BoolFeedback(() => { return OnlineStatus; });

			
            //CrestronConsole.AddNewConsoleCommand(SendLine, "send" + Key, "", ConsoleAccessLevelEnum.AccessOperator);
            return true;
        }

		/// <summary>
		/// 
		/// </summary>
		private void PostActivation()
		{
			Debug.Console(2, this, "EvertzEndpoint Post Activation");
			if (_ControllerKey != null)
			{
				StatusServer = DeviceManager.GetDeviceForKey(_ControllerKey) as EvertzEndpointStatusServer;
				StatusServer.RegisterEvertzEndpoint(this);

				// RegisterStatusServer();
				// SendStatusRequest();
			}
			// PollAll();
		}
		
		void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
		{
			if (programEventType == eProgramStatusEventType.Stopping)
			{
				Debug.Console(1, this, "Program stopping. Disabling EvertzStatusServer");
				if (StatusServerId != null)
				{
					//UnregisterServer();
				}
			}
		}
		
		private void ProcessServerStatusRequest(EvertsStatusRequesstResponse status)
		{
			// var status = JsonConvert.DeserializeObject<EvertsStatusRequesstResponse>(SendStatusRequest());
			if (status.error != null)
			{
				
			}
			else if (status.data != null)
			{
				foreach (var server in status.data.servers)
				{
					if (server.name == string.Format("{0}-{1}", this.Name, StatusServer.Address))
					{
						StatusServerId = server.id;
						Debug.Console(2, this, "EvertzEndpoint {0} StatusServer {1} Registered ID {2}", Name, StatusServer.Name, StatusServerId);
						/*
						
						foreach (var port in Ports)
						{
							// TODO JTA: This needs a better check 
							// you get a {"status": "success"} or "error": "error to register notification- Variable exists.."
							if (!server.notify.parameters.Contains(port.Value.ResolutionVarID))
							{
								RegisterForNotification(StatusServerId, port.Value.ResolutionVarID);
							}
							if (!server.notify.parameters.Contains(port.Value.ResolutionVarID))
							{
								RegisterForNotification(StatusServerId, port.Value.SyncVarID);
							}
						}
						*/
 
						break;
					}
				}
				StatusServerId = null;
			}
		}
		private void RegisterServerWithEndpoint()
		{
			/*
			var registrationResult = RegisterServer(StatusServer.Address, string.Format("{0}-{1}", this.Name, StatusServer.Address), StatusServer.Server.Port.ToString());
			Debug.Console(2, this, "EvertzEndpointStatusServer Registration Result with device {0}\n{1}", Address, registrationResult);
			if (registrationResult.Contains("success"))
			{
				RegisterStatusServer();
			}
			else
			{
				Debug.Console(0, this, "EvertzEndpointStatusServer RegisterServerWithEndpoint with device {0}\n{1}", Address, registrationResult);

			}
			 * */
		}

		public void PollAll()
		{
			string collection = "";
			foreach (var parameter in Ports)
			{
				if (parameter.Key.Contains("@"))
				{
					collection = collection + parameter.Key + ",";
				}
			}
			collection = collection.Substring(0, collection.Length - 1);
			SendGetRequest(collection);
		}
		public void PollForState(string varId)
		{
			try
			{
				SendGetRequest(varId);
				//var returnState = JsonConvert.DeserializeObject<EvertzPortRestResponse>(SendGetRequest(varId));

			}
			catch (Exception e)
			{
				Debug.Console(0, this, "PollForState {0}", e);
				
			}	
		}


		public void ProcessGetParameterResponse(EvertzPortRestResponse response)
		{
			var PortObject = Ports[response.id];
			if (response.name == "Input Status")
			{
				if (response.value == "Missing") { PortObject.SyncDetected = false; }
				else { PortObject.SyncDetected = true; } 
			}
		}
		public void SendGetRequest(string s)
		{
			Client.SendText("v.api/apis/EV/GET/parameter/{0}", s);
		}
		
		public void SendStatusRequest()
		{
			Client.SendText("/v.api/apis/EV/SERVERSTATUS");
		}
		public void RegisterServer(string hostname, string servername, string port)
		{
			Client.SendText("v.api/apis/EV/SERVERADD/server/{0}/{1}/{2}/udp", hostname, servername, port);
		}
		public void UnregisterServer()
		{
			if (StatusServerId != null)
			{
				Client.SendTextNoResponse("v.api/apis/EV/SERVERDEL/server/{0}", StatusServerId);
			}
		}

		// TODO JTA: Craete a UnregisterServerFast using DispatchASync. 
		public void RegisterForNotification(string varId)
		{
			Client.SendText("v.api/apis/EV/NOTIFYADD/parameter/{0}/{1}", StatusServerId, varId);
		}


		void Client_ResponseRecived(object sender, GenericHttpClientEventArgs e)
		{
			if (e.Error == HTTP_CALLBACK_ERROR.COMPLETED)
			{
				if (e.RequestPath.Contains("GET/parameter/"))
				{
					// Get Parameter response
					if (!e.ResponseText.Contains("["))
						ProcessGetParameterResponse(JsonConvert.DeserializeObject<EvertzPortRestResponse>(e.ResponseText));
					else if (e.ResponseText.Contains("["))
					{
						List<EvertzPortRestResponse> test = JsonConvert.DeserializeObject<List<EvertzPortRestResponse>>(e.ResponseText);
						foreach (var thing in test)
						{
							ProcessGetParameterResponse(thing);
						}

					}
				}
				else if (e.RequestPath.Contains("SERVERSTATUS"))
				{
					PollAll();
					ProcessServerStatusRequest(JsonConvert.DeserializeObject<EvertsStatusRequesstResponse>(e.ResponseText));
				}
			}
		}




		public class EvertzPortsRestResponse
		{
			List<EvertzPortRestResponse> test;
		}
		public class EvertzPortRestResponse
		{
			public string id;
			public string name;
			public string type;
			public string value;

		}

		public class EvertzEndpointPort
		{
			public string PortName;
			public string SyncVarID;
			public string ResolutionVarID;
			public bool SyncDetected;
			public string Resolution;
			public BoolFeedback SyncDetectedFeedback;

			public EvertzEndpointPort (string portName, string syncVarId, string resolutionVarId)
			{
				PortName = portName;
				SyncVarID = syncVarId;
				ResolutionVarID = resolutionVarId;
				SyncDetectedFeedback = new BoolFeedback(() => { return SyncDetected; });
			}

		}

    }
}