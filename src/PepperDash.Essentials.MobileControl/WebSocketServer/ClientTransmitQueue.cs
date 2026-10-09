using System;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PepperDash.Essentials.WebSocketServer
{
    /// <summary>
    /// A message waiting to be sent to one client.
    /// </summary>
    public sealed class OutboundClientMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutboundClientMessage"/> class.
        /// </summary>
        /// <param name="payload">The serialized message.</param>
        /// <param name="type">The message type, used only for timing logs. May be null.</param>
        /// <param name="createdTimestamp">When the message was ready to queue (<see cref="Stopwatch.GetTimestamp"/>).</param>
        /// <param name="serializeMs">How long serializing the message took, used only for timing logs.</param>
        public OutboundClientMessage(string payload, string type, long createdTimestamp, double serializeMs = 0)
        {
            Payload = payload;
            Type = type;
            CreatedTimestamp = createdTimestamp;
            SerializeMs = serializeMs;
        }

        /// <summary>The serialized message.</summary>
        public string Payload { get; }

        /// <summary>The message type, used only for timing logs. May be null.</summary>
        public string Type { get; }

        /// <summary>When the message was ready to queue (<see cref="Stopwatch.GetTimestamp"/>).</summary>
        public long CreatedTimestamp { get; }

        /// <summary>How long serializing the message took, used only for timing logs.</summary>
        public double SerializeMs { get; }
    }

    /// <summary>
    /// One client's outbound messages, sent in order by a task of its own.
    /// </summary>
    /// <remarks>
    /// Sending to a websocket blocks until the data is written, and a client that reads slowly (a
    /// panel busy processing the previous messages, or a slow network) keeps it blocked. With one
    /// transmit thread for every client, that delays every other client too. Giving each client its own
    /// queue means a slow client only delays itself; <see cref="Enqueue"/> never blocks.
    /// </remarks>
    public sealed class ClientTransmitQueue : IDisposable
    {
        private readonly Channel<OutboundClientMessage> _channel =
            Channel.CreateUnbounded<OutboundClientMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
            });

        private readonly Action<string> _send;
        private readonly Func<bool> _canSend;
        private readonly Action<OutboundClientMessage, double, double> _onSent;
        private readonly Action<Exception> _onError;
        private readonly Action<OutboundClientMessage> _onDropped;

        /// <summary>
        /// Starts a queue that sends each message with <paramref name="send"/>, in order.
        /// </summary>
        /// <param name="send">Sends one message to the client. May block.</param>
        /// <param name="canSend">Whether the client can currently receive; messages are dropped while it can't.</param>
        /// <param name="onSent">Optional: called after each send with the message, ms it waited in the queue and ms the send took.</param>
        /// <param name="onError">Optional: called when a send throws. The queue keeps going.</param>
        /// <param name="onDropped">Optional: called for each message dropped because the client couldn't receive it.</param>
        public ClientTransmitQueue(
            Action<string> send,
            Func<bool> canSend,
            Action<OutboundClientMessage, double, double> onSent = null,
            Action<Exception> onError = null,
            Action<OutboundClientMessage> onDropped = null)
        {
            _send = send ?? throw new ArgumentNullException(nameof(send));
            _canSend = canSend ?? (() => true);
            _onSent = onSent;
            _onError = onError;
            _onDropped = onDropped;

            Completion = Task.Run(PumpAsync);
        }

        /// <summary>
        /// Completes once the queue has been disposed and has stopped.
        /// </summary>
        public Task Completion { get; }

        /// <summary>
        /// Number of messages waiting to be sent.
        /// </summary>
        public int Count => _channel.Reader.Count;

        /// <summary>
        /// Adds a message to this client's queue. Never blocks.
        /// </summary>
        /// <returns>False if the queue has been disposed.</returns>
        public bool Enqueue(OutboundClientMessage message) => _channel.Writer.TryWrite(message);

        /// <summary>
        /// Stops accepting messages. Messages already queued are still sent, unless the client can no
        /// longer receive them.
        /// </summary>
        public void Dispose() => _channel.Writer.TryComplete();

        private async Task PumpAsync()
        {
            var reader = _channel.Reader;

            while (await reader.WaitToReadAsync().ConfigureAwait(false))
            {
                while (reader.TryRead(out var message))
                {
                    if (!_canSend())
                    {
                        _onDropped?.Invoke(message);
                        continue;
                    }

                    var sendStart = Stopwatch.GetTimestamp();
                    try
                    {
                        _send(message.Payload);
                    }
                    catch (Exception ex)
                    {
                        _onError?.Invoke(ex);
                        continue;
                    }

                    if (_onSent != null)
                    {
                        var sendEnd = Stopwatch.GetTimestamp();
                        _onSent(message, ElapsedMs(message.CreatedTimestamp, sendStart), ElapsedMs(sendStart, sendEnd));
                    }
                }
            }
        }

        private static double ElapsedMs(long from, long to) => (to - from) * 1000.0 / Stopwatch.Frequency;
    }
}
