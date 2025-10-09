using Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.SoftCodec
{
  /// <summary>
  /// Represents a GenericSoftCodecProperties
  /// </summary>
  public class GenericSoftCodecProperties
  {
    /// <summary>
    /// Gets or sets the HasCameraInputs
    /// </summary>
    [JsonProperty("hasCameraInputs")]
    public bool HasCameraInputs { get; set; }

    /// <summary>
    /// Gets or sets the CameraInputCount
    /// </summary>
    [JsonProperty("cameraInputCount")]
    public int CameraInputCount { get; set; }

    /// <summary>
    /// Gets or sets the ContentInputCount
    /// </summary>
    [JsonProperty("contentInputCount")]
    public int ContentInputCount { get; set; }

    /// <summary>
    /// Gets or sets the OutputCount
    /// </summary>
    [JsonProperty("contentOutputCount")]
    public int OutputCount { get; set; }
  }
}
