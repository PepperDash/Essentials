using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// A Feedback whose output is derived from the return value of a provided Func.
    /// </summary>
    public class BoolFeedback : Feedback
    {
        /// <summary>
        /// Returns the current value of the feedback, derived from the ValueFunc. The ValueFunc is 
        /// evaluated whenever FireUpdate() is called
        /// </summary>
        public override bool BoolValue { get { return _BoolValue; } }
        bool _BoolValue;

        /// <summary>
        /// Fake value to be used in test mode
        /// </summary>
        public bool TestValue { get; private set; }

        /// <summary>
        /// Func that evaluates on FireUpdate
        /// </summary>
        public Func<bool> ValueFunc { get; private set; }

        List<BoolInputSig> LinkedInputSigs = new List<BoolInputSig>();
        List<BoolInputSig> LinkedComplementInputSigs = new List<BoolInputSig>();

        public BoolFeedback(Func<bool> valueFunc)
            : this(null, valueFunc)
        {
        }

        public BoolFeedback(string key, Func<bool> valueFunc)
            : base(key)
        {
            ValueFunc = valueFunc;
        }


        public override void FireUpdate()
        {
            bool newValue = InTestMode ? TestValue : ValueFunc.Invoke();
            if (newValue != _BoolValue)
            {
                _BoolValue = newValue;
                LinkedInputSigs.ForEach(s => UpdateSig(s));
                LinkedComplementInputSigs.ForEach(s => UpdateComplementSig(s));
                OnOutputChange(newValue);
            }
        }

        public void LinkInputSig(BoolInputSig sig)
        {
            LinkedInputSigs.Add(sig);
            UpdateSig(sig);
        }

        public void UnlinkInputSig(BoolInputSig sig)
        {
            LinkedInputSigs.Remove(sig);
        }

        public void LinkComplementInputSig(BoolInputSig sig)
        {
            LinkedComplementInputSigs.Add(sig);
            UpdateComplementSig(sig);
        }

        public void UnlinkComplementInputSig(BoolInputSig sig)
        {
            LinkedComplementInputSigs.Remove(sig);
        }

        public override string ToString()
        {
            return (InTestMode ? "TEST -- " : "") + BoolValue.ToString();
        }

        /// <summary>
        /// Puts this in test mode, sets the test value and fires an update.
        /// </summary>
        /// <param name="value"></param>
        public void SetTestValue(bool value)
        {
            TestValue = value;
            InTestMode = true;
            FireUpdate();
        }

        void UpdateSig(BoolInputSig sig)
        {
            sig.BoolValue = _BoolValue;
        }

        void UpdateComplementSig(BoolInputSig sig)
        {
            sig.BoolValue = !_BoolValue;
        }
    }

}