using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Wrapper class for CEN-IO-RY-104 relay module
    /// </summary>
    [Description("Wrapper class for the CEN-IO-RY-104 relay module")]
    public class CenIoRy104Controller : CrestronGenericBaseDevice, IRelayPorts
    {
        private readonly CenIoRy104 _ry104;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="ry104"></param>
        public CenIoRy104Controller(string key, string name, CenIoRy104 ry104)
            : base(key, name, ry104)
        {
            _ry104 = ry104;
        }

        /// <summary>
        /// Relay port collection
        /// </summary>
        public CrestronCollection<Relay> RelayPorts
        {
            get { return _ry104.RelayPorts; }
        }
    
        /// <summary>
        /// Number of relay ports property
        /// </summary>
        public int NumberOfRelayPorts
        {
            get { return _ry104.NumberOfRelayPorts; }
        }        
    }
}