using System;

namespace PepperDash.Essentials.Core.Touchpanels.Keyboards
{
    /// <summary>
    /// 
    /// </summary>
    public class KeyboardControllerPressEventArgs : EventArgs
    {
        public string Text { get; private set; }
        public KeyboardSpecialKey SpecialKey { get; private set; }

        public KeyboardControllerPressEventArgs(string text)
        {
            Text = text;
        }

        public KeyboardControllerPressEventArgs(KeyboardSpecialKey key)
        {
            SpecialKey = key;
        }
    }
}