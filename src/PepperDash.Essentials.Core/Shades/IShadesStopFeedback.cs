using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Defines the contract for IShadesStopFeedback
    /// </summary>
    public interface IShadesStopFeedback : IShadesOpenCloseStop
	{
		BoolFeedback IsStoppedFeedback { get; }
	}	
}