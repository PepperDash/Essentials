using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core
{
    public class RouteRequest
    {
        public RoutingInputPort DestinationPort { get; set; }

        public RoutingOutputPort SourcePort { get; set; }
        public IRoutingInputs Destination { get; set; }
        public IRoutingOutputs Source { get; set; }
        public eRoutingSignalType SignalType { get; set; }

        public void HandleCooldown(object sender, FeedbackEventArgs args)
        {
            Debug.LogMessage(LogEventLevel.Information, "Handling cooldown route request: {destination}:{destinationPort} -> {source}:{sourcePort} {type}", null, Destination.Key, DestinationPort.Key, Source.Key, SourcePort.Key, SignalType.ToString());

            var coolingDevice = sender as IWarmingCooling;

            if (args.BoolValue == false)
            {
                Debug.LogMessage(LogEventLevel.Information, "Cooldown complete. Making route from {destination} to {source}", Destination.Key, Source.Key);
                Destination.ReleaseAndMakeRoute(Source, SignalType);

                if (sender == null) return;

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