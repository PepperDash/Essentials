using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.CrestronIO;
using Serilog.Events;


namespace PepperDash.Essentials.Core.Privacy;

/// <summary>
/// Used for applications where one or more microphones with momentary contact closure outputs are used to
/// toggle the privacy state of the room.  Privacy state feedback is represented 
/// </summary>
public class MicrophonePrivacyController : EssentialsDevice
{
    MicrophonePrivacyControllerConfig Config;

    bool initialized;

        /// <summary>
        /// Gets or sets whether LED control is enabled
        /// </summary>
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

    /// <summary>
    /// Gets or sets the list of digital inputs that are used to toggle the privacy state. Each input is expected to be momentary contact closure that triggers a change in the privacy state when activated. The controller will subscribe to the OutputChange event of each input's InputStateFeedback to monitor for changes in the input state and respond accordingly by toggling the privacy state and updating the LED indicators if enabled.
    /// </summary>
    public List<IDigitalInput> Inputs { get; private set; }

    /// <summary>
    /// Gets or sets the GenericRelayDevice that is used to indicate the privacy state with a red LED. When the privacy mode is active, the red LED will be turned on to provide a visual indication of the privacy state. The controller will manage the state of this relay based on changes in the privacy mode, ensuring that it accurately reflects whether the privacy mode is currently active or not.
    /// </summary>
    public GenericRelayDevice RedLedRelay { get; private set; }
    bool _redLedRelayState;

    /// <summary>
    /// Gets or sets the GenericRelayDevice that is used to indicate the privacy state with a green LED. When the privacy mode is inactive, the green LED will be turned on to provide a visual indication of the privacy state. The controller will manage the state of this relay based on changes in the privacy mode, ensuring that it accurately reflects whether the privacy mode is currently active or not.
    /// </summary>
    public GenericRelayDevice GreenLedRelay { get; private set; }
    bool _greenLedRelayState;

    /// <inheritdoc />
    public IPrivacy PrivacyDevice { get; private set; }

    /// <summary>
    /// Constructor for the MicrophonePrivacyController class, which initializes the controller with the specified key and configuration. The constructor sets up the necessary properties and collections for managing the digital inputs, LED relays, and privacy device. It also prepares the controller for activation by ensuring that all required components are properly initialized and ready to be used when the controller is activated in the system.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="config"></param>
    public MicrophonePrivacyController(string key, MicrophonePrivacyControllerConfig config) :
        base(key)
    {
        Config = config;

        Inputs = new List<IDigitalInput>();
    }

    protected override bool CustomActivate()
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
            Debug.LogMessage(LogEventLevel.Information, this, "Unable to add Green LED device");

        var redLed = DeviceManager.GetDeviceForKey(Config.RedLedRelay.DeviceKey) as GenericRelayDevice;

        if (redLed != null)
            RedLedRelay = redLed;
        else
            Debug.LogMessage(LogEventLevel.Information, this, "Unable to add Red LED device");

        AddPostActivationAction(() => {
            PrivacyDevice.PrivacyModeIsOnFeedback.OutputChange -= PrivacyModeIsOnFeedback_OutputChange;
            PrivacyDevice.PrivacyModeIsOnFeedback.OutputChange += PrivacyModeIsOnFeedback_OutputChange;
        });

        initialized = true;

        return base.CustomActivate();
    }

    #region Overrides of Device

    /// <inheritdoc />
    protected override void Initialize()
    {
        CheckPrivacyMode();
    }

    #endregion

    /// <inheritdoc />
    public void SetPrivacyDevice(IPrivacy privacyDevice)
    {
        PrivacyDevice = privacyDevice;
    }

    void PrivacyModeIsOnFeedback_OutputChange(object sender, EventArgs e)
    {
			Debug.LogMessage(LogEventLevel.Debug, this, "Privacy mode change: {0}", sender as BoolFeedback);
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

/// <summary>
/// Factory for creating MicrophonePrivacyController devices
/// </summary>
public class MicrophonePrivacyControllerFactory : EssentialsDeviceFactory<MicrophonePrivacyController>
{
    /// <summary>
    /// Constructor for the MicrophonePrivacyControllerFactory class, which initializes the factory and sets up the necessary type names for device creation. This factory is responsible for creating instances of the MicrophonePrivacyController device based on the provided configuration and device key when requested by the system.
    /// </summary>
    public MicrophonePrivacyControllerFactory()
    {
        TypeNames = new List<string>() { "microphoneprivacycontroller" };
    }


    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
        Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new MIcrophonePrivacyController Device");
        var props = Newtonsoft.Json.JsonConvert.DeserializeObject<Core.Privacy.MicrophonePrivacyControllerConfig>(dc.Properties.ToString());

        return new Core.Privacy.MicrophonePrivacyController(dc.Key, props);
    }
}