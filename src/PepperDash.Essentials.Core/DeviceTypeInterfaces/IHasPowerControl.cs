namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines the ability to power a device on and off
    /// </summary>
    public interface IHasPowerControl
    {
        void PowerOn();
        void PowerOff();
        void PowerToggle();
    }
}