using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;


namespace PepperDash.Essentials.Core.Privacy
{
    /// <summary>
    /// Used for applications where one or more microphones with momentary contact closure outputs are used to
    /// toggle the privacy state of the room.  Privacy state feedback is represented 
    /// </summary>
    public class MicrophonePrivacyController : EssentialsDevice
    {
        MicrophonePrivacyControllerConfig Config;

        bool initialized;

        public bool EnableLeds
        {
            get
            {
                return _enableLeds;
            }
            set
            {
                _enableLeds = value;

                if (initialized)
                {
                    if (value)
                    {
                        CheckPrivacyMode();
                        SetLedStates();
                    }
                    else
                        TurnOffAllLeds();
                }
            }
        }
        bool _enableLeds;

        public List<IDigitalInput> Inputs { get; private set; }

        public GenericRelayDevice RedLedRelay { get; private set; }
        bool _redLedRelayState;

        public GenericRelayDevice GreenLedRelay { get; private set; }
        bool _greenLedRelayState;

        public IPrivacy PrivacyDevice { get; private set; }

        public MicrophonePrivacyController(string key, MicrophonePrivacyControllerConfig config) :
            base(key)
        {
            Config = config;

            Inputs = new List<IDigitalInput>();
        }

        public override bool CustomActivate()
        {
            foreach (var i in Config.Inputs)
            {
                var input = DeviceManager.GetDeviceForKey(i.DeviceKey) as IDigitalInput;

                if(input != null)
                    AddInput(input);
            }

            var greenLed = DeviceManager.GetDeviceForKey(Config.GreenLedRelay.DeviceKey) as GenericRelayDevice;

            if (greenLed != null)
                GreenLedRelay = greenLed;
            else
                Debug.Console(0, this, "Unable to add Green LED device");

            var redLed = DeviceManager.GetDeviceForKey(Config.RedLedRelay.DeviceKey) as GenericRelayDevice;

            if (redLed != null)
                RedLedRelay = redLed;
            else
                Debug.Console(0, this, "Unable to add Red LED device");

            AddPostActivationAction(() => {
                CheckPrivacyMode();
                PrivacyDevice.PrivacyModeIsOnFeedback.OutputChange -= PrivacyModeIsOnFeedback_OutputChange;
                PrivacyDevice.PrivacyModeIsOnFeedback.OutputChange += PrivacyModeIsOnFeedback_OutputChange;
            });

            initialized = true;

            return base.CustomActivate();
        }

        public void SetPrivacyDevice(IPrivacy privacyDevice)
        {
            PrivacyDevice = privacyDevice;
        }

        void PrivacyModeIsOnFeedback_OutputChange(object sender, EventArgs e)
        {
			Debug.Console(1, this, "Privacy mode change: {0}", sender as BoolFeedback);
            CheckPrivacyMode();
        }

        void CheckPrivacyMode()
        {
            if (PrivacyDevice != null)
            {
                var privacyState = PrivacyDevice.PrivacyModeIsOnFeedback.BoolValue;

                if (privacyState)
                    TurnOnRedLeds();
                else
                    TurnOnGreenLeds();
            }
        }

        void AddInput(IDigitalInput input)
        {
            Inputs.Add(input);

            input.InputStateFeedback.OutputChange += InputStateFeedback_OutputChange;
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
            if ((sender as BoolFeedback).BoolValue == true)
                TogglePrivacyMute();
        }

        /// <summary>
        /// Toggles the state of the privacy mute
        /// </summary>
        public void TogglePrivacyMute()
        {
            PrivacyDevice.PrivacyModeToggle();
        }

        void TurnOnRedLeds()
        {
            _greenLedRelayState = false;
            _redLedRelayState = true;
            SetLedStates();
        }

        void TurnOnGreenLeds()
        {
            _redLedRelayState = false;
            _greenLedRelayState = true;
            SetLedStates();
        }

        /// <summary>
        /// If enabled, sets the actual state of the relays
        /// </summary>
        void SetLedStates()
        {
            if (_enableLeds)
            {
                SetRelayStates();
            }
            else
                TurnOffAllLeds();
        }

        /// <summary>
        /// Turns off all LEDs
        /// </summary>
        void TurnOffAllLeds()
        {
            _redLedRelayState = false;
            _greenLedRelayState = false;

            SetRelayStates();
        }

        void SetRelayStates()
        {
            if (RedLedRelay != null)
            {
                if (_redLedRelayState)
                    RedLedRelay.CloseRelay();
                else
                    RedLedRelay.OpenRelay();
            }

            if(GreenLedRelay != null)
            {
                if (_greenLedRelayState)
                    GreenLedRelay.CloseRelay();
                else
                    GreenLedRelay.OpenRelay();
            }
        }
    }

    public class MicrophonePrivacyControllerFactory : EssentialsDeviceFactory<MicrophonePrivacyController>
    {
        public MicrophonePrivacyControllerFactory()
        {
            TypeNames = new List<string>() { "microphoneprivacycontroller" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new MIcrophonePrivacyController Device");
            var props = Newtonsoft.Json.JsonConvert.DeserializeObject<Core.Privacy.MicrophonePrivacyControllerConfig>(dc.Properties.ToString());

            return new Core.Privacy.MicrophonePrivacyController(dc.Key, props);
        }
    }

}