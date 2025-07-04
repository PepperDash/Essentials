using System;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.Config;

/// <summary>
/// Configuration class for volume levels in the Essentials room. This is used to configure the volume levels for the master, program, audio call receive, and audio call transmit channels in the room.
/// </summary>
[Obsolete("This class is being deprecated in favor audio control point lists in the main config. It is recommended to use the DeviceKey property to get the device from the main system and then cast it to the correct type.")]
public class EssentialsRoomVolumesConfig
{
    public EssentialsVolumeLevelConfig Master { get; set; }
    public EssentialsVolumeLevelConfig Program { get; set; }
    public EssentialsVolumeLevelConfig AudioCallRx { get; set; }
    public EssentialsVolumeLevelConfig AudioCallTx { get; set; }
}

/// <summary>
/// Configuration class for a volume level in the Essentials room.
/// </summary>
public class EssentialsVolumeLevelConfig
{
    public string DeviceKey { get; set; }
    public string Label { get; set; }
    public int Level { get; set; }

    /// <summary>
    /// Helper to get the device associated with key - one timer.
    /// </summary>
    [Obsolete("This method references DM CHASSIS Directly and should not be used in the Core library. It is recommended to use the DeviceKey property to get the device from the main system and then cast it to the correct type.")]
    public IBasicVolumeWithFeedback GetDevice()
    {
        throw new NotImplementedException("This method references DM CHASSIS Directly and should not be used in the Core library. It is recommended to use the DeviceKey property to get the device from the main system and then cast it to the correct type.");
    }
}