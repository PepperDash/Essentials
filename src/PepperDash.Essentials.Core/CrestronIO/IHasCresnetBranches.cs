using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the contract for IHasCresnetBranches
    /// </summary>
    public interface IHasCresnetBranches
    {
        /// <summary>
        /// Collection of Cresnet branches
        /// </summary>
        CrestronCollection<CresnetBranch> CresnetBranches { get; }
    }
}