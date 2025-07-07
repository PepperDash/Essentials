using Newtonsoft.Json;


namespace PepperDash.Essentials;

/// <summary>
/// Represents a plugin that is incompatible with the current system or configuration.
/// </summary>
/// <remarks>This class provides details about an incompatible plugin, including its name, the reason for the
/// incompatibility, and the plugin that triggered the incompatibility. The triggering plugin can be updated dynamically
/// using the <see cref="UpdateTriggeringPlugin(string)"/> method.</remarks>
/// <param name="name"></param>
/// <param name="reason"></param>
/// <param name="triggeredBy"></param>
public class IncompatiblePlugin(string name, string reason, string triggeredBy = null)
{
    /// <summary>
    /// Gets the name associated with the object.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; private set; } = name;

    /// <summary>
    /// Gets the reason associated with the current operation or response.
    /// </summary>
    [JsonProperty("reason")]
    public string Reason { get; private set; } = reason;

    /// <summary>
    /// Gets the identifier of the entity or process that triggered the current operation.
    /// </summary>
    [JsonProperty("triggeredBy")]
    public string TriggeredBy { get; private set; } = triggeredBy ?? "Direct load";

    /// <summary>
    /// Updates the name of the plugin that triggered the current operation.
    /// </summary>
    /// <param name="triggeringPlugin">The name of the triggering plugin. Must not be null or empty. If the value is null or empty, the operation is
    /// ignored.</param>
    public void UpdateTriggeringPlugin(string triggeringPlugin)
    {
        if (!string.IsNullOrEmpty(triggeringPlugin))
        {
            TriggeredBy = triggeringPlugin;
        }
    }
}
