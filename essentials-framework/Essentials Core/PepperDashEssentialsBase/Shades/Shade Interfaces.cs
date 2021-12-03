using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Shades
{
	/// <summary>
	/// Requirements for an object that contains shades
	/// </summary>
    public interface IShades
    {
        List<ShadeBase> Shades { get; }
    }

    /// <summary>
    /// Requirements for a device that implements basic Open/Close shade control
    /// </summary>
    public interface IShadesOpenClose
    {
        void Open();
        void Close();
    }

    /// <summary>
    /// Requirements for a device that implements basic Open/Close/Stop shade control (Uses 3 relays)
    /// </summary>
    public interface IShadesOpenCloseStop : IShadesOpenClose
    {
        void StopOrPreset();
        string StopOrPresetButtonLabel;
    }

	/// <summary>
	/// Requirements for a shade that implements press/hold raise/lower functions
	/// </summary>
	public interface IShadesRaiseLower
	{
		void Raise(bool state);
		void Lower(bool state);
	}

    /// <summary>
    /// Requirements for a shade device that provides raising/lowering feedback
    /// </summary>
    public interface IShadesRaiseLowerFeedback
    {
		BoolFeedback ShadeIsLoweringFeedback { get; }
		BoolFeedback ShadeIsRaisingFeedback { get; }
    }

	/// <summary>
	/// Requirements for a shade/scene that is open or closed
	/// </summary>
	public interface IShadesOpenClosedFeedback: IShadesOpenClose
	{
		BoolFeedback ShadeIsOpenFeedback { get; }
		BoolFeedback ShadeIsClosedFeedback { get; }
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IShadesStop
	{
		void Stop();
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IShadesStopOrMove
	{
		void OpenOrStop();
		void CloseOrStop();
		void OpenCloseOrStop();
	}

	/// <summary>
	/// Basic feedback for shades/scene stopped
	/// </summary>
	public interface IShadesStopFeedback
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

	public interface ICrestronBasicShade : IShadesOpenClosedFeedback, IShadesStop, 
		IShadesStopOrMove, IShadesFeedback, IShadesRaiseLowerFeedback
	{

	}
}