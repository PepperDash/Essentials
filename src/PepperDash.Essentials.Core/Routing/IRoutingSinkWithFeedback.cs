namespace PepperDash.Essentials.Core
{
    
    /// <summary>
    /// Defines the contract for IRoutingSinkWithFeedback
    /// </summary>
    public interface IRoutingSinkWithFeedback : IRoutingSinkWithSwitching
    {
        
    }

/*    /// <summary>
    /// For fixed-source endpoint devices
    /// </summary>
    public interface IRoutingSinkWithFeedback<TSelector> : IRoutingSinkWithSwitching<TSelector>
    {
        RouteSwitchDescriptor CurrentRoute { get; }

        event EventHandler InputChanged;
    }*/
}