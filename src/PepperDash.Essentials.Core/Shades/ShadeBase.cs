using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Base class for shades
    /// </summary>
    [Obsolete("Please use PepperDash.Essentials.Devices.Common, this will be removed in 2.1")]
    public abstract class ShadeBase : EssentialsDevice, IShadesOpenCloseStop
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">key of the shade device</param>
        /// <param name="name">name of the shade device</param>
        public ShadeBase(string key, string name)
            : base(key, name)
        {

        }

        #region iShadesOpenClose Members

        /// <summary>
        /// Opens the shade
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// Stops the shade
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Closes the shade
        /// </summary>
        public abstract void Close();

        #endregion
    }
}