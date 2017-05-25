using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharp.Net.Http;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.PageManagers;

namespace PepperDash.Essentials
{
    public class CotijaInterfaceController : Device
    {
        StreamReader Reader;

        Crestron.SimplSharp.Net.Connection Connection;

        public CotijaInterfaceController(string key) : base(key)
        {
            CrestronConsole.AddNewConsoleCommand(InitializeClientAndEventStream, "InitializeHttpClient", "Initializes a new HTTP client connection to a specified URL", ConsoleAccessLevelEnum.AccessOperator);
        }

        public void InitializeClientAndEventStream(string url)
        {
            try
            {
                HttpClient webClient = new HttpClient();
                webClient.Verbose = true;
                HttpClientRequest request = new HttpClientRequest();
                request.Url = new UrlParser(url);
                request.Header.AddHeader(new HttpHeader("Accept", "text/event-stream"));
                request.KeepAlive = true;

                /// Experimenting with grabbing connection before the request
                Connection = webClient.ConnectNew("192.168.1.120", 3000);
                Debug.Console(1, this, "Connection Port: {0}", Connection.LocalEndPointPort);
                Reader = new StreamReader(Connection);
                Connection.OnBytesReceived += new EventHandler(DataConnection_OnBytesReceived);

                /// Not sure if this is starting a new connection
                webClient.DispatchAsync(request, StreamConnectionCallback);

            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error Initializing Cotija client:\n{0}", e);
            }
        }

        // This callback happens but none of the events would fire (when not commented out)
        void StreamConnectionCallback(HttpClientResponse resp, HTTP_CALLBACK_ERROR err)
        {
            Debug.Console(1, this, "Connection Port: {0}", resp.DataConnection.LocalEndPointPort);
            Debug.Console(1, this, "Connections are equal {0}", resp.DataConnection == Connection);
            Debug.Console(1, this, "Callback Fired");
            //resp.DataConnection.OnBytesReceived += new EventHandler(DataConnection_OnBytesReceived);
            //Debug.Console(1, this, "Received: '{0}'", resp.ContentString);
            //resp.OnTransferEnd += new Crestron.SimplSharp.Net.TransferEndEventHandler(resp_OnTransferEnd);
            //resp.OnTransferProgress += new Crestron.SimplSharp.Net.TransferProgressEventHandler(resp_OnTransferProgress);
            //resp.OnTransferStart += new Crestron.SimplSharp.Net.TransferStartEventHandler(resp_OnTransferStart);
        }

        // We could never get this event handler to fire
        void DataConnection_OnBytesReceived(object sender, EventArgs e)
        {
            Debug.Console(1, this, "OnBytesReceived Event Fired.");
            Debug.Console(1, this, "Event: Received: '{0}'", Reader.ReadToEnd());
        }

        //void resp_OnTransferStart(object sender, Crestron.SimplSharp.Net.TransferStartEventArgs e)
        //{
        //    Debug.Console(1, this, "OnTransferStart");
        //}

        //void resp_OnTransferProgress(object sender, Crestron.SimplSharp.Net.TransferProgressEventArgs e)
        //{
        //    Debug.Console(1, this, "OnTransferProgress");
        //}

 

        //void resp_OnTransferEnd(object sender, Crestron.SimplSharp.Net.TransferEndEventArgs e)
        //{
        //    Debug.Console(1, this, "OnTransferEnd");
        //}
    }
}