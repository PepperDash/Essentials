namespace PepperDash.Essentials.Core.Interfaces
{
    // TODO: Is-A relationship; consider refactoring
    /// <summary>
    /// Describes an output capable of switching on and off
    /// </summary>
    public interface ISwitchedOutput
    {
        BoolFeedback OutputIsOnFeedback {get;}

        void On();
        void Off();
    }
}