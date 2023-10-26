namespace PepperDash.Essentials.Core
{
    public class FileEventArgs
    {
        public FileEventArgs(string data) { Data = data; }
        public string Data { get; private set; } // readonly

    }
}