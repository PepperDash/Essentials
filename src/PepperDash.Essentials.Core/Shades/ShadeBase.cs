using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Core.Shades
{

    [Obsolete("Please use PepperDash.Essentials.Devices.Common")]
    public abstract class ShadeBase : EssentialsDevice, IShadesOpenCloseStop
    {
        public ShadeBase(string key, string name)
            : base(key, name)
        {

        }

        #region iShadesOpenClose Members

        public abstract void Open();
        public abstract void Stop();
        public abstract void Close();

        #endregion
    }
}