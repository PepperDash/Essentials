using PepperDash.Core;
using PepperDash.Essentials.Core.Queues;
using System;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Represents a RouteRequestQueueItem
    /// </summary>
    public class RouteRequestQueueItem : IQueueMessage
    {
        /// <summary>
        /// The action to perform for the route request.
        /// </summary>
        private readonly Action<RouteRequest> action;
        /// <summary>
        /// The route request data.
        /// </summary>
        private readonly RouteRequest routeRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteRequestQueueItem"/> class.
        /// </summary>
        /// <param name="routeAction">The action to perform.</param>
        /// <param name="request">The route request data.</param>
        public RouteRequestQueueItem(Action<RouteRequest> routeAction, RouteRequest request)
        {
            action = routeAction;
            routeRequest = request;
        }

        /// <summary>
        /// Dispatches the route request action.
        /// </summary>
        public void Dispatch()
        {
            Debug.LogMessage(LogEventLevel.Information, "Dispatching route request {routeRequest}", null, routeRequest);
            action(routeRequest);
        }
    }

    /// <summary>
    /// Represents a ReleaseRouteQueueItem
    /// </summary>
    public class ReleaseRouteQueueItem : IQueueMessage
    {
        /// <summary>
        /// The action to perform for releasing the route.
        /// </summary>
        private readonly Action<IRoutingInputs, string, bool> action;
        /// <summary>
        /// The destination device whose route is being released.
        /// </summary>
        private readonly IRoutingInputs destination;
        /// <summary>
        /// The specific input port key on the destination to release, or null/empty for any/default.
        /// </summary>
        private readonly string inputPortKey;
        /// <summary>
        /// Indicates whether to clear the route (send null) or just release the usage tracking.
        /// </summary>
        private readonly bool clearRoute;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseRouteQueueItem"/> class.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="destination">The destination device.</param>
        /// <param name="inputPortKey">The input port key.</param>
        /// <param name="clearRoute">True to clear the route, false to just release.</param>
        public ReleaseRouteQueueItem(Action<IRoutingInputs, string, bool> action, IRoutingInputs destination, string inputPortKey, bool clearRoute)
        {
            this.action = action;
            this.destination = destination;
            this.inputPortKey = inputPortKey;
            this.clearRoute = clearRoute;
        }

        /// <summary>
        /// Dispatch method
        /// </summary>
        public void Dispatch()
        {
            Debug.LogMessage(LogEventLevel.Information, "Dispatching release route request for {destination}:{inputPortKey}", null, destination?.Key ?? "no destination", string.IsNullOrEmpty(inputPortKey) ? "auto" : inputPortKey);
            action(destination, inputPortKey, clearRoute);
        }
    }
}
