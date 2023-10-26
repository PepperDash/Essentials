namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// For rooms that have default presentation and calling routes
    /// </summary>
    public interface IRunDefaultCallRoute : IRunDefaultPresentRoute
    {
        bool RunDefaultCallRoute();
    }
}