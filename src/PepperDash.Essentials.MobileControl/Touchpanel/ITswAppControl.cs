using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Touchpanel
{
    public interface ITswAppControl : IKeyed
    {
        BoolFeedback AppOpenFeedback { get; }

        void HideOpenApp();

        void CloseOpenApp();

        void OpenApp();
    }

    public interface ITswZoomControl : IKeyed
    {
        BoolFeedback ZoomIncomingCallFeedback { get; }

        BoolFeedback ZoomInCallFeedback { get; }

        void EndZoomCall();
    }
}
