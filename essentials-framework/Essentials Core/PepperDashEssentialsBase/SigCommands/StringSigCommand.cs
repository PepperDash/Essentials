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
    public class StringSigCommand : SigCommandBase
    {
        /// <summary>
        /// Func that evaluates on FireUpdate and updates the linked sigs
        /// </summary>
        public Func<string> ValueFunc { get; private set; }

        List<StringInputSig> LinkedInputSigs = new List<StringInputSig>();

        public StringSigCommand(Func<string> valueFunc)
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
        public StringSigCommand(string key, Func<string> valueFunc)
            : base(key)
        {
            ValueFunc = valueFunc;
        }

        public override void FireUpdate()
        {
            var value = InvokeValueFunc();

            LinkedInputSigs.ForEach(s => UpdateSig(value, s));
        }

        string InvokeValueFunc()
        {
            return ValueFunc.Invoke();
        }

        /// <summary>
        /// Links an input sig
        /// </summary>
        /// <param name="sig"></param>
        public void LinkInputSig(StringInputSig sig)
        {
            LinkedInputSigs.Add(sig);
            UpdateSig(InvokeValueFunc(), sig);
        }

        /// <summary>
        /// Unlinks an inputs sig
        /// </summary>
        /// <param name="sig"></param>
        public void UnlinkInputSig(StringInputSig sig)
        {
            LinkedInputSigs.Remove(sig);
        }

        void UpdateSig(string value, StringInputSig sig)
        {
            sig.StringValue = value;
        }

    }
}