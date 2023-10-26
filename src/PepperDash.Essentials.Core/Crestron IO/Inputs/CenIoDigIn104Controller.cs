using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Wrapper class for CEN-IO-DIGIN-104 digital input module
    /// </summary>
    [Description("Wrapper class for the CEN-IO-DIGIN-104 diginal input module")]
    public class CenIoDigIn104Controller : CrestronGenericBaseDevice, IDigitalInputPorts
    {
        public CenIoDi104 Di104 { get; private set; }

        public CenIoDigIn104Controller(string key, string name, CenIoDi104 di104)
            : base(key, name)
        {
            Di104 = di104;
        }

        #region IDigitalInputPorts Members

        public CrestronCollection<DigitalInput> DigitalInputPorts
        {
            get { return Di104.DigitalInputPorts; }
        }

        public int NumberOfDigitalInputPorts
        {
            get { return Di104.NumberOfDigitalInputPorts; }
        }

        #endregion
    }
}