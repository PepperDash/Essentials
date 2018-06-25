using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core
{
	public abstract class Feedback
	{
		public event EventHandler<FeedbackEventArgs> OutputChange;

		public virtual bool BoolValue { get { return false; } }
		public virtual int IntValue { get { return 0; } }
		public virtual string StringValue { get { return ""; } }

		public Cue Cue { get; private set; }

		public abstract eCueType Type { get; }

		/// <summary>
		/// Feedbacks can be put into test mode for simulation of events without real data. 
		/// Using JSON debugging methods and the Set/ClearTestValue methods, we can simulate
		/// Feedback behaviors
		/// </summary>
		public bool InTestMode { get; protected set; }

		/// <summary>
		/// Base Constructor - empty
		/// </summary>
		protected Feedback()
		{
		}

		protected Feedback(Cue cue)
		{
			Cue = cue;
		}

		/// <summary>
		/// Clears test mode and fires update.
		/// </summary>
		public void ClearTestValue()
		{
			InTestMode = false;
			FireUpdate();
		}

		/// <summary>
		/// Fires an update synchronously
		/// </summary>
		public abstract void FireUpdate();

		/// <summary>
		/// Fires the update asynchronously within a CrestronInvoke
		/// </summary>
		public void InvokeFireUpdate()
		{
			CrestronInvoke.BeginInvoke(o => FireUpdate());
		}

        ///// <summary>
        ///// Helper method that fires event. Use this intstead of calling OutputChange
        ///// </summary>
        //protected void OnOutputChange()
        //{
        //    if (OutputChange != null) OutputChange(this, EventArgs.Empty);
        //}

        protected void OnOutputChange(bool value)
        {
            if (OutputChange != null) OutputChange(this, new FeedbackEventArgs(value));
        }

        protected void OnOutputChange(int value)
        {
            if (OutputChange != null) OutputChange(this, new FeedbackEventArgs(value));
        }


        protected void OnOutputChange(string value)
        {
            if (OutputChange != null) OutputChange(this, new FeedbackEventArgs(value));
        }
	}

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

		public override eCueType Type { get { return eCueType.Bool; } }

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
			: this(Cue.DefaultBoolCue, valueFunc)
		{
		}

		public BoolFeedback(Cue cue, Func<bool> valueFunc)
			: base(cue)
		{
			if (cue == null) throw new ArgumentNullException("cue");
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

	//******************************************************************************
	public class IntFeedback : Feedback
	{
        public override int IntValue { get { return _IntValue; } } // ValueFunc.Invoke(); } }
        int _IntValue;
        public ushort UShortValue { get { return (ushort)_IntValue; } }

		public override eCueType Type { get { return eCueType.Int; } }

		public int TestValue { get; private set; }

		/// <summary>
		/// Func evaluated on FireUpdate
		/// </summary>
		Func<int> ValueFunc;
		List<UShortInputSig> LinkedInputSigs = new List<UShortInputSig>();

		public IntFeedback(Func<int> valueFunc)
			: this(Cue.DefaultIntCue, valueFunc)
		{
		}

		public IntFeedback(Cue cue, Func<int> valueFunc)
			: base(cue)
		{
			if (cue == null) throw new ArgumentNullException("cue");
			ValueFunc = valueFunc;
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


	//******************************************************************************
	public class StringFeedback : Feedback
	{
        public override string StringValue { get { return _StringValue; } } // ValueFunc.Invoke(); } }
        string _StringValue;

		public override eCueType Type { get { return eCueType.String; } }

		/// <summary>
		/// Used in testing.  Set/Clear functions
		/// </summary>
		public string TestValue { get; private set; }

		/// <summary>
		/// Evalutated on FireUpdate
		/// </summary>
		public Func<string> ValueFunc { get; private set; }
		List<StringInputSig> LinkedInputSigs = new List<StringInputSig>();

		public StringFeedback(Func<string> valueFunc)
			: this(Cue.DefaultStringCue, valueFunc)
		{
		}

		public StringFeedback(Cue cue, Func<string> valueFunc)
			: base(cue)
		{
			if (cue == null) throw new ArgumentNullException("cue");
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