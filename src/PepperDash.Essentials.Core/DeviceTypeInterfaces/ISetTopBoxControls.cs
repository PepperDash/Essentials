using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// Defines the contract for ISetTopBoxControls
	/// </summary>
	public interface ISetTopBoxControls : IChannel, IColor, IDPad, ISetTopBoxNumericKeypad, 
		ITransport, IUiDisplayInfo
	{
		/// <summary>
		/// Show DVR controls?
		/// </summary>
		bool HasDvr { get; }

		/// <summary>
		/// Show presets controls?
		/// </summary>
		bool HasPresets { get; }

		/// <summary>
		/// Show number pad controls?
		/// </summary>
		bool HasNumeric { get; }

		/// <summary>
		/// Show D-pad controls?
		/// </summary>
		bool HasDpad { get; }

		/// <summary>
		/// TV Presets model
		/// </summary>
		PepperDash.Essentials.Core.Presets.DevicePresetsModel TvPresets { get; }

		/// <summary>
		/// LoadPresets method
		/// </summary>
		/// <param name="filePath">path to file that contains the presets</param>
		void LoadPresets(string filePath);

		/// <summary>
		/// DvrList button action
		/// </summary>
		/// <param name="pressRelease">determines if the button action is a press or release</param>
		void DvrList(bool pressRelease);

		/// <summary>
		/// Replay button action
		/// </summary>
		/// <param name="pressRelease">determines if the button action is a press or release</param>
		void Replay(bool pressRelease);
	}

	/// <summary>
	/// ISetTopBoxControlsExtensions class
	/// </summary>
	public static class ISetTopBoxControlsExtensions
	{
		/// <summary>
		/// LinkButtons method
		/// </summary>
		/// <param name="dev">The ISetTopBoxControls device</param>
		/// <param name="triList">The BasicTriList to link buttons to</param>
		public static void LinkButtons(this ISetTopBoxControls dev, BasicTriList triList)
		{
			triList.SetBoolSigAction(136, dev.DvrList);
			triList.SetBoolSigAction(152, dev.Replay);
		}

		/// <summary>
		/// UnlinkButtons method
		/// </summary>
		/// <param name="dev">The ISetTopBoxControls device</param>
		/// <param name="triList">The BasicTriList to unlink buttons from</param>
		public static void UnlinkButtons(this ISetTopBoxControls dev, BasicTriList triList)
		{
			triList.ClearBoolSigAction(136);
			triList.ClearBoolSigAction(152);
		}
	}
}