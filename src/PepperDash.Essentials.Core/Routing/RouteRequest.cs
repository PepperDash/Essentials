namespace PepperDash.Essentials.Core
{
    public class RouteRequest
    {
        public IRoutingSink Destination {get; set;}
        public IRoutingOutputs Source {get; set;}
        public eRoutingSignalType SignalType {get; set;}

        public void HandleCooldown(object sender, FeedbackEventArgs args)
        {
            var coolingDevice = sender as IWarmingCooling;
            
            if(args.BoolValue == false)
            {
                Destination.ReleaseAndMakeRoute(Source, SignalType);
                
                if(sender == null) return;

                coolingDevice.IsCoolingDownFeedback.OutputChange -= HandleCooldown;
            }
        }
    }

    /*public class RouteRequest<TInputSelector, TOutputSelector>
    {
        public IRoutingSink<TInputSelector> Destination { get; set; }
        public IRoutingOutputs<TOutputSelector> Source { get; set; }
        public eRoutingSignalType SignalType { get; set; }

        public void HandleCooldown(object sender, FeedbackEventArgs args)
        {
            var coolingDevice = sender as IWarmingCooling;

            if (args.BoolValue == false)
            {
                Destination.ReleaseAndMakeRoute(Source, SignalType);

                if (sender == null) return;

                coolingDevice.IsCoolingDownFeedback.OutputChange -= HandleCooldown;
            }
        }
    }*/
}