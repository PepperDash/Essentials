using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    public class BoolFeedbackLinq : BoolFeedbackLogic
    {
        readonly Func<IEnumerable<BoolFeedback>, bool> _predicate;

        public BoolFeedbackLinq(Func<IEnumerable<BoolFeedback>, bool> predicate)
            : base()
        {
            _predicate = predicate;
        }

        protected override void Evaluate()
        {
            var prevValue = ComputedValue;
            var newValue  = _predicate(OutputsIn);
            if (newValue == prevValue)
            {
                return;
            }
            ComputedValue = newValue;
            Output.FireUpdate();
        } 
    }
}