using PepperDash.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Describes a device that has selectable surround sound modes
    /// </summary>
    /// <typeparam name="TKey">the type to use as the key for each input item. Most likely an enum or string</typeparam>
    /// <typeparam name="TSelector">the type used to select an item. Most likely an enum or string</typeparam>
    public interface IHasSurroundSoundModes<TKey, TSelector>: IKeyName
    {
        /// <summary>
        /// The available surround sound modes
        /// </summary>
        ISelectableItems<TKey> SurroundSoundModes { get; }

        /// <summary>
        /// The currently selected surround sound mode
        /// </summary>
        /// <param name="selector">the selector for the surround sound mode</param>
        void SetSurroundSoundMode(TSelector selector);
    }
}
