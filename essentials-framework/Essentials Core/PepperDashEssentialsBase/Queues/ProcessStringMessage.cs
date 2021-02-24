using System;

namespace PepperDash.Essentials.Core.Queues
{
    /// <summary>
    /// Message class for processing strings via an IQueue
    /// </summary>
    public class ProcessStringMessage : IQueueMessage
    {
        private readonly Action<string> _action;
        private readonly string _message;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message to be processed</param>
        /// <param name="action">Action to invoke on the message</param>
        public ProcessStringMessage(string message, Action<string> action)
        {
            _message = message;
            _action = action;
        }

        /// <summary>
        /// Processes the string with the given action
        /// </summary>
        public void Dispatch()
        {
            if (_action == null || String.IsNullOrEmpty(_message))
                return;

            _action(_message);
        }

        /// <summary>
        /// To string
        /// </summary>
        /// <returns>The current message</returns>
        public override string ToString()
        {
            return _message ?? String.Empty;
        }
    }
}

namespace PepperDash_Essentials_Core.Queues
{
    /// <summary>
    /// Message class for processing strings via an IQueue
    /// </summary>
    [Obsolete("Use PepperDash.Essentials.Core.Queues")]
    public class ProcessStringMessage : PepperDash.Essentials.Core.Queues.ProcessStringMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message to be processed</param>
        /// <param name="action">Action to invoke on the message</param>
        public ProcessStringMessage(string message, Action<string> action) : base(message, action){}
    }
}