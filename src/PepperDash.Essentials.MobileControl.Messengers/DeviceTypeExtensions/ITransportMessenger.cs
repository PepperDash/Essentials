using PepperDash.Core;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Room.MobileControl;

public class ITransportMessenger : MessengerBase
{
    private readonly ITransport transportDevice;
    public ITransportMessenger(string key, string messagePath, ITransport device) : base(key, messagePath, device as IKeyName)
    {
        transportDevice = device;
    }

    protected override void RegisterActions()
    {
        base.RegisterActions();

        AddAction("/play", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.Play(b)));
        AddAction("/pause", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.Pause(b)));
        AddAction("/stop", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.Stop(b)));
        AddAction("/prevTrack", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.ChapPlus(b)));
        AddAction("/nextTrack", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.ChapMinus(b)));
        AddAction("/rewind", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.Rewind(b)));
        AddAction("/ffwd", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.FFwd(b)));
        AddAction("/record", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) => transportDevice?.Record(b)));
    }

}