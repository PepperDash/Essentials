using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;


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