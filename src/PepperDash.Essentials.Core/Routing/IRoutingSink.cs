using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// For fixed-source endpoint devices
    /// </summary>
    public interface IRoutingSink : IRoutingInputs, IHasCurrentSourceInfoChange
    {

    }

    /*/// <summary>
    /// For fixed-source endpoint devices
    /// </summary>
    public interface IRoutingSink<TSelector> : IRoutingInputs<TSelector>, IHasCurrentSourceInfoChange
    {
        void UpdateRouteRequest<TOutputSelector>(RouteRequest<TSelector, TOutputSelector> request);

        RouteRequest<TSelector, TOutputSelector> GetRouteRequest<TOutputSelector>();
    }*/
}