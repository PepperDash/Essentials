using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Touchpanel
{
    /// <summary>
    /// Defines the contract for ITswAppControl
    /// </summary>
    public interface ITswAppControl : IKeyed
    {
        /// <summary>
        /// Updates when the Zoom Room Control Application opens or closes
        /// </summary>
        BoolFeedback AppOpenFeedback { get; }

        /// <summary>
        /// Hide the Zoom App and show the User Control Application
        /// </summary>
        void HideOpenApp();

        /// <summary>
        /// Close the Zoom App and show the User Control Application
        /// </summary>
        void CloseOpenApp();

        /// <summary>
        /// Open the Zoom App
        /// </summary>
        void OpenApp();
    }

    /// <summary>
    /// Defines the contract for ITswZoomControl
    /// </summary>
    public interface ITswZoomControl : IKeyed
    {
        /// <summary>
        /// Updates when Zoom has an incoming call
        /// </summary>
        BoolFeedback ZoomIncomingCallFeedback { get; }

        /// <summary>
        /// Updates when Zoom is in a call
        /// </summary>
        BoolFeedback ZoomInCallFeedback { get; }

        /// <summary>
        /// End a Zoom Call
        /// </summary>
        void EndZoomCall();
    }
}
