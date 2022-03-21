namespace PepperDash.Essentials.Core.Interfaces
{
    public interface ISetTopBoxNumericKeypad : INumericKeypad
    {
        void Dash(bool pressRelease);
        void KeypadEnter(bool pressRelease);
    }
}