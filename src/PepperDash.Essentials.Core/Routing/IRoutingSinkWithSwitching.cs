namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Endpoint device like a display, that selects inputs
    /// </summary>
    public interface IRoutingSinkWithSwitching : IRoutingSink
    {
        //void ClearRoute();
        void ExecuteSwitch(object inputSelector);
    }
}