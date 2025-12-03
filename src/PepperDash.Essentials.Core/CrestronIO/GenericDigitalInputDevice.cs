

using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;


namespace PepperDash.Essentials.Core.CrestronIO
{

    /// <summary>
    /// Represents a GenericDigitalInputDevice
    /// </summary>
    /// [Description("Wrapper class for Digital Input")]
    public class GenericDigitalInputDevice : EssentialsBridgeableDevice, IDigitalInput, IHasFeedback
    {
        private DigitalInput inputPort;

        /// <summary>
        /// Gets or sets the InputStateFeedback
        /// </summary>
        public BoolFeedback InputStateFeedback { get; private set; }

        /// <inheritdoc />
        public FeedbackCollection<Feedback> Feedbacks { get; private set; } = new FeedbackCollection<Feedback>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDigitalInputDevice"/> class.
        /// </summary>
        /// <param name="key">key for device</param>
        /// <param name="name">name for device</param>
        /// <param name="postActivationFunc">function to call after activation. Should return the DigitalInput</param>
        /// <param name="config">config for device</param>
        public GenericDigitalInputDevice(string key, string name, Func<IOPortConfig, DigitalInput> postActivationFunc,
            IOPortConfig config)
            : base(key, name)
        {
            InputStateFeedback = new BoolFeedback("inputState", () => inputPort.State);

            AddPostActivationAction(() =>
            {
                inputPort = postActivationFunc(config);

                inputPort.Register();

                inputPort.StateChange += InputPort_StateChange;
            });
        }

        #region Events

        void InputPort_StateChange(DigitalInput digitalInput, DigitalInputEventArgs args)
        {
            InputStateFeedback.FireUpdate();
        }

        #endregion

        #region PreActivate

        private static DigitalInput GetDigitalInput(IOPortConfig dc)
        {

            if (dc.PortDeviceKey.Equals("processor"))
            {
                if (!Global.ControlSystem.SupportsDigitalInput)
                {
                    Debug.LogError("GetDigitalInput: Processor does not support Digital Inputs");
                    return null;
                }

                return Global.ControlSystem.DigitalInputPorts[dc.PortNumber];
            }

            if (!(DeviceManager.GetDeviceForKey(dc.PortDeviceKey) is IDigitalInputPorts ioPortDevice))
            {
                Debug.LogError("GetDigitalInput: Device {key} is not a valid device", dc.PortDeviceKey);
                return null;
            }

            if (dc.PortNumber > ioPortDevice.NumberOfDigitalInputPorts)
            {
                Debug.LogError("GetDigitalInput: Device {key} does not contain a digital input port {port}", dc.PortDeviceKey, dc.PortNumber);
                return null;
            }

            return ioPortDevice.DigitalInputPorts[dc.PortNumber];
        }

        #endregion

        #region Bridge Linking

        /// <summary>
        /// LinkToApi method
        /// </summary>
        /// <inheritdoc />
        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new IDigitalInputJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<IDigitalInputJoinMap>(joinMapSerialized);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                this.LogWarning("Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            try
            {
                this.LogDebug("Linking to Trilist '{0}'", trilist.ID.ToString("X"));

                // Link feedback for input state
                InputStateFeedback.LinkInputSig(trilist.BooleanInput[joinMap.InputState.JoinNumber]);
            }
            catch (Exception e)
            {
                this.LogError("Unable to link device {key}.  {message}", Key, e.Message);
                this.LogDebug(e, "Stack Trace: ");
            }
        }

        #endregion

        #region Factory

        /// <summary>
        /// Factory for creating GenericDigitalInputDevice devices
        /// </summary>
        public class GenericDigitalInputDeviceFactory : EssentialsDeviceFactory<GenericDigitalInputDevice>
        {
            /// <summary>
            /// Constructor for GenericDigitalInputDeviceFactory
            /// </summary>
            public GenericDigitalInputDeviceFactory()
            {
                TypeNames = new List<string>() { "digitalinput" };
            }

            /// <inheritdoc />
            public override EssentialsDevice BuildDevice(DeviceConfig dc)
            {
                Debug.LogDebug("Factory Attempting to create new Generic Digital Input Device");

                var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());

                if (props == null) return null;

                var portDevice = new GenericDigitalInputDevice(dc.Key, dc.Name, GetDigitalInput, props);

                return portDevice;
            }
        }

        #endregion

    }


}