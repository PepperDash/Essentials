namespace PepperDash.Essentials.Devices.Common.VideoCodec
{
    /// <summary>
    /// Represents a codec command that might need to have a friendly label applied for UI feedback purposes
    /// </summary>
    public class CodecCommandWithLabel
    {
        public string Command { get; private set; }
        public string Label { get; private set; }

        public CodecCommandWithLabel(string command, string label)
        {
            Command = command;
            Label   = label;
        }
    }
}