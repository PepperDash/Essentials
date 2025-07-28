using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// Defines the contract for IShadesRaiseLowerFeedback
    /// </summary>
    public interface IShadesRaiseLowerFeedback
    {
		BoolFeedback ShadeIsLoweringFeedback { get; }
		BoolFeedback ShadeIsRaisingFeedback { get; }
    }
}