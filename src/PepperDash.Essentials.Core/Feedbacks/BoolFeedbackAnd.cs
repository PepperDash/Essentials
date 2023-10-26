using System.Linq;

namespace PepperDash.Essentials.Core
{
    public class BoolFeedbackAnd : BoolFeedbackLogic
    {
        protected override void Evaluate()
        {
            var prevValue = ComputedValue;
            var newValue  = OutputsIn.All(o => o.BoolValue);
            if (newValue == prevValue)
            {
                return;
            }
            ComputedValue = newValue;
            Output.FireUpdate();
        }
    }
}