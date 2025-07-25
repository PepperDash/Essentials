using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Defines the contract for IShades
    /// </summary>
    public interface IShades
    {
        List<IShadesOpenCloseStop> Shades { get; }
    }

    /// <summary>
    /// Requirements for a device that implements basic Open/Close/Stop shade control (Uses 3 relays)
    /// </summary>
    public interface IShadesOpenCloseStop
    {
        void Open();
        void Close();
        void Stop();
    }

    public interface IShadesOpenClosePreset : IShadesOpenCloseStop
    {
        void RecallPreset(uint presetNumber);
        void SavePreset(uint presetNumber);
        string StopOrPresetButtonLabel { get; }

        event EventHandler PresetSaved;
    }


    /// <summary>
    /// Defines the contract for IShadesRaiseLowerFeedback
    /// </summary>
    public interface IShadesRaiseLowerFeedback
    {
		BoolFeedback ShadeIsLoweringFeedback { get; }
		BoolFeedback ShadeIsRaisingFeedback { get; }
    }

	/// <summary>
	/// Requirements for a shade/scene that is open or closed
	/// </summary>
	public interface IShadesOpenClosedFeedback: IShadesOpenCloseStop
	{
		BoolFeedback ShadeIsOpenFeedback { get; }
		BoolFeedback ShadeIsClosedFeedback { get; }
	}

	/// <summary>
	/// Used to implement raise/stop/lower/stop from single button
	/// </summary>
	public interface IShadesStopOrMove
	{
		void OpenOrStop();
		void CloseOrStop();
		void OpenCloseOrStop();
	}

 /// <summary>
 /// Defines the contract for IShadesStopFeedback
 /// </summary>
	public interface IShadesStopFeedback : IShadesOpenCloseStop
	{
		BoolFeedback IsStoppedFeedback { get; }
	}	
	
	/// <summary>
	/// Requirements for position
	/// </summary>
	public interface IShadesPosition
	{
		void SetPosition(ushort value);
	}

	/// <summary>
	/// Basic feedback for shades position
	/// </summary>
	public interface IShadesFeedback: IShadesPosition, IShadesStopFeedback
	{
		IntFeedback PositionFeedback { get; }
	}

	/// <summary>
	/// 
	/// </summary>
	public interface ISceneFeedback
	{
		void Run();
		BoolFeedback AllAreAtSceneFeedback { get; }
	}

	public interface ICrestronBasicShade : IShadesOpenClosedFeedback, 
		IShadesStopOrMove, IShadesFeedback, IShadesRaiseLowerFeedback
	{

	}
}