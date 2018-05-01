using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Shades
{
    public class ShadeController : Device, IShades
    {
        public List<ShadeBase> IShades.Shades { get; private set; }

    }

    public abstract class ShadeBase : Device, iShadesRaiseLower
    {
        public ShadeBase(string key, string name)
            : base(key, name)
        {

        }

        #region iShadesRaiseLower Members

        public void iShadesRaiseLower.Raise();
        public void iShadesRaiseLower.Lower();
        public void iShadesRaiseLower.Stop();

        #endregion
    }
}