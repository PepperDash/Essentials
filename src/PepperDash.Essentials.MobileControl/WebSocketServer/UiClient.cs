using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.RoomBridges;
using Serilog.Events;
using System;
using System.Text.RegularExpressions;
using WebSocketSharp;
using WebSocketSharp.Server;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;


namespace PepperDash.Essentials.WebSocketServer
{
    /// <summary>
    /// Represents the behaviour to associate with a UiClient for WebSocket communication
    /// </summary>
    public class UiClient : WebSocketBehavior
    {
        public MobileControlSystemController Controller { get; set; }

        public string RoomKey { get; set; }

        private string _clientId;

        private DateTime _connectionTime;

        public TimeSpan ConnectedDuration
        {
            get
            {
                if (Context.WebSocket.IsAlive)
                {
                    return DateTime.Now - _connectionTime;
                }
                else
                {
                    return new TimeSpan(0);
                }
            }
        }

        public UiClient()
        {

        }

        protected override void OnOpen()
        {
            base.OnOpen();

            var url = Context.WebSocket.Url;
            Debug.LogMessage(LogEventLevel.Verbose, "New WebSocket Connection from: {0}", null, url);

            var match = Regex.Match(url.AbsoluteUri, "(?:ws|wss):\\/\\/.*(?:\\/mc\\/api\\/ui\\/join\\/)(.*)");

            if (!match.Success)
            {
                _connectionTime = DateTime.Now;
                return;
            }

            var clientId = match.Groups[1].Value;
            _clientId = clientId;

            if (Controller == null)
            {
                Debug.LogMessage(LogEventLevel.Verbose, "WebSocket UiClient Controller is null");
                _connectionTime = DateTime.Now;
            }

            var clientJoinedMessage = new MobileControlMessage
            {
                Type = "/system/clientJoined",
                Content = JToken.FromObject(new
                {
                    clientId,
                    roomKey = RoomKey,
                })
            };

            Controller.HandleClientMessage(JsonConvert.SerializeObject(clientJoinedMessage));

            var bridge = Controller.GetRoomBridge(RoomKey);

            if (bridge == null) return;

            SendUserCodeToClient(bridge, clientId);

            bridge.UserCodeChanged -= Bridge_UserCodeChanged;
            bridge.UserCodeChanged += Bridge_UserCodeChanged;

            // TODO: Future: Check token to see if there's already an open session using that token and reject/close the session 
        }

        private void Bridge_UserCodeChanged(object sender, EventArgs e)
        {
            SendUserCodeToClient((MobileControlEssentialsRoomBridge)sender, _clientId);
        }

        private void SendUserCodeToClient(MobileControlBridgeBase bridge, string clientId)
        {
            var content = new
            {
                userCode = bridge.UserCode,
                qrUrl = bridge.QrCodeUrl,
            };

            var message = new MobileControlMessage
            {
                Type = "/system/userCodeChanged",
                ClientId = clientId,
                Content = JToken.FromObject(content)
            };

            Controller.SendMessageObjectToDirectClient(message);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            if (e.IsText && e.Data.Length > 0 && Controller != null)
            {
                // Forward the message to the controller to be put on the receive queue
                Controller.HandleClientMessage(e.Data);
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);

            Debug.LogMessage(LogEventLevel.Verbose, "WebSocket UiClient Closing: {0} reason: {1}", null, e.Code, e.Reason);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);

            Debug.LogMessage(LogEventLevel.Verbose, "WebSocket UiClient Error: {exception} message: {message}", e.Exception, e.Message);
        }
    }
}
