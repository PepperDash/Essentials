using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
	public abstract class Feedback : IKeyed
	{
		public event EventHandler<FeedbackEventArgs> OutputChange;

        /// <summary>
        /// Gets or sets the Key
        /// </summary>
        public string Key { get; private set; }

  /// <summary>
  /// Gets or sets the BoolValue
  /// </summary>
  /// <inheritdoc />
		public virtual bool BoolValue { get { return false; } }
  /// <summary>
  /// Gets or sets the IntValue
  /// </summary>
		public virtual int IntValue { get { return 0; } }
  /// <summary>
  /// Gets or sets the StringValue
  /// </summary>
		public virtual string StringValue { get { return ""; } }
        /// <summary>
        /// Gets or sets the SerialValue
        /// </summary>
        public virtual string SerialValue { get { return ""; } }

  /// <summary>
  /// Gets or sets the InTestMode
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
  /// ClearTestValue method
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
}