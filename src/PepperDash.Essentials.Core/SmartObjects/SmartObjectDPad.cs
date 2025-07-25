using Crestron.SimplSharpPro;

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

		public SmartObjectDPad(SmartObject so, bool useUserObjectHandler)
			: base(so, useUserObjectHandler)
		{
		}
	}
}