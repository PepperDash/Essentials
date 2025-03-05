using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core
{
    public class IntFeedback : Feedback
    {
        public override int IntValue { get { return _IntValue; } } // ValueFunc.Invoke(); } }
        int _IntValue;
        public ushort UShortValue { get { return (ushort)_IntValue; } }

        //public override eCueType Type { get { return eCueType.Int; } }

        public int TestValue { get; private set; }

        /// <summary>
        /// Func evaluated on FireUpdate
        /// </summary>
        Func<int> ValueFunc;
        List<UShortInputSig> LinkedInputSigs = new List<UShortInputSig>();

        /// <summary>
        /// Creates the feedback with the Func as described.
        /// </summary>
        /// <remarks>
        /// While the linked sig value will be updated with the current value stored when it is linked to a EISC Bridge,
        /// it will NOT reflect an actual value from a device until <seealso cref="FireUpdate"/> has been called
        /// </remarks>
        /// <param name="valueFunc">Delegate to invoke when this feedback needs to be updated</param>
        public IntFeedback(Func<int> valueFunc)
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
        public IntFeedback(string key, Func<int> valueFunc)
            : base(key)
        {
            ValueFunc = valueFunc;
        }

        public void SetValueFunc(Func<int> newFunc)
        {
            ValueFunc = newFunc;
        }


        public override void FireUpdate()
        {
            var newValue = InTestMode ? TestValue : ValueFunc.Invoke();
            if (newValue != _IntValue)
            {
                _IntValue = newValue;
                LinkedInputSigs.ForEach(s => UpdateSig(s));
                OnOutputChange(newValue);
            }
        }

        public void LinkInputSig(UShortInputSig sig)
        {
            LinkedInputSigs.Add(sig);
            UpdateSig(sig);
        }

        public void UnlinkInputSig(UShortInputSig sig)
        {
            LinkedInputSigs.Remove(sig);
        }

        public override string ToString()
        {
            return (InTestMode ? "TEST -- " : "") + IntValue.ToString();
        }

        /// <summary>
        /// Puts this in test mode, sets the test value and fires an update.
        /// </summary>
        /// <param name="value"></param>
        public void SetTestValue(int value)
        {
            TestValue = value;
            InTestMode = true;
            FireUpdate();
        }

        void UpdateSig(UShortInputSig sig)
        {
            sig.UShortValue = UShortValue;
        }
    }
}