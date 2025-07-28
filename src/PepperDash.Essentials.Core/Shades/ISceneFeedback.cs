using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Shades
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISceneFeedback
	{
		void Run();
		BoolFeedback AllAreAtSceneFeedback { get; }
	}
}