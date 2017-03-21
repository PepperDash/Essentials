using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.SmartObjects
{
	public class SmartObjectHelperBase
	{
		public SmartObject SmartObject { get; private set; }

		/// <summary>
		/// This should be set by all inheriting classes, after the class has verified that it is linked to the right object.
		/// </summary>
		public bool Validated { get; protected set; }

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

		~SmartObjectHelperBase()
		{
			SmartObject.SigChange -= this.SmartObject_SigChange;
		}

		public BoolOutputSig GetBoolOutputNamed(string name)
		{
			if (SmartObject.BooleanOutput.Contains(name))
				return SmartObject.BooleanOutput[name];
			return null;
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