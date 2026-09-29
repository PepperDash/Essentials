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
  public class ICurrentSourcesMessenger : MessengerBase
  {
    private readonly ICurrentSources sourceDevice;

    /// <summary>
    /// Initializes a new instance of the <see cref="ICurrentSourcesMessenger"/> class.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="messagePath">The message path.</param>
    /// <param name="device">The device.</param>
    public ICurrentSourcesMessenger(string key, string messagePath, ICurrentSources device) : base(key, messagePath, device as IKeyName)
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
        PostStatusMessage(JToken.FromObject(new
        {
          currentSourceKeys = CopyCurrentSourceKeys(),
        }));
      };
    }

    // Copies to avoid enumeration issues, and sends a cleared source as "" rather than null:
    // clients merge status updates into existing state and skip nulls, so a null would leave the
    // previous source showing.
    private Dictionary<eRoutingSignalType, string> CopyCurrentSourceKeys() =>
      sourceDevice.CurrentSourceKeys.ToDictionary(kvp => kvp.Key, kvp => kvp.Value ?? string.Empty);

    private void SendCurrentSourceStatus(string id)
    {
      var message = new CurrentSourcesStateMessage
      {
        CurrentSourceKeys = CopyCurrentSourceKeys(),
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

  }
}
