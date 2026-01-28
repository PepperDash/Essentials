using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core.SmartObjects
{
	/// <summary>
	/// Represents a SmartObjectHelperBase
	/// </summary>
	public class SmartObjectHelperBase
	{
		/// <summary>
		/// Gets or sets the SmartObject
		/// </summary>
		public SmartObject SmartObject { get; private set; }

		/// <summary>
		/// Gets or sets the Validated
		/// </summary>
		public bool Validated { get; protected set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="so">smart object</param>
		/// <param name="useUserObjectHandler">use the user object hadnler if true</param>
		public SmartObjectHelperBase(SmartObject so, bool useUserObjectHandler)
		{
			SmartObject = so;
			if (useUserObjectHandler)
			{
				// Prevent this from double-registering
				SmartObject.SigChange -= this.SmartObject_SigChange;
				SmartObject.SigChange += this.SmartObject_SigChange;
			}
		}

		/// <summary>
		/// Destructor
		/// </summary>
		~SmartObjectHelperBase()
		{
			SmartObject.SigChange -= this.SmartObject_SigChange;
		}

        /// <summary>
        /// Helper to get a sig name with debugging when fail
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
		public BoolOutputSig GetBoolOutputNamed(string name)
		{
			if (SmartObject.BooleanOutput.Contains(name))
				return SmartObject.BooleanOutput[name];
            else
                Debug.LogMessage(LogEventLevel.Information, "WARNING: Cannot get signal. Smart object {0} on trilist {1:x2} does not contain signal '{2}'",
                    SmartObject.ID, SmartObject.Device.ID, name);
			return null;
		}

        /// <summary>
        /// Sets action on signal after checking for existence.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="a"></param>
        /// <summary>
        /// SetBoolAction method
        /// </summary>
        public void SetBoolAction(string name, Action<bool> a)
        {
            if (SmartObject.BooleanOutput.Contains(name))
                SmartObject.BooleanOutput[name].UserObject = a;
            else
            {
                Debug.LogMessage(LogEventLevel.Information, "WARNING: Cannot set action. Smart object {0} on trilist {1:x2} does not contain signal '{2}'",
                    SmartObject.ID, SmartObject.Device.ID, name);
            }
        }

		/// <summary>
		/// Standard Action listener
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		void SmartObject_SigChange(GenericBase currentDevice, SmartObjectEventArgs args)
		{
			var uo = args.Sig.UserObject;
			if (uo is Action<bool>)
				(uo as Action<bool>)(args.Sig.BoolValue);
			else if (uo is Action<ushort>)
				(uo as Action<ushort>)(args.Sig.UShortValue);
			else if (uo is Action<string>)
				(uo as Action<string>)(args.Sig.StringValue);
		}

	}
}