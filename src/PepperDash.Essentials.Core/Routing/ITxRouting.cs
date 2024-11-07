using PepperDash.Essentials.Core.Feedbacks;

namespace PepperDash.Essentials.Core.Routing
{
    public interface ITxRouting : IRoutingNumeric
    {
        IntFeedback VideoSourceNumericFeedback { get; }
        IntFeedback AudioSourceNumericFeedback { get; }
    }
}