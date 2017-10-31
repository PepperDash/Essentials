using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core.Crestron_IO;

namespace PepperDash.Essentials.Core.Microphone_Privacy
{
    public class MicrophonePrivacyController
    {
        public List<IDigitalInput> Inputs { get; private set; }

        public GenericRelayDevice RedLedRelay { get; private set; }

        public GenericRelayDevice GreenLedRelay { get; private set; }

        public IPrivacy PrivacyDevice { get; private set; }



        public MicrophonePrivacyController(IPrivacy privacyDevice)
        {
            PrivacyDevice = privacyDevice;

            PrivacyDevice.PrivacyModeIsOnFeedback.OutputChange += new EventHandler<EventArgs>(PrivacyModeIsOnFeedback_OutputChange);

            Inputs = new List<IDigitalInput>();
        }

        void PrivacyModeIsOnFeedback_OutputChange(object sender, EventArgs e)
        {
            var privacyState = (sender as IPrivacy).PrivacyModeIsOnFeedback.BoolValue;

            if(privacyState)

        }

        void AddInput(IDigitalInput input)
        {
            Inputs.Add(input);

            input.InputStateFeedback.OutputChange += new EventHandler<EventArgs>(InputStateFeedback_OutputChange);
        }

        void RemoveInput(IDigitalInput input)
        {
            var tempInput = Inputs.FirstOrDefault(i => i.Equals(input));

            if (tempInput != null)
                tempInput.InputStateFeedback.OutputChange -= InputStateFeedback_OutputChange;

            Inputs.Remove(input);
        }

        void SetRedLedRelay(GenericRelayDevice relay)
        {
            RedLedRelay = relay;
        }

        void SetGreenLedRelay(GenericRelayDevice relay)
        {
            GreenLedRelay = relay;
        }

        /// <summary>
        /// Check the state of the input change and handle accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InputStateFeedback_OutputChange(object sender, EventArgs e)
        {
            if ((sender as IDigitalInput).InputStateFeedback.BoolValue)
                TogglePrivacyMute();
        }

        /// <summary>
        /// Toggles the state of the privacy mute
        /// </summary>
        void TogglePrivacyMute()
        {
            PrivacyDevice.PrivacyModeToggle();
        }
    }
}