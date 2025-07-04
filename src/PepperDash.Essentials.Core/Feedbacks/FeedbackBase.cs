using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;

namespace PepperDash.Essentials.Core;

	public abstract class Feedback : IKeyed
	{
		public event EventHandler<FeedbackEventArgs> OutputChange;

    public string Key { get; private set; }

		public virtual bool BoolValue { get { return false; } }
		public virtual int IntValue { get { return 0; } }
		public virtual string StringValue { get { return ""; } }
    public virtual string SerialValue { get { return ""; } }

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

    protected Feedback(string key)
    {
        if (key == null)
            Key = "";
        else
            Key = key;
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