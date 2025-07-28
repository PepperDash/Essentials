using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Defines the contract for IShades
    /// </summary>
    public interface IShades
    {
        List<IShadesOpenCloseStop> Shades { get; }
    }
}