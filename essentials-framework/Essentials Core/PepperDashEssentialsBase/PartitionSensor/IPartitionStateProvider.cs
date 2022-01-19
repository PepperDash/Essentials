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
        BoolFeedback PartitionPresentFeedback { get; }
    }

    /// <summary>
    /// Describes the functionality of a device that can provide partition state either manually via user input or optionally via a sensor state
    /// </summary>
    public interface IPartitionController : IPartitionStateProvider
    {
        List<string> AdjacentRoomKeys { get; }

        void SetPartitionStatePresent();

        void SetPartitionStateNotPresent();

        void ToggglePartitionState();

        void SetManualMode();

        void SetAutoMode();
    }
}