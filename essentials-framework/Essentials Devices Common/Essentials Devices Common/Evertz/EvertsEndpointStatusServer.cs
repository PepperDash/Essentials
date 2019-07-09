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
using Newtonsoft.Json.Serialization;

using Crestron.SimplSharp.CrestronSockets;


namespace PepperDash.Essentials.Devices.Common
{

	/*****
	 * TODO JTA: Add Polling
	 * TODO JTA: Move all the registration commnads to the EvertEndpoint class. 
	 * 
	 * 
	 * 
	 * 
	 * 
	 */ 

    public class EvertzEndpointStatusServer : Device
    {
        public IBasicCommunication Communication { get; private set; }
        public CommunicationGather PortGather { get; private set; }
        public StatusMonitorBase CommunicationMonitor { get; private set; }
        public bool isSubscribed;
		public string Address;
		public GenericUdpServer Server;



        /// <summary>
        /// Shows received lines as hex
        /// </summary>
        public bool ShowHexResponse { get; set; }
		public Dictionary<string, EvertzEndpoint> Endpoints;
		public Dictionary<string, string> ServerIdByEndpointIp;

		public EvertzEndpointStatusServer(string key, string name, GenericUdpServer server, EvertzEndpointStatusServerPropertiesConfig props) :
            base(key, name)
        {
			Server = server;
			Address = props.serverHostname;
			Server.DataRecievedExtra += new EventHandler<GenericUdpReceiveTextExtraArgs>(_Server_DataRecievedExtra);

			Server.Connect();
			Endpoints = new Dictionary<string,EvertzEndpoint>();
			
			//CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(CrestronEnvironment_ProgramStatusEventHandler);
        }



		//TODO JTA: Move this method and process over to the endpoint itself. return a bool. 
		public bool RegisterEvertzEndpoint (EvertzEndpoint device)
		{

			if (Endpoints.ContainsKey(device.Address) == false)
			{
				Endpoints.Add(device.Address, device);
			}

			return true;
		}




		void _Server_DataRecievedExtra(object sender, GenericUdpReceiveTextExtraArgs e)
		{
			Debug.Console(2, this, "_Server_DataRecievedExtra:\nIP:{0}\nPort:{1}\nText{2}\nBytes:{3} ", e.IpAddress, e.Port, e.Text, e.Bytes);
		}

        public override bool CustomActivate()
        {
			/*
			Communication.Connect();
			CommunicationMonitor.StatusChange += (o, a) => { Debug.Console(2, this, "Communication monitor state: {0}", CommunicationMonitor.Status); };
			CommunicationMonitor.Start();
			*/ 





            return true;
        }

	}











		public class EvertzPortRestResponse
		{
			public string id;
			public string name;
			public string type;
			public string value;

		}

		public class EvertsStatusRequesstResponse
		{
			public EvertzStatusDataResponse data;
			public string error;
		}

		public class EvertzStatusDataResponse
		{
			public List<EvertzServerStatusResponse> servers;
		}

		public class EvertzServerStatusResponse
		{
			public string id;
			public string name;
			public EvertsServerStausNotificationsResponse notify;

		}
		public class EvertsServerStausNotificationsResponse
		{
			public string ip;
			public List<string> parameters;
			public string port;
			public string protocol;
		}
		public class EvertzEndpointStatusServerPropertiesConfig
		{

			public ControlPropertiesConfig control { get; set; }
			public string serverHostname { get; set; }

		}

    }
