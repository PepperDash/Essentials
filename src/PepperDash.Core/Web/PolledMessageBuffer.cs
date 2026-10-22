using System;
using System.Collections.Generic;
using System.Threading;

namespace PepperDash.Core.Web;

/// <summary>
/// Holds the most recent messages of a stream so clients can fetch them over plain HTTP instead of a
/// WebSocket. Used where the WebSocket port can't be reached, such as a Virtual Control server whose
/// firewall only opens 443.
/// </summary>
/// <remarks>
/// Every message gets an increasing ID. A client passes the last ID it received as the cursor and gets
/// everything after it. The buffer only stores messages while a client has polled within
/// <see cref="IdleTimeout"/>, so a stream nobody is reading costs nothing. Crestron's web server sends a
/// response only once it has ended, so <see cref="Poll"/> can hold a request open until a message
/// arrives (long polling) but cannot stream one.
/// </remarks>
public class PolledMessageBuffer
{
    /// <summary>
    /// The longest a single poll may wait for new messages. Kept well under typical proxy timeouts.
    /// </summary>
    public static readonly TimeSpan MaxWait = TimeSpan.FromSeconds(10);

    private readonly object _lock = new object();
    private readonly Queue<KeyValuePair<long, string>> _messages = new Queue<KeyValuePair<long, string>>();
    private readonly Func<DateTime> _utcNow;
    private long _lastId;
    private DateTime _lastPollUtc = DateTime.MinValue;

    /// <summary>
    /// Creates a buffer.
    /// </summary>
    /// <param name="capacity">How many messages to keep. Older ones are dropped and reported as missed.</param>
    /// <param name="idleTimeout">How long after the last poll the buffer keeps collecting messages.</param>
    /// <param name="utcNow">Clock override for tests.</param>
    public PolledMessageBuffer(int capacity, TimeSpan idleTimeout, Func<DateTime> utcNow = null)
    {
        if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));

        Capacity = capacity;
        IdleTimeout = idleTimeout;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <summary>
    /// The number of messages kept.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// How long after the last poll the buffer keeps collecting messages.
    /// </summary>
    public TimeSpan IdleTimeout { get; }

    /// <summary>
    /// True while a client has polled within <see cref="IdleTimeout"/>.
    /// </summary>
    public bool IsActive
    {
        get
        {
            lock (_lock)
            {
                return _utcNow() - _lastPollUtc < IdleTimeout;
            }
        }
    }

    /// <summary>
    /// The ID of the newest message, or 0 when none has been added.
    /// </summary>
    public long LatestId
    {
        get
        {
            lock (_lock)
            {
                return _lastId;
            }
        }
    }

    /// <summary>
    /// Marks the buffer active, as a poll does, and returns the current cursor. Clients call this to
    /// start reading from now on, rather than from whatever is already buffered.
    /// </summary>
    /// <returns>The ID to pass as the first <c>since</c>.</returns>
    public long Activate()
    {
        lock (_lock)
        {
            _lastPollUtc = _utcNow();
            return _lastId;
        }
    }

    /// <summary>
    /// Marks the buffer inactive and drops what it holds. IDs keep increasing, so a client that polls
    /// again later sees the gap as missed messages.
    /// </summary>
    public void Deactivate()
    {
        lock (_lock)
        {
            _lastPollUtc = DateTime.MinValue;
            _messages.Clear();
            Monitor.PulseAll(_lock);
        }
    }

    /// <summary>
    /// Adds a message if a client is polling. Wakes any poll waiting for it.
    /// </summary>
    /// <param name="message">The message text, sent to the client unchanged.</param>
    /// <returns>True if the message was stored.</returns>
    public bool Add(string message)
    {
        if (message == null) return false;

        lock (_lock)
        {
            if (_utcNow() - _lastPollUtc >= IdleTimeout) return false;

            _messages.Enqueue(new KeyValuePair<long, string>(++_lastId, message));

            while (_messages.Count > Capacity)
            {
                _messages.Dequeue();
            }

            Monitor.PulseAll(_lock);
            return true;
        }
    }

    /// <summary>
    /// Returns the messages after <paramref name="since"/>, waiting up to <paramref name="wait"/> for one
    /// to arrive when there are none yet.
    /// </summary>
    /// <param name="since">The last ID the client received.</param>
    /// <param name="wait">How long to hold the request when nothing is new. Clamped to <see cref="MaxWait"/>.</param>
    /// <param name="maxMessages">The most messages to return at once; the client polls again for the rest.</param>
    public PollResult Poll(long since, TimeSpan wait, int maxMessages = 500)
    {
        if (wait < TimeSpan.Zero) wait = TimeSpan.Zero;
        if (wait > MaxWait) wait = MaxWait;
        if (maxMessages < 1) maxMessages = 1;

        var deadline = DateTime.UtcNow + wait;

        lock (_lock)
        {
            // Messages that arrived while the buffer was idle were never stored
            var wasIdle = _utcNow() - _lastPollUtc >= IdleTimeout;

            _lastPollUtc = _utcNow();

            // A cursor ahead of the buffer means the program restarted since the client last polled
            if (wasIdle || since > _lastId)
            {
                return new PollResult(new List<string>(), _lastId, true);
            }

            while (_lastId <= since)
            {
                var remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero) break;

                Monitor.Wait(_lock, remaining);

                // Deactivate() pulses too; stop waiting rather than hold a request for a closed session
                if (_utcNow() - _lastPollUtc >= IdleTimeout) break;
            }

            var messages = new List<string>();
            var next = since;
            var oldestId = _messages.Count > 0 ? _messages.Peek().Key : _lastId + 1;
            var missed = since < _lastId && since + 1 < oldestId;

            foreach (var entry in _messages)
            {
                if (entry.Key <= since) continue;
                if (messages.Count >= maxMessages) break;

                messages.Add(entry.Value);
                next = entry.Key;
            }

            // Nothing left to deliver from the gap; move the cursor past it
            if (messages.Count == 0 && missed) next = _lastId;

            return new PollResult(messages, next, missed);
        }
    }

    /// <summary>
    /// The result of a <see cref="Poll"/>.
    /// </summary>
    public class PollResult
    {
        internal PollResult(IReadOnlyList<string> messages, long next, bool missed)
        {
            Messages = messages;
            Next = next;
            Missed = missed;
        }

        /// <summary>
        /// The messages, oldest first.
        /// </summary>
        public IReadOnlyList<string> Messages { get; }

        /// <summary>
        /// The cursor to pass as <c>since</c> on the next poll.
        /// </summary>
        public long Next { get; }

        /// <summary>
        /// True when messages after the client's cursor were dropped before it fetched them, the buffer
        /// went idle since the client's last poll, or the program restarted. A client that needs complete state should fetch it again.
        /// </summary>
        public bool Missed { get; }
    }
}
