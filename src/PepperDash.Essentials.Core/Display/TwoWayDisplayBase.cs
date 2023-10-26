using System;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TwoWayDisplayBase : DisplayBase, IRoutingFeedback, IHasPowerControlWithFeedback
    {
        public StringFeedback CurrentInputFeedback { get; private set; }

        abstract protected Func<string> CurrentInputFeedbackFunc { get; }

        public override BoolFeedback PowerIsOnFeedback { get; protected set; }

        abstract protected Func<bool> PowerIsOnFeedbackFunc { get; }


        public static MockDisplay DefaultDisplay
        { 
            get 
            {
                if (_DefaultDisplay == null)
                    _DefaultDisplay = new MockDisplay("default", "Default Display");
                return _DefaultDisplay;
            } 
        }
        static MockDisplay _DefaultDisplay;

        public TwoWayDisplayBase(string key, string name)
            : base(key, name)
        {
            CurrentInputFeedback = new StringFeedback(CurrentInputFeedbackFunc);

            WarmupTime   = 7000;
            CooldownTime = 15000;

            PowerIsOnFeedback = new BoolFeedback("PowerOnFeedback", PowerIsOnFeedbackFunc);

            Feedbacks.Add(CurrentInputFeedback);
            Feedbacks.Add(PowerIsOnFeedback);

            PowerIsOnFeedback.OutputChange += PowerIsOnFeedback_OutputChange;

        }

        void PowerIsOnFeedback_OutputChange(object sender, EventArgs e)
        {
            if (UsageTracker != null)
            {
                if (PowerIsOnFeedback.BoolValue)
                    UsageTracker.StartDeviceUsage();
                else
                    UsageTracker.EndDeviceUsage();
            }
        }

        public event EventHandler<RoutingNumericEventArgs> NumericSwitchChange;

        /// <summary>
        /// Raise an event when the status of a switch object changes.
        /// </summary>
        /// <param name="e">Arguments defined as IKeyName sender, output, input, and eRoutingSignalType</param>
        protected void OnSwitchChange(RoutingNumericEventArgs e)
        {
            var newEvent = NumericSwitchChange;
            if (newEvent != null) newEvent(this, e);
        }
    }
}