namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
  /// <summary>
  /// Represents a CodecCommandWithLabel
  /// </summary>
  public class CodecCommandWithLabel
  {
    /// <summary>
    /// Gets or sets the Command
    /// </summary>
    public string Command { get; private set; }
    /// <summary>
    /// Gets or sets the Label
    /// </summary>
    public string Label { get; private set; }

    /// <summary>
    /// Constructor for <see cref="CodecCommandWithLabel"/>
    /// </summary>
    /// <param name="command">Command string</param>
    /// <param name="label">Label string</param>
    public CodecCommandWithLabel(string command, string label)
    {
      Command = command;
      Label = label;
    }
  }



}