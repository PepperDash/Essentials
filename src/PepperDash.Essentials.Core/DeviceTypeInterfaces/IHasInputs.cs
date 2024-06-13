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
    [Obsolete("Use IHasInputs<T> instead.  Will be removed for 2.0 release")]
    public interface IHasInputs<T, TSelector>: IKeyName
    {
        ISelectableItems<T> Inputs { get; }
    }


    /// <summary>
    /// Describes a device that has selectable inputs
    /// </summary>
    /// <typeparam name="T">the type to use as the key for each input item. Most likely an enum or string</typeparam>\
    /// <example>
    /// See MockDisplay for example implemntation
    /// </example>
    public interface IHasInputs<T> : IKeyName
    {
        ISelectableItems<T> Inputs { get; }
    }
}
