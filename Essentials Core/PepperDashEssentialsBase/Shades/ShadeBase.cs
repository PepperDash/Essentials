using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Core.Shades
{
    public abstract class ShadeBase : Device, iShadesRaiseLower
    {
        public ISwitchedOutput SwitchedOutput;

        public ShadeBase(string key, string name)
            : base(key, name)
        {

        }

        #region iShadesRaiseLower Members

        public abstract void Open();
        public abstract void Stop();
        public abstract void Close();

        #endregion
    }
}