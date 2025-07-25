using System;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Queues
{
    /// <summary>
    /// Represents a ComsMessage
    /// </summary>
    public class ComsMessage : IQueueMessage
    {
        private readonly byte[] _bytes;
        private readonly IBasicCommunication _coms;
        private readonly string _string;
        private readonly bool _isByteMessage;

        /// <summary>
        /// Constructor for a string message
        /// </summary>
        /// <param name="coms">IBasicCommunication to send the message</param>
        /// <param name="message">Message to send</param>
        public ComsMessage(IBasicCommunication coms, string message)
        {
            Validate(coms, message);
            _coms = coms;
            _string = message;
        }

        /// <summary>
        /// Constructor for a byte message
        /// </summary>
        /// <param name="coms">IBasicCommunication to send the message</param>
        /// <param name="message">Message to send</param>
        public ComsMessage(IBasicCommunication coms, byte[] message)
        {
            Validate(coms, message);
            _coms = coms;
            _bytes = message;
            _isByteMessage = true;
        }

        private void Validate(IBasicCommunication coms, object message)
        {
            if (coms == null)
                throw new ArgumentNullException("coms");

            if (message == null)
                throw new ArgumentNullException("message");
        }

        /// <summary>
        /// Dispatch method
        /// </summary>
        public void Dispatch()
        {
            if (_isByteMessage)
            {
                _coms.SendBytes(_bytes);
            }
            else
            {
                _coms.SendText(_string);
            }
        }

        /// <summary>
        /// ToString method
        /// </summary>
        /// <inheritdoc />
        public override string ToString()
        {
            return _bytes != null ? _bytes.ToString() : _string;
        }
    }
}