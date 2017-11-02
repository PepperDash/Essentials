using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Crestron_IO;


namespace PepperDash.Essentials.Devices.Common.Microphones
{
    /// <summary>
    /// Used for applications where one or more microphones with momentary contact closure outputs are used to
    /// toggle the privacy state of the room.  Privacy state feedback is represented 
    /// </summary>
    public class MicrophonePrivacyController
    {
        public bool EnableLeds
        {
            get
            {
                return _enableLeds;
            }
            set
            {
                if (value)
                    SetLedRelayStates();
                else
                    TurnOffAllLeds();
                _enableLeds = value;
            }
        }
        bool _enableLeds;

        public List<IDigitalInput> Inputs { get; private set; }

        public GenericRelayDevice RedLedRelay { get; private set; }
        bool _redLedRelayState;

        public GenericRelayDevice GreenLedRelay { get; private set; }
        bool _greenLedRelayState;

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

            if (privacyState)
                TurnOnRedLeds();
            else
                TurnOnGreenLeds();

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

        void TurnOnRedLeds()
        {
            _greenLedRelayState = false;
            _redLedRelayState = true;
            SetLedRelayStates();
        }

        void TurnOnGreenLeds()
        {
            _redLedRelayState = false;
            _greenLedRelayState = true;
            SetLedRelayStates();
        }

        /// <summary>
        /// If enabled, sets the actual state of the relays
        /// </summary>
        void SetLedRelayStates()
        {
            if (_enableLeds)
            {
                if (_redLedRelayState)
                    RedLedRelay.CloseRelay();
                else
                    RedLedRelay.OpenRelay();

                if (_greenLedRelayState)
                    GreenLedRelay.CloseRelay();
                else
                    GreenLedRelay.OpenRelay();
            }
            else
                TurnOffAllLeds();
        }

        /// <summary>
        /// Turns off all LEDs
        /// </summary>
        void TurnOffAllLeds()
        {
            GreenLedRelay.OpenRelay();
            RedLedRelay.OpenRelay();
        }
    }
}