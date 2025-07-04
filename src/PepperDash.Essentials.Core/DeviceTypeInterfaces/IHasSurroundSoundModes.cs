using PepperDash.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

/// <summary>
/// Describes a device that has selectable surround sound modes
/// </summary>
/// <typeparam name="TKey">the type to use as the key for each input item. Most likely an enum or string</typeparam>
public interface IHasSurroundSoundModes<TKey, TSelector>: IKeyName
{
    ISelectableItems<TKey> SurroundSoundModes { get; }

    void SetSurroundSoundMode(TSelector selector);
}
