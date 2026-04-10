using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace PepperDash.Essentials.Core.SmartObjects
{
      /// <summary>
      /// Represents a SmartObjectNumeric
      /// </summary>
	public class SmartObjectNumeric : SmartObjectHelperBase
      {
            /// <summary>
            /// Gets or sets the Misc1SigName
            /// </summary>
            public string Misc1SigName { get; set; }

            /// <summary>
            /// Gets or sets the Misc2SigName
            /// </summary>
            public string Misc2SigName { get; set; }

            /// <summary>
            /// Gets or sets the Digit1
            /// </summary>
            public BoolOutputSig Digit1 { get { return GetBoolOutputNamed("1"); } }

            /// <summary>
            /// Gets or sets the Digit2
            /// </summary>
            public BoolOutputSig Digit2 { get { return GetBoolOutputNamed("2"); } }

            /// <summary>
            /// Gets or sets the Digit3
            /// </summary>
            public BoolOutputSig Digit3 { get { return GetBoolOutputNamed("3"); } }

            /// <summary>
            /// Gets or sets the Digit4
            /// </summary>
            public BoolOutputSig Digit4 { get { return GetBoolOutputNamed("4"); } }

            /// <summary>
            /// Gets or sets the Digit5
            /// </summary>
            public BoolOutputSig Digit5 { get { return GetBoolOutputNamed("5"); } }

            /// <summary>
            /// Gets or sets the Digit6
            /// </summary>
            public BoolOutputSig Digit6 { get { return GetBoolOutputNamed("6"); } }

            /// <summary>
            /// Gets or sets the Digit7
            /// </summary>
            public BoolOutputSig Digit7 { get { return GetBoolOutputNamed("7"); } }

            /// <summary>
            /// Gets or sets the Digit8
            /// </summary>
            public BoolOutputSig Digit8 { get { return GetBoolOutputNamed("8"); } }

            /// <summary>
            /// Gets or sets the Digit9
            /// </summary>
            public BoolOutputSig Digit9 { get { return GetBoolOutputNamed("9"); } }

            /// <summary>
            /// Gets or sets the Digit0
            /// </summary>
            public BoolOutputSig Digit0 { get { return GetBoolOutputNamed("0"); } }

            /// <summary>
            /// Gets or sets the Misc1
            /// </summary>
            public BoolOutputSig Misc1 { get { return GetBoolOutputNamed(Misc1SigName); } }

            /// <summary>
            /// Gets or sets the Misc2
            /// </summary>
            public BoolOutputSig Misc2 { get { return GetBoolOutputNamed(Misc2SigName); } }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="so">smart object</param>
            /// <param name="useUserObjectHandler">use user handler if true</param>
            public SmartObjectNumeric(SmartObject so, bool useUserObjectHandler) : base(so, useUserObjectHandler)
            {
                  Misc1SigName = "Misc_1";
                  Misc2SigName = "Misc_2";
            }
      }
}