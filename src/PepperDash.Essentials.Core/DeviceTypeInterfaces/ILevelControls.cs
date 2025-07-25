using System.Collections.Generic;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for ILevelControls
    /// </summary>
    public interface ILevelControls
    {
        Dictionary<string, IBasicVolumeWithFeedback> LevelControlPoints { get; }
    }
}
