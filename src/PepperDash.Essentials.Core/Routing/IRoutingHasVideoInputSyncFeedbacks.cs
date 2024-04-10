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

	public interface IRoutingHasVideoInputSyncFeedbacks
	{
		FeedbackCollection<BoolFeedback> VideoInputSyncFeedbacks { get; }
	}	
}