using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core
{

    /// <summary>
    /// Defines the contract for IDiscPlayerControls
    /// </summary>
    public interface IDiscPlayerControls : IColor, IDPad, INumericKeypad, IHasPowerControl, ITransport, IUiDisplayInfo
	{
	}

}