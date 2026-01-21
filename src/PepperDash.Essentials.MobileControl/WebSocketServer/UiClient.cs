using System;
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
        /// Client ID used by client for this connection
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Updates the client ID - only accessible from within the assembly (e.g., by the server)
        /// </summary>
        /// <param name="newId">The new client ID</param>
        internal void UpdateId(string newId)
        {
            Id = newId;
        }

        /// <summary>
        /// Token associated with this client
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// The URL token key used to connect (from UiClientContexts dictionary key)
        /// </summary>
        public string TokenKey { get; set; }

        /// <summary>
        /// Touchpanel Key associated with this client
        /// </summary>
        public string TouchpanelKey { get; private set; }

        /// <summary>
        /// Gets or sets the mobile control system controller that handles this client's messages
        /// </summary>
        public MobileControlSystemController Controller { get; set; }

        /// <summary>
        /// Gets or sets the server instance for client registration
        /// </summary>
        public MobileControlWebsocketServer Server { get; set; }

        /// <summary>
        /// Gets or sets the room key that this client is associated with
        /// </summary>
        public string RoomKey { get; set; }

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
        /// Triggered when this client closes it's connection
        /// </summary>
        public event EventHandler<ConnectionClosedEventArgs> ConnectionClosed;

        /// <summary>
        /// Initializes a new instance of the UiClient class with the specified key
        /// </summary>
        /// <param name="key">The unique key to identify this client</param>
        /// <param name="id">The client ID used by the client for this connection</param>
        /// <param name="token">The token associated with this client</param>
        /// <param name="touchpanelKey">The touchpanel key associated with this client</param>
        public UiClient(string key, string id, string token, string touchpanelKey = "")
        {
            Key = key;
            Id = id;
            Token = token;
            TouchpanelKey = touchpanelKey;
        }

        /// <inheritdoc />
        protected override void OnOpen()
        {
            base.OnOpen();

            _connectionTime = DateTime.Now;

            Log.Output = (data, message) => Utilities.ConvertWebsocketLog(data, message, this);
            Log.Level = LogLevel.Trace;

            // Get clientId from query parameter
            var queryString = Context.QueryString;
            var clientId = queryString["clientId"];

            if (!string.IsNullOrEmpty(clientId))
            {
                // New behavior: Validate and register with the server using provided clientId
                if (Server == null || !Server.RegisterUiClient(this, clientId, TokenKey))
                {
                    this.LogError("Failed to register client with ID {clientId}. Invalid or expired registration.", clientId);
                    Context.WebSocket.Close(CloseStatusCode.PolicyViolation, "Invalid or expired clientId");
                    return;
                }

                // Update this client's ID to the validated one
                Id = clientId;
                Key = $"uiclient-{TokenKey}-{RoomKey}-{clientId}";

                this.LogInformation("Client {clientId} successfully connected and registered (new flow)", clientId);
            }
            else
            {
                // Legacy behavior: Use clientId from Token.Id (generated in HandleJoinRequest)
                this.LogInformation("Client connected without clientId query parameter. Using legacy registration flow.");

                // Id is already set from Token in constructor, use it
                if (string.IsNullOrEmpty(Id))
                {
                    this.LogError("Legacy client has no ID from token. Connection will be closed.");
                    Context.WebSocket.Close(CloseStatusCode.PolicyViolation, "No client ID available");
                    return;
                }

                Key = $"uiclient-{TokenKey}-{RoomKey}-{Id}";

                // Register directly to active clients (legacy flow)
                if (Server != null)
                {
                    Server.RegisterLegacyUiClient(this);
                }

                this.LogInformation("Client {clientId} registered using legacy flow", Id);
            }

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
                    touchpanelKey = TouchpanelKey ?? string.Empty,
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
            SendUserCodeToClient((MobileControlEssentialsRoomBridge)sender, Id);
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
