using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl;

public class IDvrMessenger : MessengerBase
{
    private readonly IDvr dvrDevice;
    public IDvrMessenger(string key, string messagePath, IDvr device) : base(key, messagePath, device as IKeyName)
    {
        dvrDevice = device;
    }

    protected override void RegisterActions()
    {
        base.RegisterActions();

        AddAction("/dvrlist", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => dvrDevice?.DvrList(b)));
        AddAction("/record", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => dvrDevice?.Record(b)));
    }

}