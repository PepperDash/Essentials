using System;
namespace PepperDash.Essentials.Core.Devices.DeviceTypeInterfaces
{
 /// <summary>
 /// Defines the contract for IDisplayBasic
 /// </summary>
 [Obsolete("This interface is no longer used and will be removed in a future version. Please use IDisplay instead.")]
	public interface IDisplayBasic
	{
		/// <summary>
		/// Sets the input to HDMI 1
		/// </summary>
		void InputHdmi1();

		/// <summary>
		/// Sets the input to HDMI 2
		/// </summary>
		void InputHdmi2();

		/// <summary>
		/// Sets the input to HDMI 3
		/// </summary>
		void InputHdmi3();

		/// <summary>
		/// Sets the input to HDMI 4
		/// </summary>
		void InputHdmi4();

		/// <summary>
		/// Sets the input to DisplayPort 1
		/// </summary>
		void InputDisplayPort1();

		/// <summary>
		/// Sets the input to DVI 1
		/// </summary>
		void InputDvi1();

		/// <summary>
		/// Sets the input to Video 1
		/// </summary>
		void InputVideo1();

		/// <summary>
		/// Sets the input to VGA 1
		/// </summary>
		void InputVga1();
		
		/// <summary>
		/// Sets the input to VGA 2
		/// </summary>
		void InputVga2();

		/// <summary>
		/// Sets the input to RGB 1
		/// </summary>
		void InputRgb1();
	}
}