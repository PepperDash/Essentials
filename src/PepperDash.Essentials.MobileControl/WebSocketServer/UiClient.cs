using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.RoomBridges;
using Serilog.Events;
using WebSocketSharp;
using WebSocketSharp.Server;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;


namespace PepperDash.Essentials.WebSocketServer
{
    /// <summary>
    /// Represents the behaviour to associate with a UiClient for WebSocket communication
    /// </summary>
    public class UiClient : WebSocketBehavior, IKeyed
    {
        /// <inheritdoc />
        public string Key { get; private set; }

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

        public UiClient(string key)
        {
            Key = key;
        }

        /// <inheritdoc />
        protected override void OnOpen()
        {
            base.OnOpen();

            var url = Context.WebSocket.Url;
            this.LogInformation("New WebSocket Connection from: {url}", url);

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

        /// <inheritdoc />
        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            if (e.IsText && e.Data.Length > 0 && Controller != null)
            {
                // Forward the message to the controller to be put on the receive queue
                Controller.HandleClientMessage(e.Data);
            }
        }

        /// <inheritdoc />
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);

            this.LogInformation("WebSocket UiClient Closing: {code} reason: {reason}", e.Code, e.Reason);
        }

        /// <inheritdoc />
        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);

            this.LogError("WebSocket UiClient Error: {message}", e.Message);
            this.LogDebug(e.Exception, "Stack Trace");
        }
    }
}
