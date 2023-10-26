using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Describes the functionality of a device that senses and provides partition state
    /// </summary>
    public interface IPartitionStateProvider : IKeyName
    {
        BoolFeedback PartitionPresentFeedback { get; }
    }
}