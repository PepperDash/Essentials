using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.Fusion
{
    /// <summary>
    /// Represents Fusion Help Request functionality
    /// </summary>
    public interface IFusionHelpRequest
    {
        /// <summary>
        /// Feedback containing the response to a help request
        /// </summary>
        StringFeedback HelpRequestResponseFeedback { get; }

        /// <summary>
        /// Indicates whether a help request has been sent
        /// </summary>
        BoolFeedback HelpRequestSentFeedback { get; }

        /// <summary>
        /// Sends a help request
        /// </summary>
        /// <param name="isHtml"></param>
        void SendHelpRequest();

        /// <summary>
        /// Clears the current help request status
        /// </summary>
        void CancelHelpRequest();

        /// <summary>
        /// Toggles between sending and cancelling a help request
        /// </summary>
        void ToggleHelpRequest();
    }
}
