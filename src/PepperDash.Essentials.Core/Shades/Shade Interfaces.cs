using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Defines the contract for IShades
    /// </summary>
    public interface IShades
    {
		/// <summary>
		/// List of shades controlled by this device
		/// </summary>
        List<IShadesOpenCloseStop> Shades { get; }
    }

    /// <summary>
    /// Requirements for a device that implements basic Open/Close/Stop shade control (Uses 3 relays)
    /// </summary>
    public interface IShadesOpenCloseStop
    {
		/// <summary>
		/// Opens the shade
		/// </summary>
        void Open();

		/// <summary>
		/// Closes the shade
		/// </summary>
        void Close();

		/// <summary>
		/// Stops the shade
		/// </summary>
        void Stop();
    }

	/// <summary>
	/// Requirements for a device that implements Open/Close/Stop shade control with presets
	/// </summary>
    public interface IShadesOpenClosePreset : IShadesOpenCloseStop
    {
		/// <summary>
		/// Recalls the preset
		/// </summary>
		/// <param name="presetNumber">preset number to recall</param>
        void RecallPreset(uint presetNumber);

		/// <summary>
		/// Saves the preset
		/// </summary>
		/// <param name="presetNumber">preset number to save</param>
        void SavePreset(uint presetNumber);

		/// <summary>
		/// Label for the preset button
		/// </summary>
        string StopOrPresetButtonLabel { get; }

		/// <summary>
		/// Event raised when a preset is recalled
		/// </summary>
        event EventHandler PresetSaved;
    }


    /// <summary>
    /// Defines the contract for IShadesRaiseLowerFeedback
    /// </summary>
    public interface IShadesRaiseLowerFeedback
    {
		/// <summary>
		/// Feedback to indicate if the shade is lowering
		/// </summary>
		BoolFeedback ShadeIsLoweringFeedback { get; }

		/// <summary>
		/// Feedback to indicate if the shade is raising
		/// </summary>
		BoolFeedback ShadeIsRaisingFeedback { get; }
    }

	/// <summary>
	/// Requirements for a shade/scene that is open or closed
	/// </summary>
	public interface IShadesOpenClosedFeedback: IShadesOpenCloseStop
	{
		/// <summary>
		/// Feedback to indicate if the shade is open
		/// </summary>
		BoolFeedback ShadeIsOpenFeedback { get; }

		/// <summary>
		/// Feedback to indicate if the shade is closed
		/// </summary>
		BoolFeedback ShadeIsClosedFeedback { get; }
	}

	/// <summary>
	/// Used to implement raise/stop/lower/stop from single button
	/// </summary>
	public interface IShadesStopOrMove
	{
		/// <summary>
		/// Raises the shade or stops it if it's already moving
		/// </summary>
		void OpenOrStop();

		/// <summary>
		/// Lowers the shade or stops it if it's already moving
		/// </summary>
		void CloseOrStop();

		/// <summary>
		/// Opens, closes, or stops the shade depending on current state
		/// </summary>
		void OpenCloseOrStop();
	}

 /// <summary>
 /// Defines the contract for IShadesStopFeedback
 /// </summary>
	public interface IShadesStopFeedback : IShadesOpenCloseStop
	{
		/// <summary>
		/// Feedback to indicate if the shade is stopped
		/// </summary>
		BoolFeedback IsStoppedFeedback { get; }
	}	
	
	/// <summary>
	/// Requirements for position
	/// </summary>
	public interface IShadesPosition
	{
		/// <summary>
		/// Gets the current position of the shade
		/// </summary>
		/// <param name="value">value of the position to set</param>
		void SetPosition(ushort value);
	}

	/// <summary>
	/// Basic feedback for shades position
	/// </summary>
	public interface IShadesFeedback: IShadesPosition, IShadesStopFeedback
	{
		/// <summary>
		/// Feedback to indicate the current position of the shade
		/// </summary>
		IntFeedback PositionFeedback { get; }
	}

	/// <summary>
	/// Feedback for scenes
	/// </summary>
	public interface ISceneFeedback
	{
		/// <summary>
		/// Runs the scene
		/// </summary>
		void Run();

		/// <summary>
		/// Feedback to indicate if all shades are at the scene position
		/// </summary>
		BoolFeedback AllAreAtSceneFeedback { get; }
	}

	/// <summary>
	/// Combines basic shade interfaces for Crestron Basic shades
	/// </summary>
	public interface ICrestronBasicShade : IShadesOpenClosedFeedback, 
		IShadesStopOrMove, IShadesFeedback, IShadesRaiseLowerFeedback
	{

	}
}