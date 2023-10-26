namespace PepperDash.Essentials.Core
{
    public class CommunicationMonitorConfig
    {
        public int PollInterval { get; set; }
        public int TimeToWarning { get; set; }
        public int TimeToError { get; set; }
        public string PollString { get; set; }

        public CommunicationMonitorConfig()
        {
            PollInterval  = 30000;
            TimeToWarning = 120000;
            TimeToError   = 300000;
            PollString    = "";
        }
    }
}