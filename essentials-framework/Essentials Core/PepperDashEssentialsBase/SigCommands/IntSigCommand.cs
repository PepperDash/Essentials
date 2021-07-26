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
    public class IntSigCommand : SigCommandBase
    {
        /// <summary>
        /// Func that evaluates on FireUpdate and updates the linked sigs
        /// </summary>
        public Func<int> ValueFunc { get; private set; }

        List<UShortInputSig> LinkedInputSigs = new List<UShortInputSig>();

        public IntSigCommand(Func<int> valueFunc)
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
        public IntSigCommand(string key, Func<int> valueFunc)
            : base(key)
        {
            ValueFunc = valueFunc;
        }

        public override void FireUpdate()
        {
            var value = InvokeValueFunc();

            LinkedInputSigs.ForEach(s => UpdateSig(value, s));
        }

        ushort InvokeValueFunc()
        {
            return (ushort)ValueFunc.Invoke();
        }

        /// <summary>
        /// Links an input sig
        /// </summary>
        /// <param name="sig"></param>
        public void LinkInputSig(UShortInputSig sig)
        {
            LinkedInputSigs.Add(sig);
            UpdateSig(InvokeValueFunc(), sig);
        }

        /// <summary>
        /// Unlinks an inputs sig
        /// </summary>
        /// <param name="sig"></param>
        public void UnlinkInputSig(UShortInputSig sig)
        {
            LinkedInputSigs.Remove(sig);
        }

        void UpdateSig(ushort value, UShortInputSig sig)
        {
            sig.UShortValue = value;
        }

    }
}