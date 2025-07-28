using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Feedbacks;
using Serilog.Events;
using System;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Represents a RouteRequest
    /// </summary>
    public class RouteRequest
    {
        /// <summary>
        /// The specific input port on the destination device to use for the route. Can be null if the port should be automatically determined or is not applicable.
        /// </summary>
        public RoutingInputPort DestinationPort { get; set; }

        /// <summary>
        /// Gets or sets the SourcePort
        /// </summary>
        public RoutingOutputPort SourcePort { get; set; }

        /// <summary>
        /// Gets or sets the Destination
        /// </summary>
        public IRoutingInputs Destination { get; set; }

        /// <summary>
        /// Gets or sets the Source
        /// </summary>
        public IRoutingOutputs Source { get; set; }

        /// <summary>
        /// Gets or sets the SignalType
        /// </summary>
        public eRoutingSignalType SignalType { get; set; }

        /// <summary>
        /// Handles the route request after a device's cooldown period has finished.
        /// This method is typically subscribed to the IsCoolingDownFeedback.OutputChange event.
        /// </summary>
        /// <param name="sender">The object that triggered the event (usually the cooling device).</param>
        /// <param name="args">Event arguments indicating the cooldown state change.</param>
        public void HandleCooldown(object sender, FeedbackEventArgs args)
        {
            try
            {
                Debug.LogMessage(LogEventLevel.Information, "Handling cooldown route request: {destination}:{destinationPort} -> {source}:{sourcePort} {type}", null, Destination?.Key ?? "empty destination", DestinationPort?.Key ?? "no destination port", Source?.Key ?? "empty source", SourcePort?.Key ?? "empty source port", SignalType.ToString());

                if (args.BoolValue == true)
                {
                    return;
                }

                Debug.LogMessage(LogEventLevel.Information, "Cooldown complete. Making route from {destination} to {source}", Destination?.Key, Source?.Key);

                Destination.ReleaseAndMakeRoute(Source, SignalType, DestinationPort?.Key ?? string.Empty, SourcePort?.Key ?? string.Empty);

                if (sender is IWarmingCooling coolingDevice)
                {
                    Debug.LogMessage(LogEventLevel.Debug, "Unsubscribing from cooling feedback for {destination}", null, Destination.Key);
                    coolingDevice.IsCoolingDownFeedback.OutputChange -= HandleCooldown;
                }
            } catch(Exception ex)
            {
                Debug.LogMessage(ex, "Exception handling cooldown", Destination);
            }
        }

        /// <summary>
        /// Returns a string representation of the route request.
        /// </summary>
        /// <returns>A string describing the source and destination of the route request.</returns>
        public override string ToString()
        {
            return $"Route {Source?.Key ?? "No Source Device"}:{SourcePort?.Key ?? "auto"} to {Destination?.Key ?? "No Destination Device"}:{DestinationPort?.Key ?? "auto"}";
        }
    }
}