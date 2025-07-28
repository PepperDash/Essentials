using PepperDash.Essentials.Core.Devices;
using System;

namespace PepperDash.Essentials.Core.Shades
{

    [Obsolete("Please use PepperDash.Essentials.Devices.Common, this will be removed in 2.1")]
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