namespace PepperDash.Essentials.Core.Devices
{
    /// <summary>
    /// Represents a codec command that might need to have a friendly label applied for UI feedback purposes
    /// </summary>
    public class CodecCommandWithLabel
    {
        public string Command { get; set; }
        public string Label { get; set; }

        public CodecCommandWithLabel(string command, string label)
        {
            Command = command;
            Label = label;
        }
    }
}