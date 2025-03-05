namespace PepperDash.Essentials.Core
{
    public interface ITxRouting : IRoutingNumeric
    {
        IntFeedback VideoSourceNumericFeedback { get; }
        IntFeedback AudioSourceNumericFeedback { get; }
    }
}