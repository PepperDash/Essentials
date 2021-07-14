using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Describes the functionality of a device that senses and provides partition state
    /// </summary>
    public interface IPartitionStateProvider
    {
        BoolFeedback PartitionSensedFeedback { get; }
    }
}