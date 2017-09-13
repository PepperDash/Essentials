using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Extensions used for more-clear attachment of Actions to user objects on sigs
	/// </summary>
	public static class SigAndTriListExtensions
	{
		/// <summary>
		/// Attaches Action to Sig's user object and returns the same Sig. This provides no protection
        /// from null sigs
		/// </summary>
		/// <param name="sig">The BoolOutputSig to attach the Action to</param>
		/// <param name="a">An action to run when sig is pressed and when released</param>
		/// <returns>The Sig, sig</returns>
		public static BoolOutputSig SetBoolSigAction(this BoolOutputSig sig, Action<bool> a)
		{
			sig.UserObject = a;
			return sig;
		}

		/// <summary>
		/// Attaches Action to Sig's user object and returns the same Sig.
		/// </summary>
		/// <param name="tl"></param>
		/// <param name="sigNum"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		public static BoolOutputSig SetBoolSigAction(this BasicTriList tl, uint sigNum, Action<bool> a)
		{
			return tl.BooleanOutput[sigNum].SetBoolSigAction(a);
		}

		public static BoolOutputSig SetSigTrueAction(this BasicTriList tl, uint sigNum, Action a)
		{
			return tl.BooleanOutput[sigNum].SetBoolSigAction(b => { if(b) a(); });
		}

		/// <summary>
		/// Attaches a void Action to a TriList's output sig's UserObject, to be run on release
		/// </summary>
		/// <returns>The sig</returns>
		public static BoolOutputSig SetSigFalseAction(this BasicTriList tl, uint sigNum, Action a)
		{
			return tl.BooleanOutput[sigNum].SetBoolSigAction(b => { if (!b) a(); });
		}

		/// <summary>
		/// Attaches a void Action to an output sig's UserObject, to be run on release
		/// </summary>
		/// <returns>The Sig</returns>
		public static BoolOutputSig SetSigFalseAction(this BoolOutputSig sig, Action a)
		{
			return sig.SetBoolSigAction(b => { if (!b) a(); });
		}

        /// <summary>
        /// Sets an action to a held sig
        /// </summary>
        /// <returns>The sig</returns>
        public static BoolOutputSig SetSigHeldAction(this BasicTriList tl, uint sigNum, uint heldMs, Action heldAction)
        {
            return SetSigHeldAction(tl, sigNum, heldMs, heldAction, null);
        }


        /// <summary>
        /// Sets an action to a held sig as well as a released-without-hold action
        /// </summary>
        /// <returns>The sig</returns>
        public static BoolOutputSig SetSigHeldAction(this BasicTriList tl, uint sigNum, uint heldMs, Action heldAction, Action releaseAction)
        {
            CTimer heldTimer = null;
            return tl.SetBoolSigAction(sigNum, press =>
            {
                if (press)
                {

                    // Could insert a pressed action here
                    heldTimer = new CTimer(o =>
                    {
                        // if still held and there's an action
                        if (tl.BooleanOutput[sigNum].BoolValue && heldAction != null)
                            // Hold action here
                            heldAction();
                    }, heldMs);
                }
                else if (heldTimer != null) // released
                {
                    heldTimer.Stop();
                    if (releaseAction != null)
                        releaseAction();
                }
            });
        }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sig"></param>
		/// <param name="a"></param>
		/// <returns>The Sig</returns>
		public static UShortOutputSig SetUShortSigAction(this UShortOutputSig sig, Action<ushort> a)
		{
			sig.UserObject = a;
			return sig;
		}

		public static UShortOutputSig SetUShortSigAction(this BasicTriList tl, uint sigNum, Action<ushort> a)
		{
			return tl.UShortOutput[sigNum].SetUShortSigAction(a);
		}

		public static StringOutputSig SetStringSigAction(this StringOutputSig sig, Action<string> a)
		{
			sig.UserObject = a;
			return sig;
		}

		public static StringOutputSig SetStringSigAction(this BasicTriList tl, uint sigNum, Action<string> a)
		{
			return tl.SetStringSigAction(sigNum, a);
		}

		public static Sig ClearSigAction(this Sig sig)
		{
			sig.UserObject = null;
			return sig;
		}

		public static BoolOutputSig ClearBoolSigAction(this BasicTriList tl, uint sigNum)
		{
			return ClearSigAction(tl.BooleanOutput[sigNum]) as BoolOutputSig;
		}

		public static UShortOutputSig ClearUShortSigAction(this BasicTriList tl, uint sigNum)
		{
			return ClearSigAction(tl.UShortOutput[sigNum]) as UShortOutputSig;
		}

		public static StringOutputSig ClearStringSigAction(this BasicTriList tl, uint sigNum)
		{
			return ClearSigAction(tl.StringOutput[sigNum]) as StringOutputSig;
		}

        /// <summary>
        /// Helper method to set the value of a bool Sig on TriList
        /// </summary>
        public static void SetBool(this BasicTriList tl, uint sigNum, bool value)
        {
            tl.BooleanInput[sigNum].BoolValue = value;
        }

        /// <summary>
        /// Helper method to set the value of a string Sig on TriList
        /// </summary>
        public static void SetString(this BasicTriList tl, uint sigNum, string value)
        {
            tl.StringInput[sigNum].StringValue = value;
        }
    }
}