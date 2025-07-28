using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Basic feedback for shades position
    /// </summary>
    public interface IShadesFeedback: IShadesPosition, IShadesStopFeedback
	{
		IntFeedback PositionFeedback { get; }
	}
}