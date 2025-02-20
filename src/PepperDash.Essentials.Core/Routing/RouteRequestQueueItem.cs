using PepperDash.Core;
using PepperDash.Essentials.Core.Queues;
using System;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Routing
{
    public class RouteRequestQueueItem : IQueueMessage
    {
        private readonly Action<RouteRequest> action;
        private readonly RouteRequest routeRequest;

        public RouteRequestQueueItem(Action<RouteRequest> routeAction, RouteRequest request)
        {
            action = routeAction;
            routeRequest = request;
        }

        public void Dispatch()
        {
            Debug.LogMessage(LogEventLevel.Information, "Dispatching route request {routeRequest}", null, routeRequest);
            action(routeRequest);
        }
    }

    public class ReleaseRouteQueueItem : IQueueMessage
    {
        private readonly Action<IRoutingInputs, string> action;
        private readonly IRoutingInputs destination;
        private readonly string inputPortKey;

        public ReleaseRouteQueueItem(Action<IRoutingInputs, string> action, IRoutingInputs destination, string inputPortKey)
        {
            this.action = action;
            this.destination = destination;
            this.inputPortKey = inputPortKey;
        }

        public void Dispatch()
        {
            Debug.LogMessage(LogEventLevel.Information, "Dispatching release route request for {destination}:{inputPortKey}", null, destination?.Key ?? "no destination", string.IsNullOrEmpty(inputPortKey) ? "auto" : inputPortKey);
            action(destination, inputPortKey);
        }
    }
}
