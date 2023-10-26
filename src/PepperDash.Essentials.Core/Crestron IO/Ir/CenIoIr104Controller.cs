using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Wrapper class for CEN-IO-IR-104 module
    /// </summary>
    [Description("Wrapper class for the CEN-IO-IR-104 module")]
    public class CenIoIr104Controller : CrestronGenericBaseDevice, IIROutputPorts
    {
	    private readonly CenIoIr104 _ir104;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="ir104"></param>
        public CenIoIr104Controller(string key, string name, CenIoIr104 ir104)
            : base(key, name, ir104)
        {
            _ir104 = ir104;
        }

        #region IDigitalInputPorts Members

		/// <summary>
		/// IR port collection
		/// </summary>
		public CrestronCollection<IROutputPort> IROutputPorts
        {
            get { return _ir104.IROutputPorts; }
        }

		/// <summary>
		/// Number of relay ports property
		/// </summary>
		public int NumberOfIROutputPorts
        {
            get { return _ir104.NumberOfIROutputPorts; }
        }

        #endregion
    }
}