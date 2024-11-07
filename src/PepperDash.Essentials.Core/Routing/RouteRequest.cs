using PepperDash.Core;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Feedbacks;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Routing
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

            if (args.BoolValue == true)
            {
                return;
            }

            Debug.LogMessage(LogEventLevel.Information, "Cooldown complete. Making route from {destination} to {source}", Destination.Key, Source.Key);
            Destination.ReleaseAndMakeRoute(Source, SignalType, DestinationPort?.Key ?? string.Empty, SourcePort?.Key ?? string.Empty);

            if (sender is IWarmingCooling coolingDevice)
            {
                Debug.LogMessage(LogEventLevel.Debug, "Unsubscribing from cooling feedback for {destination}", null, Destination.Key);
                coolingDevice.IsCoolingDownFeedback.OutputChange -= HandleCooldown;
            }
        }
    }
}