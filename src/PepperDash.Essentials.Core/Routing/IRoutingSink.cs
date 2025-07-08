namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// For fixed-source endpoint devices
    /// </summary>
    public interface IRoutingSink : IRoutingInputs, IHasCurrentSourceInfoChange
    {        
    }

    /// <summary>
    /// For fixed-source endpoint devices with an input port
    /// </summary>
    public interface IRoutingSinkWithInputPort :IRoutingSink
    {
        /// <summary>
        /// Gets the current input port for this routing sink.
        /// </summary>
        RoutingInputPort CurrentInputPort { get; }
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