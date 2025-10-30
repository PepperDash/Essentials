

namespace PepperDash.Essentials.Core.Fusion
{
    /// <summary>
    /// Enumeration of possible Fusion Help Responses based on the standard responses from Fusion
    /// </summary>
    public enum eFusionHelpResponse
    {
        /// <summary>
        /// No help response
        /// </summary>
        None,
        /// <summary>
        /// Help has been requested
        /// </summary>
        HelpRequested,
        /// <summary>
        /// Help is on the way
        /// </summary>
        HelpOnTheWay,
        /// <summary>
        /// Please call the helpdesk.
        /// </summary>
        CallHelpDesk,
        /// <summary>
        /// Rescheduling meeting.
        /// </summary>
        ReschedulingMeeting,

        /// <summary>
        /// Technician taking control.
        /// </summary>
        TakingControl,
    }

}