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
        private readonly Action<IRoutingInputs, string, bool> action;
        private readonly IRoutingInputs destination;
        private readonly string inputPortKey;
        private readonly bool clearRoute;

        public ReleaseRouteQueueItem(Action<IRoutingInputs, string, bool> action, IRoutingInputs destination, string inputPortKey, bool clearRoute)
        {
            this.action = action;
            this.destination = destination;
            this.inputPortKey = inputPortKey;
            this.clearRoute = clearRoute;
        }

        public void Dispatch()
        {
            Debug.LogMessage(LogEventLevel.Information, "Dispatching release route request for {destination}:{inputPortKey}", null, destination?.Key ?? "no destination", string.IsNullOrEmpty(inputPortKey) ? "auto" : inputPortKey);
            action(destination, inputPortKey, clearRoute);
        }
    }
}
