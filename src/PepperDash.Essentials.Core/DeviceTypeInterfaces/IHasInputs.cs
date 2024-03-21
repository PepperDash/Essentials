using PepperDash.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{

    /// <summary>
    /// Describes a device that has selectable inputs
    /// </summary>
    /// <typeparam name="TKey">the type to use as the key for each input item. Most likely an enum or string</typeparam>\
    /// <example>
    /// See MockDisplay for example implemntation
    /// </example>
    public interface IHasInputs<TKey, TSelector>: IKeyName
    {
        ISelectableItems<TKey> Inputs { get; }

        void SetInput(TSelector selector);
    }
}
