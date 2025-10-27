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
        /// Gets the HelpRequstResponseFeedback
        /// </summary>
        StringFeedback HelpRequestResponseFeedback { get; }

        /// <summary>
        /// Sends a help request
        /// </summary>
        /// <param name="isHtml"></param>
        void SendHelpRequest(bool isHtml);
    }
}
