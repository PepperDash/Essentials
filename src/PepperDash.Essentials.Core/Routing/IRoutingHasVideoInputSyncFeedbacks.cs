using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Routing
{

 /// <summary>
 /// Defines the contract for IRoutingHasVideoInputSyncFeedbacks
 /// </summary>
	public interface IRoutingHasVideoInputSyncFeedbacks
	{
		FeedbackCollection<BoolFeedback> VideoInputSyncFeedbacks { get; }
	}	
}