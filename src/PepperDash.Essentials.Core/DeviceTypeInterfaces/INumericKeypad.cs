using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface INumericKeypad:IKeyed
    {
        void Digit0(bool pressRelease);
        void Digit1(bool pressRelease);
        void Digit2(bool pressRelease);
        void Digit3(bool pressRelease);
        void Digit4(bool pressRelease);
        void Digit5(bool pressRelease);
        void Digit6(bool pressRelease);
        void Digit7(bool pressRelease);
        void Digit8(bool pressRelease);
        void Digit9(bool pressRelease);

        /// <summary>
        /// Used to hide/show the button and/or text on the left-hand keypad button
        /// </summary>
        bool HasKeypadAccessoryButton1 { get; }
        string KeypadAccessoryButton1Label { get; }
        void KeypadAccessoryButton1(bool pressRelease);

        bool HasKeypadAccessoryButton2 { get; }
        string KeypadAccessoryButton2Label { get; }
        void KeypadAccessoryButton2(bool pressRelease);
    }
}