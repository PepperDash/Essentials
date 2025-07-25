using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Touchpanel
{
    /// <summary>
    /// Defines the contract for ITswAppControl
    /// </summary>
    public interface ITswAppControl : IKeyed
    {
        BoolFeedback AppOpenFeedback { get; }

        void HideOpenApp();

        void CloseOpenApp();

        void OpenApp();
    }

    /// <summary>
    /// Defines the contract for ITswZoomControl
    /// </summary>
    public interface ITswZoomControl : IKeyed
    {
        BoolFeedback ZoomIncomingCallFeedback { get; }

        BoolFeedback ZoomInCallFeedback { get; }

        void EndZoomCall();
    }
}
