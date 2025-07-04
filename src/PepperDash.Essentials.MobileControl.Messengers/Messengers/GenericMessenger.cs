using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers;

public class GenericMessenger : MessengerBase
{
    public GenericMessenger(string key, EssentialsDevice device, string messagePath) : base(key, messagePath, device)
    {
    }

    protected override void RegisterActions()
    {
        base.RegisterActions();

        AddAction("/fullStatus", (id, content) => SendFullStatus());
    }

    private void SendFullStatus()
    {
        var state = new DeviceStateMessageBase();

        PostStatusMessage(state);
    }
}
