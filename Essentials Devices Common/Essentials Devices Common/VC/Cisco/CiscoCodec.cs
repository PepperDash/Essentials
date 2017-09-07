using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Https;
using Newtonsoft.Json;
using Cisco_One_Button_To_Push;
using Cisco_SX80_Corporate_Phone_Book;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.VideoCodec.Cisco
{
    public class CiscoCodec : VideoCodecBase
    {
        public IBasicCommunication Communication { get; private set; }

        public StatusMonitorBase CommunicationMonitor { get; private set; }

        private CiscoOneButtonToPush CodecObtp;

        private Corporate_Phone_Book PhoneBook;

        private HttpsClient Client;

        private HttpApiServer Server;

        // Constructor for IBasicCommunication
        public CiscoCodec(string key, string name, IBasicCommunication comm)
            : base(key, name)
        {
            Communication = comm;
            Communication.TextReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(Communication_TextReceived);

            CodecObtp = new CiscoOneButtonToPush();

            PhoneBook = new Corporate_Phone_Book();

            Client = new HttpsClient();

            Server = new HttpApiServer();
      
        }
        public override bool CustomActivate()
        {
            Debug.Console(1, this, "Starting Cisco API Server");

            Server.Start(8080);

            Server.ApiRequest += new EventHandler<Crestron.SimplSharp.Net.Http.OnHttpRequestArgs>(Server_ApiRequest);

            CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 2000, 120000, 300000, "xStatus SystemUnit Software Version\r");
            DeviceManager.AddDevice(CommunicationMonitor);

            CodecObtp.Initialize();

            CodecObtp.GetMeetings();

            return base.CustomActivate();
        }

        void Server_ApiRequest(object sender, Crestron.SimplSharp.Net.Http.OnHttpRequestArgs e)
        {
            Debug.Console(1, this, "Api Reqeust from Codec: {0}", e.Request.ContentString);
            e.Response.Code = 200;
            e.Response.ContentString = "HelloWorld";
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

        protected override Func<bool> TransmitMuteFeedbackFunc { get { return () => false; } }

        protected override Func<bool> ReceiveMuteFeedbackFunc { get { return () => false; } }

        protected override Func<bool> PrivacyModeFeedbackFunc { get { return () => false; } }

        public override void Dial()
        {
            
        }

        public override void EndCall()
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