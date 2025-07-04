using PepperDash.Core;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces;

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
