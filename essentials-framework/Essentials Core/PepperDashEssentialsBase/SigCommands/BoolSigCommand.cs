using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// A helper class to make it easier to work with Crestron Sigs
    /// </summary>
    public class BoolSigCommand : SigCommandBase
    {
        /// <summary>
        /// Func that evaluates on FireUpdate and updates the linked sigs
        /// </summary>
        public Func<bool> ValueFunc { get; private set; }

        List<BoolInputSig> LinkedInputSigs = new List<BoolInputSig>();
        List<BoolInputSig> LinkedComplementInputSigs = new List<BoolInputSig>();

        public BoolSigCommand(Func<bool> valueFunc)
            : this(null, valueFunc)
        {
        }

        /// <summary>
        /// Creates the SigCommand with the Func as described.
        /// </summary>
        /// <remarks>
        /// When linking to a sig, the sig value func will be run and that sigs value updated.
        /// To update all the linked sigs, <seealso cref="FireUpdate"/> must been called
        /// </remarks>
        /// <param name="valueFunc">Delegate to invoke when this SigCommand gets fired</param>
        public BoolSigCommand(string key, Func<bool> valueFunc)
            : base(key)
        {
            ValueFunc = valueFunc;
        }

        public override void FireUpdate()
        {
            var value = InvokeValueFunc();

            LinkedInputSigs.ForEach(s => UpdateSig(value, s));
            LinkedComplementInputSigs.ForEach(s => UpdateComplementSig(value, s));
        }

        bool InvokeValueFunc()
        {
            return ValueFunc.Invoke();
        }

        /// <summary>
        /// Links an input sig
        /// </summary>
        /// <param name="sig"></param>
        public void LinkInputSig(BoolInputSig sig)
        {
            LinkedInputSigs.Add(sig);
            UpdateSig(InvokeValueFunc(), sig);
        }

        /// <summary>
        /// Unlinks an inputs sig
        /// </summary>
        /// <param name="sig"></param>
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
            UpdateComplementSig(InvokeValueFunc(), sig);
        }

        /// <summary>
        /// Unlinks an input sig to the complement value
        /// </summary>
        /// <param name="sig"></param>
        public void UnlinkComplementInputSig(BoolInputSig sig)
        {
            LinkedComplementInputSigs.Remove(sig);
        }

        void UpdateSig(bool value, BoolInputSig sig)
        {
            sig.BoolValue = value;
        }

        void UpdateComplementSig(bool value, BoolInputSig sig)
        {
            sig.BoolValue = !value;
        }
    }
}