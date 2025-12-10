using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentSourcesMessenger"/> class.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="messagePath">The message path.</param>
    /// <param name="device">The device.</param>
    public CurrentSourcesMessenger(string key, string messagePath, ICurrentSources device) : base(key, messagePath, device as IKeyName)
    {
      sourceDevice = device;
    }

    /// <summary>
    /// Registers the actions for the messenger.
    /// </summary>
    protected override void RegisterActions()
    {
      base.RegisterActions();

      AddAction("/fullStatus", (id, content) => SendCurrentSourceStatus(id));

      AddAction("/currentSourceStatus", (id, content) => SendCurrentSourceStatus(id));

      sourceDevice.CurrentSourcesChanged += (sender, e) =>
      {
        // need to copy the dictionaries to avoid enumeration issues
        var currentSourceKeys = sourceDevice.CurrentSourceKeys.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var currentSources = sourceDevice.CurrentSources.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        PostStatusMessage(JToken.FromObject(new
        {
          currentSourceKeys,
          currentSources,
        }));
      };
    }

    private void SendCurrentSourceStatus(string id = null)
    {
      var message = new CurrentSourcesStateMessage
      {
        CurrentSourceKeys = sourceDevice.CurrentSourceKeys,
        CurrentSources = sourceDevice.CurrentSources
      };

      PostStatusMessage(message, id);
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
    [JsonProperty("currentSourceKeys", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; set; }


    /// <summary>
    /// Gets or sets the CurrentSource
    /// </summary>
    [JsonProperty("currentSources")]
    public Dictionary<eRoutingSignalType, SourceListItem> CurrentSources { get; set; }
  }
}
