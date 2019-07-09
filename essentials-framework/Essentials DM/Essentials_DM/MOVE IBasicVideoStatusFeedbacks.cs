using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.DM
{
    public interface IBasicVideoStatusFeedbacks 
    {
		BoolFeedback HasVideoStatusFeedback { get; }
		BoolFeedback HdcpActiveFeedback { get; }
		StringFeedback HdcpStateFeedback { get; }
		StringFeedback VideoResolutionFeedback { get; }
		BoolFeedback VideoSyncFeedback { get; }
    }
}