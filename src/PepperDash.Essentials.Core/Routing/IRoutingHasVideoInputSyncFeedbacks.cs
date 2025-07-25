namespace PepperDash.Essentials.Core
{

 /// <summary>
 /// Defines the contract for IRoutingHasVideoInputSyncFeedbacks
 /// </summary>
	public interface IRoutingHasVideoInputSyncFeedbacks
	{
		FeedbackCollection<BoolFeedback> VideoInputSyncFeedbacks { get; }
	}	
}