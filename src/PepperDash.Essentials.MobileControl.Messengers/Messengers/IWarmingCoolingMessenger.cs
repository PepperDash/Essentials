using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers;

/// <summary>
/// Messenger for devices that implement <see cref="IWarmingCooling"/>
/// </summary>
public class IWarmingCoolingMessenger : MessengerBase
{
    private readonly IWarmingCooling device;

    /// <summary>
    /// Initializes a new instance of the <see cref="IWarmingCoolingMessenger"/> class.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="messagePath"></param>
    /// <param name="device"></param>
    public IWarmingCoolingMessenger(string key, string messagePath, EssentialsDevice device)
        : base(key, messagePath, device)
    {
        this.device = device as IWarmingCooling;
    }

    /// <inheritdoc />
    protected override void RegisterActions()
    {
        base.RegisterActions();

        AddAction("/fullStatus", (id, content) => SendFullStatus(id));

        AddAction("/warmingCoolingStatus", (id, content) => SendFullStatus(id));

        device.IsWarmingUpFeedback.OutputChange += IsWarmingFeedbackOnOutputChange;
        device.IsCoolingDownFeedback.OutputChange += IsCoolingFeedbackOnOutputChange;
    }

    private void IsWarmingFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
    {
        PostStatusMessage(JToken.FromObject(new
        {
            isWarming = feedbackEventArgs.BoolValue
        }));
    }

    private void IsCoolingFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
    {
        PostStatusMessage(JToken.FromObject(new
        {
            isCooling = feedbackEventArgs.BoolValue
        }));
    }

    private void SendFullStatus(string id = null)
    {
        var messageObj = new IWarmingCoolingStateMessage
        {
            IsWarming = device.IsWarmingUpFeedback.BoolValue,
            IsCooling = device.IsCoolingDownFeedback.BoolValue
        };

        PostStatusMessage(messageObj, id);
    }
}


/// <summary>
/// Message object for warming/cooling status
/// </summary>
public class IWarmingCoolingStateMessage : DeviceStateMessageBase
{
    /// <summary>
    /// Indicates whether the device is currently warming up.
    /// </summary>
    [JsonProperty("isWarming", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsWarming { get; set; }

    /// <summary>
    /// Indicates whether the device is currently cooling down.
    /// </summary>
    [JsonProperty("isCooling", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsCooling { get; set; }
}

