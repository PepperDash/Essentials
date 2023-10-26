using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Touchpanels
{
    /// <summary>
    /// Represents the configuration of a keybad buggon
    /// </summary>
    public class KeypadButton
    {
        public Dictionary<string, DeviceActionWrapper[]> EventTypes { get; set; }
        public KeypadButtonFeedback Feedback { get; set; }

        public KeypadButton()
        {
            EventTypes = new Dictionary<string, DeviceActionWrapper[]>();
            Feedback   = new KeypadButtonFeedback();
        }
    }
}