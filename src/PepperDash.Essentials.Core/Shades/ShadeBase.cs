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
    /// Base class for a shade device
    /// </summary>
    public abstract class ShadeBase : EssentialsDevice, IShadesOpenClose
    {
        public ShadeBase(string key, string name)
            : base(key, name)
        {

        }

        #region iShadesOpenClose Members

        public abstract void Open();
        public abstract void StopOrPreset();
        public abstract void Close();

        #endregion
    }
}