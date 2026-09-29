using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crestron.SimplSharp.CrestronSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using Serilog.Events;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace PepperDash.Essentials.Core.Web.ConsoleSession
{
    /// <summary>
    /// One browser connection to the console session server, backed by one loopback SSH session.
    /// </summary>
    /// <remarks>
    /// Messages are JSON text frames.
    /// Browser to server: {"type":"connect","username":"","password":""}, {"type":"send","data":"ver\r\n"}, {"type":"disconnect"}.
    /// Server to browser: {"type":"status","state":"...","message":"..."}, {"type":"data","data":"..."}.
    /// Console output is never logged here: Essentials log output appears on the console being
    /// streamed, so logging per chunk would feed back into the stream.
    /// </remarks>
    public class ConsoleSessionClient : WebSocketBehavior
    {
        private const string LoopbackHost = "127.0.0.1";
        private const int SshPort = 22;

        // Console output is batched so bursts become a few WebSocket frames instead of one per SSH read
        private const int FlushIntervalMs = 20;
        private const int MaxPendingChars = 1000000;

        private readonly object _sshLock = new object();
        private readonly object _pendingLock = new object();
        private readonly StringBuilder _pending = new StringBuilder();
        private readonly Timer _flushTimer;

        private GenericSshClient _ssh;
        private bool _overflowed;
        private bool _closed;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConsoleSessionClient()
        {
            _flushTimer = new Timer(o => Flush(), null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <inheritdoc />
        protected override void OnOpen()
        {
            base.OnOpen();

            Debug.LogMessage(LogEventLevel.Information, "Console session {id} opened", ID);

            SendStatus("idle", "WebSocket connected. Send credentials to open the SSH console.");
        }

        /// <inheritdoc />
        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            JObject message;

            try
            {
                message = JObject.Parse(e.Data);
            }
            catch (Exception)
            {
                SendStatus("error", "Message was not valid JSON");
                return;
            }

            switch ((string)message["type"])
            {
                case "connect":
                    var username = (string)message["username"];
                    var password = (string)message["password"];

                    // Connect blocks until the SSH handshake finishes, so keep it off the WebSocket thread
                    Task.Run(() => ConnectSsh(username, password));
                    break;
                case "send":
                    SendToConsole((string)message["data"]);
                    break;
                case "disconnect":
                    DisconnectSsh();
                    SendStatus("disconnected", "SSH console closed");
                    break;
                default:
                    SendStatus("error", $"Unknown message type '{message["type"]}'");
                    break;
            }
        }

        /// <inheritdoc />
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);

            Debug.LogMessage(LogEventLevel.Information, "Console session {id} closed: {code}", ID, e.Code);

            lock (_sshLock)
            {
                _closed = true;
            }

            DisconnectSsh();
            _flushTimer.Dispose();
        }

        /// <inheritdoc />
        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            base.OnError(e);

            Debug.LogMessage(LogEventLevel.Warning, "Console session {id} error: {message}", ID, e.Message);
        }

        private void ConnectSsh(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || password == null)
            {
                SendStatus("error", "Username and password are required");
                return;
            }

            GenericSshClient ssh;

            lock (_sshLock)
            {
                if (_closed) return;

                TearDownSsh();

                ssh = new GenericSshClient($"console-session-{ID}", LoopbackHost, SshPort, username, password)
                {
                    // Drops are reported to the browser instead of silently reconnecting
                    AutoReconnect = false
                };

                ssh.TextReceived += Ssh_TextReceived;
                ssh.ConnectionChange += Ssh_ConnectionChange;

                _ssh = ssh;
            }

            SendStatus("connecting", $"Opening SSH to {LoopbackHost}:{SshPort} as {username}");

            ssh.Connect();

            lock (_sshLock)
            {
                // The browser went away or asked for a new session while the handshake was running
                if (_closed || _ssh != ssh)
                {
                    ssh.TextReceived -= Ssh_TextReceived;
                    ssh.ConnectionChange -= Ssh_ConnectionChange;
                    ssh.Disconnect();
                    return;
                }
            }

            if (!ssh.IsConnected)
            {
                SendStatus("error", "SSH connect failed. Check credentials and the Essentials log for details.");
            }
        }

        private void DisconnectSsh()
        {
            lock (_sshLock)
            {
                TearDownSsh();
            }

            Flush();
        }

        // Caller holds _sshLock
        private void TearDownSsh()
        {
            if (_ssh == null) return;

            _ssh.TextReceived -= Ssh_TextReceived;
            _ssh.ConnectionChange -= Ssh_ConnectionChange;

            try
            {
                _ssh.Disconnect();
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception closing console session SSH");
            }

            _ssh = null;
        }

        private void SendToConsole(string data)
        {
            if (string.IsNullOrEmpty(data)) return;

            GenericSshClient ssh;

            lock (_sshLock)
            {
                ssh = _ssh;
            }

            if (ssh == null || !ssh.IsConnected)
            {
                SendStatus("error", "SSH console is not connected");
                return;
            }

            ssh.SendText(data);
        }

        private void Ssh_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e)
        {
            var status = e.Client.ClientStatus;

            switch (status)
            {
                case SocketStatus.SOCKET_STATUS_CONNECTED:
                    SendStatus("connected", "SSH console connected");
                    break;
                case SocketStatus.SOCKET_STATUS_WAITING:
                    break;
                default:
                    Flush();
                    SendStatus("disconnected", $"SSH console status: {status}");
                    break;
            }
        }

        // Runs on the SSH.NET receive thread: only buffer here, never block or log
        private void Ssh_TextReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            lock (_pendingLock)
            {
                if (_pending.Length + e.Text.Length > MaxPendingChars)
                {
                    _overflowed = true;
                    return;
                }

                var wasEmpty = _pending.Length == 0;

                _pending.Append(e.Text);

                if (wasEmpty)
                {
                    try
                    {
                        _flushTimer.Change(FlushIntervalMs, Timeout.Infinite);
                    }
                    catch (ObjectDisposedException)
                    {
                        // WebSocket already closed; nothing left to flush to
                    }
                }
            }
        }

        private void Flush()
        {
            string text;
            bool overflowed;

            lock (_pendingLock)
            {
                if (_pending.Length == 0 && !_overflowed) return;

                text = _pending.ToString();
                overflowed = _overflowed;

                _pending.Clear();
                _overflowed = false;
            }

            if (text.Length > 0)
            {
                SendJson(new { type = "data", data = text });
            }

            if (overflowed)
            {
                SendStatus("overflow", "Browser fell behind; some console output was dropped");
            }
        }

        private void SendStatus(string state, string message)
        {
            SendJson(new { type = "status", state, message });
        }

        private void SendJson(object payload)
        {
            try
            {
                if (Context.WebSocket.ReadyState != WebSocketState.Open) return;

                Send(JsonConvert.SerializeObject(payload));
            }
            catch (Exception)
            {
                // Deliberately silent: a log line here would be streamed back through this session
                // and fail again, looping every flush
            }
        }
    }
}
