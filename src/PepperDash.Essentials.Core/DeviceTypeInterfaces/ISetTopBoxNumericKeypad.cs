namespace PepperDash.Essentials.Core
{
    public interface ISetTopBoxNumericKeypad : INumericKeypad
    {
        void Dash(bool pressRelease);
        void KeypadEnter(bool pressRelease);
    }
}