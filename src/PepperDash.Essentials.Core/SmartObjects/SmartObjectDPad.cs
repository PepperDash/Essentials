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
 /// Represents a SmartObjectDPad
 /// </summary>
	public class SmartObjectDPad : SmartObjectHelperBase
	{
    /// <summary>
    /// Gets or sets the SigUp
    /// </summary>
		public BoolOutputSig SigUp { get { return GetBoolOutputNamed("Up"); } }

    /// <summary>
    /// Gets or sets the SigDown
    /// </summary>
		public BoolOutputSig SigDown { get { return GetBoolOutputNamed("Down"); } }

    /// <summary>
    /// Gets or sets the SigLeft
    /// </summary>
		public BoolOutputSig SigLeft { get { return GetBoolOutputNamed("Left"); } }

    /// <summary>
    /// Gets or sets the SigRight
    /// </summary>
		public BoolOutputSig SigRight { get { return GetBoolOutputNamed("Right"); } }

    /// <summary>
    /// Gets or sets the SigCenter
    /// </summary>
		public BoolOutputSig SigCenter { get { return GetBoolOutputNamed("Center"); } }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="so">smart object</param>
    /// <param name="useUserObjectHandler">use user object handler if true</param>
		public SmartObjectDPad(SmartObject so, bool useUserObjectHandler)
			: base(so, useUserObjectHandler)
		{
		}
	}
}