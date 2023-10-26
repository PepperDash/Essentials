using System;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// For fixed-source endpoint devices
    /// </summary>
    [Obsolete("Please switch to IRoutingSink")]
    public interface IRoutingSinkNoSwitching : IRoutingSink
    {

    }
}