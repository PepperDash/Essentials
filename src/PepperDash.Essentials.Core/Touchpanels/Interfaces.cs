using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IHasBasicTriListWithSmartObject
    /// </summary>
    public interface IHasBasicTriListWithSmartObject
    {
        BasicTriListWithSmartObject Panel { get; }
    }
}