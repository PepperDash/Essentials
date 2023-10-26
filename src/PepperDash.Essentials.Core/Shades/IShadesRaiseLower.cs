using System;

namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Requirements for a shade that implements press/hold raise/lower functions
    /// </summary>
    [Obsolete("Please use IShadesOpenCloseStop instead")]
    public interface IShadesRaiseLower
    {
        void Raise(bool state);
        void Lower(bool state);
    }
}