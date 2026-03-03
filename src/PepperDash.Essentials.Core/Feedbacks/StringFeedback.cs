using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core
{

    /// <summary>
    /// Represents a StringFeedback
    /// </summary>
    public class StringFeedback : Feedback
    {
        /// <summary>
        /// Gets or sets the StringValue
        /// </summary>
        public override string StringValue { get { return _StringValue; } } // ValueFunc.Invoke(); } }
        string _StringValue;

        /// <summary>
        /// Gets or sets the TestValue
        /// </summary>
        public string TestValue { get; private set; }

        /// <summary>
        /// Gets or sets the ValueFunc
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
        [Obsolete("use constructor with Key parameter. This constructor will be removed in a future version")]
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

        /// <summary>
        /// Sets the ValueFunc
        /// </summary>
        /// <param name="newFunc">function to set</param>
        public void SetValueFunc(Func<string> newFunc)
        {
            ValueFunc = newFunc;
        }

        /// <summary>
        /// FireUpdate method
        /// </summary>
        /// <inheritdoc />
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
            return (InTestMode ? "TEST -- " : "") + StringValue;
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
            FireUpdate();
        }

        void UpdateSig(StringInputSig sig)
        {
            sig.StringValue = _StringValue;
        }
    }
}