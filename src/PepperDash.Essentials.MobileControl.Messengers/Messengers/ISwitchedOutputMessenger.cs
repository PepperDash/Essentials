using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.CrestronIO;
using System;

namespace PepperDash.Essentials.AppServer.Messengers;

public class ISwitchedOutputMessenger : MessengerBase
{

    private readonly ISwitchedOutput device;

    public ISwitchedOutputMessenger(string key, ISwitchedOutput device, string messagePath)
        : base(key, messagePath, device as IKeyName)
    {
        this.device = device;
    }

    protected override void RegisterActions()
    {
        base.RegisterActions();

        AddAction("/fullStatus", (id, content) => SendFullStatus());

        AddAction("/on", (id, content) =>
        {

            device.On();

        });

        AddAction("/off", (id, content) =>
        {

            device.Off();

        });

        device.OutputIsOnFeedback.OutputChange += new EventHandler<Core.FeedbackEventArgs>((o, a) => SendFullStatus());
    }

    private void SendFullStatus()
    {
        var state = new ISwitchedOutputStateMessage
        {
            IsOn = device.OutputIsOnFeedback.BoolValue
        };

        PostStatusMessage(state);
    }
}

public class ISwitchedOutputStateMessage : DeviceStateMessageBase
{
    [JsonProperty("isOn")]
    public bool IsOn { get; set; }
}
