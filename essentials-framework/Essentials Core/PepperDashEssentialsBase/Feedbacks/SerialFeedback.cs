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
        public override string SerialValue { get { return _SerialValue; } } 
        string _SerialValue;

        public override eCueType Type { get { return eCueType.Serial; } }

        /// <summary>
        /// Used in testing.  Set/Clear functions
        /// </summary>
        public string TestValue { get; private set; }

        List<StringInputSig> LinkedInputSigs = new List<StringInputSig>();

        public SerialFeedback()
        {
        }

        public SerialFeedback(string key)
            : base(key)
        {
        }

        public override void FireUpdate()
        {
            throw new NotImplementedException("This feedback type does not use Funcs");
        }

        public void FireUpdate(string newValue)
        {
            _SerialValue = newValue;
            LinkedInputSigs.ForEach(s => UpdateSig(s, newValue));
            OnOutputChange(newValue);
        }

        public void LinkInputSig(StringInputSig sig)
        {
            LinkedInputSigs.Add(sig);
            UpdateSig(sig);
        }

        public void UnlinkInputSig(StringInputSig sig)
        {
            LinkedInputSigs.Remove(sig);
        }

        public override string ToString()
        {
            return (InTestMode ? "TEST -- " : "") + SerialValue;
        }

        /// <summary>
        /// Puts this in test mode, sets the test value and fires an update.
        /// </summary>
        /// <param name="value"></param>
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