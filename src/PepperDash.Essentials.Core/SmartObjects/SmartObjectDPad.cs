using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.SmartObjects
{
	public class SmartObjectDPad : SmartObjectHelperBase
	{
		public BoolOutputSig SigUp { get { return GetBoolOutputNamed("Up"); } }
		public BoolOutputSig SigDown { get { return GetBoolOutputNamed("Down"); } }
		public BoolOutputSig SigLeft { get { return GetBoolOutputNamed("Left"); } }
		public BoolOutputSig SigRight { get { return GetBoolOutputNamed("Right"); } }
		public BoolOutputSig SigCenter { get { return GetBoolOutputNamed("Center"); } }

		public SmartObjectDPad(SmartObject so, bool useUserObjectHandler)
			: base(so, useUserObjectHandler)
		{
		}
	}
}