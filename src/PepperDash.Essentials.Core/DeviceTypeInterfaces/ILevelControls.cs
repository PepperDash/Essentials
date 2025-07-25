using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
