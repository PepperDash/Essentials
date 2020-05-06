using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core
{

    public class StringFeedback : Feedback
    {
        public override string StringValue { get { return _StringValue; } } // ValueFunc.Invoke(); } }
        string _StringValue;

        /// <summary>
        /// Used in testing.  Set/Clear functions
        /// </summary>
        public string TestValue { get; private set; }

        /// <summary>
        /// Evaluated on FireUpdate
        /// </summary>
        public Func<string> ValueFunc { get; private set; }
        List<StringInputSig> LinkedInputSigs = new List<StringInputSig>();

        /// <summary>
        /// Creates the feedback with the Func as described.
        /// </summary>
        /// <remarks>
        /// While the linked sig value will be updated with the current value stored when it is linked to a EISC Bridge,
        /// it will NOT reflect an actual value from a device until <seealso cref="FireUpdate"/> has been called
        /// </remarks>
        /// <param name="valueFunc">Delegate to invoke when this feedback needs to be updated</param>
        public StringFeedback(Func<string> valueFunc)
            : this(null, valueFunc)
        {
        }

        /// <summary>
        /// Creates the feedback with the Func as described.
        /// </summary>
        /// <remarks>
        /// While the linked sig value will be updated with the current value stored when it is linked to a EISC Bridge,
        /// it will NOT reflect an actual value from a device until <seealso cref="FireUpdate"/> has been called
        /// </remarks>
        /// <param name="key">Key to find this Feedback</param>
        /// <param name="valueFunc">Delegate to invoke when this feedback needs to be updated</param>
        public StringFeedback(string key, Func<string> valueFunc)
            : base(key)
        {
            ValueFunc = valueFunc;
        }



        public override void FireUpdate()
        {
            var newValue = InTestMode ? TestValue : ValueFunc.Invoke();
            if (newValue != _StringValue)
            {
                _StringValue = newValue;
                LinkedInputSigs.ForEach(s => UpdateSig(s));
                OnOutputChange(newValue);
            }
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
            return (InTestMode ? "TEST -- " : "") + StringValue;
        }

        /// <summary>
        /// Puts this in test mode, sets the test value and fires an update.
        /// </summary>
        /// <param name="value"></param>
        public void SetTestValue(string value)
        {
            TestValue = value;
            InTestMode = true;
            FireUpdate();
        }

        void UpdateSig(StringInputSig sig)
        {
            sig.StringValue = _StringValue;
        }
    }
}