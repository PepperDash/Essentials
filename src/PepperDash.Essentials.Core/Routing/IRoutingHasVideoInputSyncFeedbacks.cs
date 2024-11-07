using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Routing
{

	public interface IRoutingHasVideoInputSyncFeedbacks
	{
		FeedbackCollection<BoolFeedback> VideoInputSyncFeedbacks { get; }
	}	
}