using PepperDash.Essentials.Core.Shades;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Devices;

namespace PepperDash.Essentials.Devices.Common.Shades
{
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
