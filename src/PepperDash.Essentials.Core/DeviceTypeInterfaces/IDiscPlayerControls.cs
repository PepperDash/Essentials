using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core;


public interface IDiscPlayerControls : IColor, IDPad, INumericKeypad, IHasPowerControl, ITransport, IUiDisplayInfo
	{
	}