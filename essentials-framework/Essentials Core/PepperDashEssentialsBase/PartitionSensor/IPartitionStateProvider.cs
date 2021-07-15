using System;
using System.Collections.Generic;
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
        BoolFeedback PartitionSensedFeedback { get; }
    }

    public interface IManualPartitionSensor : IPartitionStateProvider
    {
        void SetPartitionStatePresent();

        void SetPartitionStateNotPresent();

        void ToggglePartitionState();
    }
}