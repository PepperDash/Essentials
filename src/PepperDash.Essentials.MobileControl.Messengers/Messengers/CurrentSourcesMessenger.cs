using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.AppServer.Messengers
{
  /// <summary>
  /// Represents a IHasCurrentSourceInfoMessenger
  /// </summary>
  public class CurrentSourcesMessenger : MessengerBase
  {
    private readonly ICurrentSources sourceDevice;
    public CurrentSourcesMessenger(string key, string messagePath, ICurrentSources device) : base(key, messagePath, device as IKeyName)
    {
      sourceDevice = device;
    }

    protected override void RegisterActions()
    {
      base.RegisterActions();

      AddAction("/fullStatus", (id, content) =>
      {
        var message = new CurrentSourcesStateMessage
        {
          CurrentSourceKeys = sourceDevice.CurrentSourceKeys,
          CurrentSources = sourceDevice.CurrentSources
        };

        PostStatusMessage(message);
      });

      sourceDevice.CurrentSourcesChanged += (sender, e) =>
      {
        PostStatusMessage(JToken.FromObject(new
        {
          currentSourceKeys = sourceDevice.CurrentSourceKeys,
          currentSources = sourceDevice.CurrentSources
        }));
      };
    }
  }

  /// <summary>
  /// Represents a CurrentSourcesStateMessage
  /// </summary>
  public class CurrentSourcesStateMessage : DeviceStateMessageBase
  {

    /// <summary>
    /// Gets or sets the CurrentSourceKey
    /// </summary>
    [JsonProperty("currentSourceKey", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(StringEnumConverter))]
    public Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; set; }


    /// <summary>
    /// Gets or sets the CurrentSource
    /// </summary>
    [JsonProperty("currentSource")]
    public Dictionary<eRoutingSignalType, SourceListItem> CurrentSources { get; set; }
  }
}
