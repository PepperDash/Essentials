using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// A helper class to make it easier to work with Crestron Sigs
    /// </summary>
    public abstract class SigCommandBase : IKeyed
    {

        public string Key { get; private set; }  

        /// <summary>
		/// Base Constructor - empty
		/// </summary>
		protected SigCommandBase()
		{
		}

        protected SigCommandBase(string key)
        {
            if (key == null)
                Key = "";
            else
                Key = key;
        }

        /// <summary>
        /// Computes the value by running the ValueFunc and if the value has changed, updates the linked sigs and fires the OnOutputChange event
        /// </summary>
        public abstract void FireUpdate();

        /// <summary>
        /// Fires the update asynchronously within a CrestronInvoke
        /// </summary>
        public void InvokeFireUpdate()
        {
            CrestronInvoke.BeginInvoke(o => FireUpdate());
        }
    }
}