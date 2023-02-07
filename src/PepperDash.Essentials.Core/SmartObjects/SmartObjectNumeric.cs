﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.SmartObjects
{
	public class SmartObjectNumeric : SmartObjectHelperBase
	{
        /// <summary>
        /// Defaults to "Misc_1".  The name of the button in VTPro (Usually the text)
        /// </summary>
        public string Misc1SigName { get; set; }
        /// <summary>
        /// Defaults to "Misc_2".  The name of the button in VTPro (Usually the text)
        /// </summary>
        public string Misc2SigName { get; set; }

		public BoolOutputSig Digit1 { get { return GetBoolOutputNamed("1"); } }
		public BoolOutputSig Digit2 { get { return GetBoolOutputNamed("2"); } }
		public BoolOutputSig Digit3 { get { return GetBoolOutputNamed("3"); } }
		public BoolOutputSig Digit4 { get { return GetBoolOutputNamed("4"); } }
		public BoolOutputSig Digit5 { get { return GetBoolOutputNamed("5"); } }
		public BoolOutputSig Digit6 { get { return GetBoolOutputNamed("6"); } }
		public BoolOutputSig Digit7 { get { return GetBoolOutputNamed("7"); } }
		public BoolOutputSig Digit8 { get { return GetBoolOutputNamed("8"); } }
		public BoolOutputSig Digit9 { get { return GetBoolOutputNamed("9"); } }
		public BoolOutputSig Digit0 { get { return GetBoolOutputNamed("0"); } }
		public BoolOutputSig Misc1 { get { return GetBoolOutputNamed(Misc1SigName); } }
		public BoolOutputSig Misc2 { get { return GetBoolOutputNamed(Misc2SigName); } }

		public SmartObjectNumeric(SmartObject so, bool useUserObjectHandler) : base(so, useUserObjectHandler) 
		{
            Misc1SigName = "Misc_1";
            Misc2SigName = "Misc_2";
		}
	}
}