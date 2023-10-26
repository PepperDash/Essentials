using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Wrapper to help in IR port creation
    /// </summary>
    public class IrOutPortConfig
    {
        public IROutputPort Port { get; set; }
        public string FileName { get; set; }

        public IrOutPortConfig()
        {
            FileName = "";
        }
    }
}