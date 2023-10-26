namespace PepperDash.Essentials.Core
{
    public class ConsoleCommMockDevicePropertiesConfig
    {
        public string LineEnding { get; set; }
        public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }

        public ConsoleCommMockDevicePropertiesConfig()
        {
            LineEnding = "\x0a";
        }
    }
}