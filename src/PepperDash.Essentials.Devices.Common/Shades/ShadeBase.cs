using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Shades;

namespace PepperDash.Essentials.Devices.Common.Shades
{
    /// <summary>
    /// Base class for shade devices
    /// </summary>
    public abstract class ShadeBase : EssentialsDevice, IShadesOpenCloseStop
    {
        /// <summary>
        /// Initializes a new instance of the ShadeBase class
        /// </summary>
        /// <param name="key">The device key</param>
        /// <param name="name">The device name</param>
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
