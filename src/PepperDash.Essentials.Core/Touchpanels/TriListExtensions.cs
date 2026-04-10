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
  /// <summary>
  /// SetBoolSigAction method
  /// </summary>
		public static BoolOutputSig SetBoolSigAction(this BasicTriList tl, uint sigNum, Action<bool> a)
		{
			return tl.BooleanOutput[sigNum].SetBoolSigAction(a);
		}

		/// <summary>
		/// Attaches a void Action to a TriList's output sig's UserObject, to be run on press
		/// </summary>
		/// <param name="tl">trilist</param>
		/// <param name="sigNum">number of the signal</param>
		/// <param name="a">action to run when the signal is true (pressed)</param>
		/// <returns></returns>
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
		/// <returns></returns>
		public static BoolOutputSig SetSigHeldAction(this BoolOutputSig sig, uint heldMs, Action heldAction, Action holdReleasedAction, Action releaseAction)
		{
			CTimer heldTimer = null;
			bool wasHeld = false;
			return sig.SetBoolSigAction(press =>
			{
				if (press)
				{
					wasHeld = false;
					// Could insert a pressed action here
					heldTimer = new CTimer(o =>
					{
						// if still held and there's an action
						if (sig.BoolValue && heldAction != null)
						{
							wasHeld = true;
							// Hold action here
							heldAction();
						}
					}, heldMs);
				}
				else if (!press && !wasHeld) // released, no hold
				{
					heldTimer.Stop();
					if (releaseAction != null)
						releaseAction();
				}
				else // !press && wasHeld // released after held
				{
					heldTimer.Stop();
					if (holdReleasedAction != null)
						holdReleasedAction();
				}
			});

		}

        /// <summary>
        /// Sets an action to a held sig as well as a released-without-hold action
        /// </summary>
        /// <returns>The sig</returns>
        /// <summary>
        /// SetSigHeldAction method
        /// </summary>
        public static BoolOutputSig SetSigHeldAction(this BasicTriList tl, uint sigNum, uint heldMs, Action heldAction, Action releaseAction)
        {
			return tl.BooleanOutput[sigNum].SetSigHeldAction(heldMs, heldAction, null, releaseAction);
        }

		/// <summary>
		/// Sets an action to a held sig, an action for the release of hold, as well as a released-without-hold action
		/// </summary>
		/// <returns></returns>
		public static BoolOutputSig SetSigHeldAction(this BasicTriList tl, uint sigNum, uint heldMs, Action heldAction,
			Action holdReleasedAction, Action releaseAction)
		{
			return tl.BooleanOutput[sigNum].SetSigHeldAction(heldMs, heldAction, holdReleasedAction, releaseAction);
		}

		/// <summary>
  		/// SetUShortSigAction method
  		/// </summary>
		/// <param name="sig"></param>
		/// <param name="a"></param>
		/// <returns>The Sig</returns>
 		public static UShortOutputSig SetUShortSigAction(this UShortOutputSig sig, Action<ushort> a)
		{
			sig.UserObject = a;
			return sig;
		}

		/// <summary>
		/// SetUShortSigAction method
		/// </summary>
		/// <param name="tl"></param>
		/// <param name="sigNum"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		public static UShortOutputSig SetUShortSigAction(this BasicTriList tl, uint sigNum, Action<ushort> a)
		{
			return tl.UShortOutput[sigNum].SetUShortSigAction(a);
		}

		/// <summary>
		/// SetStringSigAction method
		/// </summary>
		/// <param name="sig"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		public static StringOutputSig SetStringSigAction(this StringOutputSig sig, Action<string> a)
		{
			sig.UserObject = a;
			return sig;
		}

		/// <summary>
		/// SetStringSigAction method
		/// </summary>
		/// <param name="tl"></param>
		/// <param name="sigNum"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		public static StringOutputSig SetStringSigAction(this BasicTriList tl, uint sigNum, Action<string> a)
		{
			return tl.StringOutput[sigNum].SetStringSigAction(a);
		}

		/// <summary>
		/// ClearSigAction method
		/// </summary>
		/// <param name="sig"></param>
		/// <returns></returns>
		public static Sig ClearSigAction(this Sig sig)
		{
			sig.UserObject = null;
			return sig;
		}

		/// <summary>
		/// ClearBoolSigAction method
		/// </summary>
		/// <param name="tl">trilist</param>
		/// <param name="sigNum">signal number to clear</param>
		/// <returns></returns>
		public static BoolOutputSig ClearBoolSigAction(this BasicTriList tl, uint sigNum)
		{
			return ClearSigAction(tl.BooleanOutput[sigNum]) as BoolOutputSig;
		}

		/// <summary>
		/// ClearUShortSigAction method
		/// </summary>
		public static UShortOutputSig ClearUShortSigAction(this BasicTriList tl, uint sigNum)
		{
			return ClearSigAction(tl.UShortOutput[sigNum]) as UShortOutputSig;
		}

		/// <summary>
		/// ClearStringSigAction method
		/// </summary>
		public static StringOutputSig ClearStringSigAction(this BasicTriList tl, uint sigNum)
		{
			return ClearSigAction(tl.StringOutput[sigNum]) as StringOutputSig;
		}

        /// <summary>
        /// ClearAllSigActions method
        /// </summary>
        public static void ClearAllSigActions(this BasicTriList t1)
        {
            foreach (var sig in t1.BooleanOutput)
            {
                ClearSigAction(sig);
            }

            foreach (var sig in t1.UShortOutput)
            {
                ClearSigAction(sig);
            }

            foreach (var sig in t1.StringOutput)
            {
                ClearSigAction(sig);
            }
        }

        /// <summary>
        /// SetBool method
        /// </summary>
        public static void SetBool(this BasicTriList tl, uint sigNum, bool value)
        {
            tl.BooleanInput[sigNum].BoolValue = value;
        }

		/// <summary>
		/// Sends an true-false pulse to the sig
		/// </summary>
		/// <param name="tl"></param>
		/// <param name="sigNum"></param>
		public static void PulseBool(this BasicTriList tl, uint sigNum)
		{
			tl.BooleanInput[sigNum].Pulse();
		}

		/// <summary>
		/// Sends a timed pulse to the sig
		/// </summary>
		/// <param name="tl"></param>
		/// <param name="sigNum"></param>
		/// <param name="ms"></param>
		public static void PulseBool(this BasicTriList tl, uint sigNum, int ms)
		{
			tl.BooleanInput[sigNum].Pulse(ms);
		}

        /// <summary>
        /// Helper method to set the value of a ushort Sig on TriList
        /// </summary>
        public static void SetUshort(this BasicTriList tl, uint sigNum, ushort value)
        {
            tl.UShortInput[sigNum].UShortValue = value;
        }

        /// <summary>
        /// Helper method to set the value of a string Sig on TriList
        /// </summary>
        public static void SetString(this BasicTriList tl, uint sigNum, string value)
        {
            tl.StringInput[sigNum].StringValue = value;
        }

		/// <summary>
		/// Helper method to set the value of a string Sig on TriList with encoding
		/// </summary>
		/// <param name="tl">trilist</param>
		/// <param name="sigNum">signal number to set</param>
		/// <param name="value">string value to set</param>
		/// <param name="encoding">string encoding to use</param>
	    public static void SetString(this BasicTriList tl, uint sigNum, string value, eStringEncoding encoding)
	    {
	        tl.StringInput[sigNum].StringEncoding = encoding;
	        tl.StringInput[sigNum].StringValue = value;
	    }

		/// <summary>
		/// Returns bool value of trilist sig
		/// </summary>
		/// <param name="tl"></param>
		/// <param name="sigNum"></param>
		/// <returns></returns>
		public static bool GetBool(this BasicTriList tl, uint sigNum)
		{
			return tl.BooleanOutput[sigNum].BoolValue;
		}

		/// <summary>
		/// Returns ushort value of trilist sig
		/// </summary>
		/// <param name="tl"></param>
		/// <param name="sigNum"></param>
		/// <returns></returns>
		public static ushort GetUshort(this BasicTriList tl, uint sigNum)
		{
			return tl.UShortOutput[sigNum].UShortValue;
		}

		/// <summary>
		/// Returns string value of trilist sig.
		/// </summary>
		/// <param name="tl"></param>
		/// <param name="sigNum"></param>
		/// <returns></returns>
		public static string GetString(this BasicTriList tl, uint sigNum)
		{
			return tl.StringOutput[sigNum].StringValue;
		}
    }
}