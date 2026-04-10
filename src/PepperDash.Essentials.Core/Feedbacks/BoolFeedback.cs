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
        /// Gets or sets the ValueFunc
        /// </summary>
        public Func<bool> ValueFunc { get; private set; }

        List<BoolInputSig> LinkedInputSigs = new List<BoolInputSig>();
        List<BoolInputSig> LinkedComplementInputSigs = new List<BoolInputSig>();

        List<Crestron.SimplSharpPro.DeviceSupport.Feedback> LinkedCrestronFeedbacks = new List<Crestron.SimplSharpPro.DeviceSupport.Feedback>();

        /// <summary>
        /// Creates the feedback with the Func as described.
        /// </summary>
        /// <remarks>
        /// While the linked sig value will be updated with the current value stored when it is linked to a EISC Bridge,
        /// it will NOT reflect an actual value from a device until <seealso cref="FireUpdate"/> has been called
        /// </remarks>
        /// <param name="valueFunc">Delegate to invoke when this feedback needs to be updated</param>
        [Obsolete("use constructor with Key parameter. This constructor will be removed in a future version")]
        public BoolFeedback(Func<bool> valueFunc)
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
        public BoolFeedback(string key, Func<bool> valueFunc)
            : base(key)
        {
            ValueFunc = valueFunc;
        }

        /// <summary>
        /// Sets the ValueFunc
        /// </summary>
        /// <param name="newFunc">New function to set as the ValueFunc</param>
        public void SetValueFunc(Func<bool> newFunc)
        {
            ValueFunc = newFunc;
        }

        /// <summary>
        /// FireUpdate method
        /// </summary>
        /// <inheritdoc />
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

        /// <summary>
        /// Links an input sig
        /// </summary>
        /// <param name="sig"></param>
        /// <summary>
        /// LinkInputSig method
        /// </summary>
        public void LinkInputSig(BoolInputSig sig)
        {
            LinkedInputSigs.Add(sig);
            UpdateSig(sig);
        }

        /// <summary>
        /// Unlinks an inputs sig
        /// </summary>
        /// <param name="sig"></param>
        /// <summary>
        /// UnlinkInputSig method
        /// </summary>
        public void UnlinkInputSig(BoolInputSig sig)
        {
            LinkedInputSigs.Remove(sig);
        }

        /// <summary>
        /// Links an input sig to the complement value
        /// </summary>
        /// <param name="sig"></param>
        public void LinkComplementInputSig(BoolInputSig sig)
        {
            LinkedComplementInputSigs.Add(sig);
            UpdateComplementSig(sig);
        }

        /// <summary>
        /// Unlinks an input sig to the complement value
        /// </summary>
        /// <param name="sig"></param>
        /// <summary>
        /// UnlinkComplementInputSig method
        /// </summary>
        public void UnlinkComplementInputSig(BoolInputSig sig)
        {
            LinkedComplementInputSigs.Remove(sig);
        }

        /// <summary>
        /// Links a Crestron Feedback object
        /// </summary>
        /// <param name="feedback"></param>
        public void LinkCrestronFeedback(Crestron.SimplSharpPro.DeviceSupport.Feedback feedback)
        {
            LinkedCrestronFeedbacks.Add(feedback);
            UpdateCrestronFeedback(feedback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="feedback"></param>
        /// <summary>
        /// UnlinkCrestronFeedback method
        /// </summary>
        public void UnlinkCrestronFeedback(Crestron.SimplSharpPro.DeviceSupport.Feedback feedback)
        {
            LinkedCrestronFeedbacks.Remove(feedback);
        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (InTestMode ? "TEST -- " : "") + BoolValue.ToString();
        }

        /// <summary>
        /// Puts this in test mode, sets the test value and fires an update.
        /// </summary>
        /// <param name="value"></param>
        /// <summary>
        /// SetTestValue method
        /// </summary>
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

        void UpdateCrestronFeedback(Crestron.SimplSharpPro.DeviceSupport.Feedback feedback)
        {
            feedback.State = _BoolValue;
        }
    }

}