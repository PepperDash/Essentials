using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Https;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXml.Serialization;
using Newtonsoft.Json;
using Cisco_One_Button_To_Push;
using Cisco_SX80_Corporate_Phone_Book;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Cisco
{
    enum eCommandType { SessionStart, SessionEnd, Command, GetStatus, GetConfiguration };

    public class CiscoCodec : VideoCodecBase
    {
        public IBasicCommunication Communication { get; private set; }
        public CommunicationGather PortGather { get; private set; }

        public StatusMonitorBase CommunicationMonitor { get; private set; }

        private CiscoOneButtonToPush CodecObtp;

        private Corporate_Phone_Book PhoneBook;

        private CiscoCodecConfiguration.Configuration CodecConfiguration;

        private CiscoCodecStatus.Status CodecStatus;

        private HttpsClient Client;

        private HttpApiServer Server;

        private int ServerPort;

        private string CodecUrl;

        private string HttpSessionId;

        private string FeedbackRegistrationExpression;

        // Constructor for IBasicCommunication
        public CiscoCodec(string key, string name, IBasicCommunication comm, int serverPort)
            : base(key, name)
        {
            Communication = comm;

            PortGather = new CommunicationGather(Communication, "\x0d\x0a");
            PortGather.LineReceived += this.Port_LineReceived;
  
            Communication.TextReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(Communication_TextReceived);

            ServerPort = serverPort;

            CodecObtp = new CiscoOneButtonToPush();

            PhoneBook = new Corporate_Phone_Book();

            CodecConfiguration = new CiscoCodecConfiguration.Configuration();

            CodecStatus = new CiscoCodecStatus.Status();

            Client = new HttpsClient();

            Server = new HttpApiServer();
      
        }



        /// <summary>
        /// Starts the HTTP feedback server and syncronizes state of codec
        /// </summary>
        /// <returns></returns>
        public override bool CustomActivate()
        {
            CrestronConsole.AddNewConsoleCommand(SendText, "send" + Key, "", ConsoleAccessLevelEnum.AccessOperator);

            Communication.Connect();

            Debug.Console(1, this, "Starting Cisco API Server");

            Server.Start(ServerPort);

            Server.ApiRequest += new EventHandler<Crestron.SimplSharp.Net.Http.OnHttpRequestArgs>(Server_ApiRequest);

            CodecUrl = string.Format("http://{0}", (Communication as GenericSshClient).Hostname);

            CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 2000, 120000, 300000, "xStatus SystemUnit Software Version\r");
            DeviceManager.AddDevice(CommunicationMonitor);

            Client = new HttpsClient();

            Client.Verbose = true;
            Client.KeepAlive = true;


            // Temp feedback registration

            FeedbackRegistrationExpression =
                "<Command><HttpFeedback><Register command=\"True\"><FeedbackSlot>1</FeedbackSlot>" +
                string.Format("<ServerUrl>http://{0}:{1}/cisco/api</ServerUrl>", CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0), ServerPort) +
                "<Format>JSON</Format>" +
                "<Expression item=\"1\">/Configuration</Expression>" +
                "<Expression item=\"2\">/Event/CallDisconnect</Expression>" +
                "<Expression item=\"3\">/Status/Call</Expression>" +
                "</Register>" +
                "</HttpFeedback>" +
                "</Command>";

            StartHttpsSession();

            //CodecObtp.Initialize();

            //CodecObtp.GetMeetings();

            //PhoneBook.DownloadPhoneBook(Corporate_Phone_Book.ePhoneBookLocation.Corporate);         

            return base.CustomActivate();
        }

        void Port_LineReceived(object dev, GenericCommMethodReceiveTextArgs args)
        {
            if (Debug.Level == 1)
                Debug.Console(1, this, "RX: '{0}'",
                    ComTextHelper.GetEscapedText(args.Text));
        }

        public void SendText(string command)
        {
            Debug.Console(1, this, "Sending: '{{0}'", command);
            Communication.SendText(command + "\0xd\0xa");
        }

        private void StartHttpsSession()
        {
            SendHttpCommand("", eCommandType.SessionStart);
        }

        private void EndHttpsSession()
        {
            SendHttpCommand("", eCommandType.SessionEnd);
        }

        private void SendHttpCommand(string command, eCommandType commandType)
        {
            HttpsClientRequest request = new HttpsClientRequest();

            string urlSuffix = null;

            Client.UserName = null;
            Client.Password = null;

            Client.PeerVerification = false;
            Client.HostVerification = false;

            request.RequestType = RequestType.Post;

            if(!string.IsNullOrEmpty(HttpSessionId))
                request.Header.SetHeaderValue("Cookie", HttpSessionId);

            switch (commandType)
            {
                case eCommandType.Command:
                    {
                        urlSuffix = "/putxml";
                        request.ContentString = command;
                        request.Header.SetHeaderValue("Content-Type", "text/xml");
                        break;
                    }
                case eCommandType.SessionStart:
                    {
                        
                        urlSuffix = "/xmlapi/session/begin";

                        Client.UserName = (Communication as GenericSshClient).Username;
                        Client.Password = (Communication as GenericSshClient).Password;

                        break;
                    }
                case eCommandType.SessionEnd:
                    {
                        urlSuffix = "/xmlapi/session/end";
                        request.Header.SetHeaderValue("Cookie", HttpSessionId);
                        break;
                    }
                case eCommandType.GetStatus:
                    {
                        request.RequestType = RequestType.Get;
                        request.Header.SetHeaderValue("Content-Type", "text/xml");
                        urlSuffix = "/getxml?location=/Status";
                        break;
                    }
                case eCommandType.GetConfiguration:
                    {
                        request.RequestType = RequestType.Get;
                        request.Header.SetHeaderValue("Content-Type", "text/xml");
                        urlSuffix = "/getxml?location=/Configuration";
                        break;
                    }
            }

            var requestUrl = CodecUrl + urlSuffix;
            request.Header.RequestVersion = "HTTP/1.1";
            request.Url.Parse(requestUrl);

            Debug.Console(1, this, "Sending HTTP request to Cisco Codec at {0}\nHeader:\n{1}\nContent:\n{2}", requestUrl, request.Header, request.ContentString);

            Client.DispatchAsync(request, PostConnectionCallback);
        }

        void PostConnectionCallback(HttpsClientResponse resp, HTTPS_CALLBACK_ERROR err)
        {
            try
            {
                if (resp != null)
                {
                    if (resp.Code == 200)
                    {
                        Debug.Console(1, this, "Http Post to Cisco Codec Successful. Code: {0}\nContent: {1}", resp.Code, resp.ContentString);

                        if (resp.ContentString.IndexOf("<HttpFeedbackRegisterResult status=\"OK\">") > 1)
                        {
                            // Get the initial configruation for sync purposes
                            SendHttpCommand("", eCommandType.GetConfiguration);
                        }
                        else
                        {
                            try
                            {                               
                                if (resp.ContentString.IndexOf("</Configuration>") > -1)
                                {
                                    XmlReaderSettings settings = new XmlReaderSettings();

                                    XmlReader reader = new XmlReader(resp.ContentString, settings);

                                    CodecConfiguration = CrestronXMLSerialization.DeSerializeObject<CiscoCodecConfiguration.Configuration>(reader);

                                    Debug.Console(1, this, "Product Name: {0} Software Version: {1} ApiVersion: {2}", CodecConfiguration.Product, CodecConfiguration.Version, CodecConfiguration.ApiVersion);

                                    // Get the initial status for sync purposes
                                    SendHttpCommand("", eCommandType.GetStatus);
                                }
                                else if (resp.ContentString.IndexOf("</Status>") > -1)
                                {
                                    XmlReaderSettings settings = new XmlReaderSettings();

                                    XmlReader reader = new XmlReader(resp.ContentString, settings);

                                    CodecStatus = CrestronXMLSerialization.DeSerializeObject<CiscoCodecStatus.Status>(reader);

                                    Debug.Console(1, this, "Product Name: {0} Software Version: {1} ApiVersion: {2} Volume: {3}", CodecStatus.Product, CodecStatus.Version, CodecStatus.ApiVersion, CodecStatus.Audio.Volume);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.Console(1, this, "Error Deserializing XML document from codec: {0}", ex);
                            }
                        }
                    }
                    else if (resp.Code == 204)
                    {
                        Debug.Console(1, this, "Response Code: {0}\nHeader:\n{1}Content:\n{1}", resp.Code, resp.Header, resp.ContentString);

                        HttpSessionId = resp.Header.GetHeaderValue("Set-Cookie");
                        //var chunks = HttpSessionId.Split(';');
                        //HttpSessionId = chunks[0];
                        //HttpSessionId = HttpSessionId.Substring(HttpSessionId.IndexOf("=") + 1);


                        // Register for feedbacks once we have a valid session
                        SendHttpCommand(FeedbackRegistrationExpression, eCommandType.Command);
                    }
                    else
                    {
                        Debug.Console(1, this, "Response Code: {0}\nHeader:\n{1}Content:\n{1}Err:\n{2}", resp.Code, resp.Header, resp.ContentString, err);
                    }
                }
                else
                    Debug.Console(1, this, "Null response received from server");
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error Initializing HTTPS Client: {0}", e);
            }
        }

        void Server_ApiRequest(object sender, Crestron.SimplSharp.Net.Http.OnHttpRequestArgs e)
        {
            Debug.Console(1, this, "Api Reqeust from Codec: {0}", e.Request.ContentString);
            e.Response.Code = 200;
            e.Response.ContentString = "OK";

            try
            {
                // Serializer settings.  We want to ignore null values and mising members
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                settings.MissingMemberHandling = MissingMemberHandling.Ignore;

                if (e.Request.ContentString.IndexOf("\"Configuration\":{") > -1)
                    CodecConfiguration = JsonConvert.DeserializeObject<CiscoCodecConfiguration.Configuration>(e.Request.ContentString, settings);
                else if (e.Request.ContentString.IndexOf("\"Status\":{") > -1)
                    CodecStatus = JsonConvert.DeserializeObject<CiscoCodecStatus.Status>(e.Request.ContentString, settings);
            }
            catch (Exception ex)
            {
                Debug.Console(1, this, "Error Deserializing feedback from codec: {0}", ex);
            }
        }

        void Communication_TextReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            //CodecObtp.
        }

        public override void ExecuteSwitch(object selector)
        {
            throw new NotImplementedException();
        }

        protected override Func<bool> InCallFeedbackFunc { get { return () => false; } }

        protected override Func<bool> IncomingCallFeedbackFunc { get { return () => false; } }

        protected override Func<bool> TransmitMuteFeedbackFunc { get { return () => false; } }

        protected override Func<bool> ReceiveMuteFeedbackFunc { get { return () => false; } }

        protected override Func<bool> PrivacyModeFeedbackFunc { get { return () => false; } }

        public override void  Dial(string s)
        {
         	
        }  
 
        public override void EndCall()
        {
            
        }

        public override void AcceptCall()
        {

        }

        public override void RejectCall()
        {

        }

        public override void SendDtmf(string s)
        {
            
        }

        public override void StartSharing()
        {

        }

        public override void StopSharing()
        {

        }

        public override void ReceiveMuteOff()
        {
            
        }

        public override void ReceiveMuteOn()
        {
            
        }

        public override void ReceiveMuteToggle()
        {
            
        }

        public override void SetReceiveVolume(ushort level)
        {
            
        }

        public override void TransmitMuteOff()
        {
            
        }

        public override void TransmitMuteOn()
        {
            
        }

        public override void TransmitMuteToggle()
        {
            
        }

        public override void SetTransmitVolume(ushort level)
        {
            
        }

        public override void PrivacyModeOn()
        {
            
        }

        public override void PrivacyModeOff()
        {
            
        }

        public override void PrivacyModeToggle()
        {
            
        }
    }
}