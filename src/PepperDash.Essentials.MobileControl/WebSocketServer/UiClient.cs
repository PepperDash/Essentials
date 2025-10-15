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

        /// <summary>
        /// Gets or sets the mobile control system controller that handles this client's messages
        /// </summary>
        public MobileControlSystemController Controller { get; set; }

        /// <summary>
        /// Gets or sets the room key that this client is associated with
        /// </summary>
        public string RoomKey { get; set; }

        /// <summary>
        /// The unique identifier for this client instance
        /// </summary>
        private string _clientId;

        /// <summary>
        /// The timestamp when this client connection was established
        /// </summary>
        private DateTime _connectionTime;

        /// <summary>
        /// Gets the duration that this client has been connected. Returns zero if not currently connected.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the UiClient class with the specified key
        /// </summary>
        /// <param name="key">The unique key to identify this client</param>
        /// <param name="id">The client ID used by the client for this connection</param>
        public UiClient(string key, string id)
        {
            Key = key;
            Id = id;
        }

        /// <inheritdoc />
        protected override void OnOpen()
        {
            base.OnOpen();

            Log.Output = (data, message) => Utilities.ConvertWebsocketLog(data, message);
            Log.Level = LogLevel.Trace;

            try
            {
                this.LogDebug("Current session count on open {count}", Sessions.Count);
                this.LogDebug("Current WebsocketServiceCount on open: {count}", Controller.DirectServer.WebsocketServiceCount);
            }
            catch (Exception ex)
            {
                this.LogError("Error getting service count: {message}", ex.Message);
                this.LogDebug(ex, "Stack Trace: ");
            }

            // var url = Context.WebSocket.Url;
            // this.LogInformation("New WebSocket Connection from: {url}", url);

            // var match = Regex.Match(url.AbsoluteUri, "(?:ws|wss):\\/\\/.*(?:\\/mc\\/api\\/ui\\/join\\/)(.*)");

            // if (!match.Success)
            // {
            //     _connectionTime = DateTime.Now;
            //     return;
            // }

            // var clientId = match.Groups[1].Value;
            // _clientId = clientId;

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
                    clientId = Id,
                    roomKey = RoomKey,
                })
            };

            Controller.HandleClientMessage(JsonConvert.SerializeObject(clientJoinedMessage));

            var bridge = Controller.GetRoomBridge(RoomKey);

            if (bridge == null) return;

            SendUserCodeToClient(bridge, Id);

            bridge.UserCodeChanged -= Bridge_UserCodeChanged;
            bridge.UserCodeChanged += Bridge_UserCodeChanged;

            // TODO: Future: Check token to see if there's already an open session using that token and reject/close the session 
        }

        /// <summary>
        /// Handles the UserCodeChanged event from a room bridge and sends the updated user code to the client
        /// </summary>
        /// <param name="sender">The room bridge that raised the event</param>
        /// <param name="e">Event arguments</param>
        private void Bridge_UserCodeChanged(object sender, EventArgs e)
        {
            SendUserCodeToClient((MobileControlEssentialsRoomBridge)sender, _clientId);
        }

        /// <summary>
        /// Sends the current user code and QR code URL to the specified client
        /// </summary>
        /// <param name="bridge">The room bridge containing the user code information</param>
        /// <param name="clientId">The ID of the client to send the information to</param>
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

            try
            {
                this.LogDebug("Current session count on close {count}", Sessions.Count);
                this.LogDebug("Current WebsocketServiceCount on close: {count}", Controller.DirectServer.WebsocketServiceCount);
            }
            catch (Exception ex)
            {
                this.LogError("Error getting service count: {message}", ex.Message);
                this.LogDebug(ex, "Stack Trace: ");
            }

            this.LogInformation("WebSocket UiClient Closing: {code} reason: {reason}", e.Code, e.Reason);

            foreach (var messenger in Controller.Messengers)
            {
                messenger.Value.UnsubscribeClient(Id);
            }

            foreach (var messenger in Controller.DefaultMessengers)
            {
                messenger.Value.UnsubscribeClient(Id);
            }

            ConnectionClosed?.Invoke(this, new ConnectionClosedEventArgs(Id));
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
