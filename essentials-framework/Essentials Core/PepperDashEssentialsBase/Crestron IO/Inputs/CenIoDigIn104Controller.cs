using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Wrapper class for CEN-IO-DIGIN-104 digital input module
    /// </summary>
    public class CenIoDigIn104Controller : Device, IDigitalInputPorts
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