using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// To be used for serial data feedback where the event chain / asynchronicity must be maintained
    /// and calculating the value based on a Func when it is needed will not suffice.
    /// </summary>
    public class SerialFeedback : Feedback
    {
        /// <summary>
        /// Gets the SerialValue
        /// </summary>
        public override string SerialValue { get { return _SerialValue; } }
        string _SerialValue;

        //public override eCueType Type { get { return eCueType.Serial; } }

        /// <summary>
        /// Gets or sets the TestValue
        /// </summary>
        public string TestValue { get; private set; }

        List<StringInputSig> LinkedInputSigs = new List<StringInputSig>();

        /// <summary>
        /// Constructor
        /// </summary>
        [Obsolete("use constructor with Key parameter. This constructor will be removed in a future version")]
        public SerialFeedback()
        {
        }

        /// <summary>
        /// Constructor with Key parameter
        /// </summary>
        /// <param name="key">Key to find this Feedback</param>
        public SerialFeedback(string key)
            : base(key)
        {
        }

        /// <summary>
        /// FireUpdate method
        /// </summary>
        /// <inheritdoc />
        public override void FireUpdate()
        {
            throw new NotImplementedException("This feedback type does not use Funcs");
        }

        /// <summary>
        /// FireUpdate method
        /// </summary>
        public void FireUpdate(string newValue)
        {
            _SerialValue = newValue;
            LinkedInputSigs.ForEach(s => UpdateSig(s, newValue));
            OnOutputChange(newValue);
        }

        /// <summary>
        /// LinkInputSig method
        /// </summary>
        public void LinkInputSig(StringInputSig sig)
        {
            LinkedInputSigs.Add(sig);
            UpdateSig(sig);
        }

        /// <summary>
        /// UnlinkInputSig method
        /// </summary>
        public void UnlinkInputSig(StringInputSig sig)
        {
            LinkedInputSigs.Remove(sig);
        }

        /// <summary>
        /// ToString method
        /// </summary>
        public override string ToString()
        {
            return (InTestMode ? "TEST -- " : "") + SerialValue;
        }

        /// <summary>
        /// Puts this in test mode, sets the test value and fires an update.
        /// </summary>
        /// <param name="value"></param>
        /// <summary>
        /// SetTestValue method
        /// </summary>
        public void SetTestValue(string value)
        {
            TestValue = value;
            InTestMode = true;
            FireUpdate(TestValue);
        }

        void UpdateSig(StringInputSig sig)
        {
            sig.StringValue = _SerialValue;
        }

        void UpdateSig(StringInputSig sig, string value)
        {
            sig.StringValue = value;
        }
    }
}