using System;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Read = Provides feedback to SIMPL
    /// Write = Responds to sig values from SIMPL
    /// </summary>
    [Flags]
    public enum eJoinCapabilities
    {
        None = 0,
        ToSIMPL = 1,
        FromSIMPL = 2,
        ToFromSIMPL = ToSIMPL | FromSIMPL,
        ToFusion = 4,
        FromFusion = 8,
        ToFromFusion = ToFusion | FromFusion,
    }
}