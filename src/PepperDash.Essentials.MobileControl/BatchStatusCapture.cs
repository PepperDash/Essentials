using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Collects one client's replies to an aggregated batch status request, so they can be sent as a
    /// single message instead of one message per messenger.
    /// </summary>
    /// <remarks>
    /// The capture is ambient for the code a batch handler runs (an <see cref="AsyncLocal{T}"/>), so the
    /// messengers' existing <c>/fullStatus</c> handlers need no changes: their replies still go through
    /// <c>SendMessageObject</c>, which hands them to <see cref="TryCapture"/> first. Only messages
    /// addressed to the requesting client are captured, and only until <see cref="Close"/> - anything a
    /// handler posts later (e.g. from its own background task) is sent normally rather than lost.
    /// </remarks>
    public sealed class BatchStatusCapture
    {
        private static readonly AsyncLocal<BatchStatusCapture> ambient = new AsyncLocal<BatchStatusCapture>();

        private readonly object _lock = new object();
        private readonly List<CapturedStatusMessage> _messages = new List<CapturedStatusMessage>();
        private bool _closed;

        /// <summary>
        /// Creates a capture for replies addressed to <paramref name="clientId"/>.
        /// </summary>
        public BatchStatusCapture(string clientId)
        {
            ClientId = clientId;
        }

        /// <summary>
        /// The client whose replies are captured.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Runs <paramref name="action"/> with this capture active for it and everything it starts.
        /// </summary>
        public void Run(Action action)
        {
            var previous = ambient.Value;
            ambient.Value = this;
            try
            {
                action();
            }
            finally
            {
                ambient.Value = previous;
            }
        }

        /// <summary>
        /// Captures <paramref name="message"/> if a capture is active for the current code, the message
        /// is addressed to that capture's client, and the capture is still open.
        /// </summary>
        /// <returns>True if captured, in which case the caller must not send the message itself.</returns>
        public static bool TryCapture(IMobileControlMessage message) => TryCapture(message, out _);

        /// <summary>
        /// Captures <paramref name="message"/> if a capture is active for the current code, the message
        /// is addressed to that capture's client, and the capture is still open.
        /// </summary>
        /// <param name="message">The message to capture.</param>
        /// <param name="missedBatch">
        /// True when the message belongs to a batch whose capture has already closed: a handler replied
        /// from work that outlived it, so the reply goes out after the aggregated message.
        /// </param>
        /// <returns>True if captured, in which case the caller must not send the message itself.</returns>
        public static bool TryCapture(IMobileControlMessage message, out bool missedBatch)
        {
            missedBatch = false;
            var capture = ambient.Value;
            if (capture == null || message == null || message.ClientId != capture.ClientId)
            {
                return false;
            }

            lock (capture._lock)
            {
                if (capture._closed)
                {
                    missedBatch = true;
                    return false;
                }

                capture._messages.Add(new CapturedStatusMessage(message.Type, message.Content));
                return true;
            }
        }

        /// <summary>
        /// Stops capturing and returns everything captured, in the order it was captured.
        /// </summary>
        public IReadOnlyList<CapturedStatusMessage> Close()
        {
            lock (_lock)
            {
                _closed = true;
                return _messages.ToList();
            }
        }
    }

    /// <summary>
    /// A reply captured by <see cref="BatchStatusCapture"/>.
    /// </summary>
    public sealed class CapturedStatusMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CapturedStatusMessage"/> class.
        /// </summary>
        public CapturedStatusMessage(string type, JToken content)
        {
            Type = type;
            Content = content;
        }

        /// <summary>
        /// The message type (path), e.g. <c>/device/display-1</c>.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// The message content.
        /// </summary>
        public JToken Content { get; }
    }
}
