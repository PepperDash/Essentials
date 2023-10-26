using System;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// For rooms with routing
    /// </summary>
    public interface IRunRouteAction
    {
        void RunRouteAction(string routeKey, string sourceListKey);

        void RunRouteAction(string routeKey, string sourceListKey, Action successCallback);

    }
}