using System.Linq;

namespace PepperDash.Essentials.Core
{
    public class BoolFeedbackOr : BoolFeedbackLogic
    {
        protected override void Evaluate()
        {
            var prevValue = ComputedValue;
            var newValue  = OutputsIn.Any(o => o.BoolValue);
            if (newValue == prevValue)
            {
                return;
            }
            ComputedValue = newValue;
            Output.FireUpdate();
        }
    }
}