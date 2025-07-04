using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;

namespace PepperDash.Essentials.Core;

	/// <summary>
	/// 
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

		PepperDash.Essentials.Core.Presets.DevicePresetsModel TvPresets { get; }
		void LoadPresets(string filePath);

		void DvrList(bool pressRelease);
		void Replay(bool pressRelease);
	}

	public static class ISetTopBoxControlsExtensions
	{
		public static void LinkButtons(this ISetTopBoxControls dev, BasicTriList triList)
		{
			triList.SetBoolSigAction(136, dev.DvrList);
			triList.SetBoolSigAction(152, dev.Replay);
		}

		public static void UnlinkButtons(this ISetTopBoxControls dev, BasicTriList triList)
		{
			triList.ClearBoolSigAction(136);
			triList.ClearBoolSigAction(152);
		}
	}