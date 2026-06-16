using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers;

/// <summary>
/// Messenger that accepts generic device actions at /action/executeAction
/// and executes them using DeviceJsonApi.DoDeviceAction
/// </summary>
public class DeviceActionMessenger : MessengerBase
{
    public DeviceActionMessenger(string key, string messagePath)
        : base(key, messagePath)
    {
    }

    protected override void RegisterActions()
    {
        AddAction("/executeAction", (id, content) =>
        {
            var action = content.ToObject<DeviceActionWrapper>();

            if (action == null)
            {
                this.LogWarning("Received null DeviceActionWrapper");
                return;
            }

            DeviceJsonApi.DoDeviceAction(action);
        });
    }
}
