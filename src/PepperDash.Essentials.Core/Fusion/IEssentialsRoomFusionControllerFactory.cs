using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Fusion;

/// <summary>
/// Factory for creating IEssentialsRoomFusionController devices
/// </summary>
public class IEssentialsRoomFusionControllerFactory : EssentialsDeviceFactory<IEssentialsRoomFusionController>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public IEssentialsRoomFusionControllerFactory()
    {
        TypeNames = new List<string>() { "fusionRoom" };
    }

    /// <summary>
    /// Builds the device
    /// </summary>
    /// <param name="dc"></param>
    /// <returns></returns>
    public override EssentialsDevice BuildDevice(PepperDash.Essentials.Core.Config.DeviceConfig dc)
    {
        Debug.LogDebug("Factory Attempting to create new IEssentialsRoomFusionController Device");


        var properties = dc.Properties.ToObject<IEssentialsRoomFusionControllerPropertiesConfig>();

        return new IEssentialsRoomFusionController(dc.Key, dc.Name, properties);
    }
}