

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
    /// Represents a generic digital input deviced tied to a versiport
    /// </summary>
    public class GenericVersiportDigitalInputDevice : EssentialsBridgeableDevice, IDigitalInput, IPartitionStateProvider, IHasFeedback
    {
        private Versiport inputPort;
        private bool invertState;

        /// <summary>
        /// Gets or sets the InputStateFeedback
        /// </summary>
        public BoolFeedback InputStateFeedback { get; private set; }

        /// <inheritdoc />
        public FeedbackCollection<Feedback> Feedbacks { get; private set; } = new FeedbackCollection<Feedback>();

        /// <summary>
        /// Gets or sets the PartitionPresentFeedback
        /// </summary>
        public BoolFeedback PartitionPresentFeedback { get; }

        /// <summary>
        /// Get partition state
        /// </summary>
        public bool PartitionPresent => !inputPort.DigitalIn;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericVersiportDigitalInputDevice"/> class.
        /// </summary>
        /// <param name="key">key for device</param>
        /// <param name="name">name for device</param>
        /// <param name="postActivationFunc">function to call after activation. Should return the Versiport</param>
        /// <param name="config">config for device</param>
        public GenericVersiportDigitalInputDevice(string key, string name, Func<IOPortConfig, Versiport> postActivationFunc, IOPortConfig config) :
            base(key, name)
        {
            invertState = !string.IsNullOrEmpty(config.CircuitType) && 
                         config.CircuitType.Equals("NC", StringComparison.OrdinalIgnoreCase);
            
            InputStateFeedback = new BoolFeedback("inputState", () => invertState ? !inputPort.DigitalIn : inputPort.DigitalIn);
            PartitionPresentFeedback = new BoolFeedback("partitionPresent", () => !inputPort.DigitalIn);

            AddPostActivationAction(() =>
            {
                inputPort = postActivationFunc(config);

                inputPort.Register();

                inputPort.SetVersiportConfiguration(eVersiportConfiguration.DigitalInput);
                if (config.DisablePullUpResistor)
                    inputPort.DisablePullUpResistor = true;

                inputPort.VersiportChange += InputPort_VersiportChange;

                InputStateFeedback.FireUpdate();
                PartitionPresentFeedback.FireUpdate();

                this.LogDebug("Created GenericVersiportDigitalInputDevice for port {port}.  DisablePullUpResistor: {pullUpResistorDisable}", config.PortNumber, inputPort.DisablePullUpResistor);

            });

            Feedbacks.Add(InputStateFeedback);
            Feedbacks.Add(PartitionPresentFeedback);
        }

        void InputPort_VersiportChange(Versiport port, VersiportEventArgs args)
        {
            this.LogDebug("Versiport change: {0}", args.Event);

            if (args.Event == eVersiportEvent.DigitalInChange)
            {
                InputStateFeedback.FireUpdate();
                PartitionPresentFeedback.FireUpdate();
            }
        }


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
                this.LogError("Unable to link device {key}.  Input is null. {message}", Key, e.Message);
                this.LogDebug(e, "Stack Trace: ");
            }
        }

        #endregion


        /// <summary>
        /// GetVersiportDigitalInput method
        /// </summary>
        public static Versiport GetVersiportDigitalInput(IOPortConfig dc)
        {
            if (dc.PortDeviceKey.Equals("processor"))
            {
                if (!Global.ControlSystem.SupportsVersiport)
                {
                    Debug.LogError("GetVersiportDigitalInput: Processor does not support Versiports");
                    return null;
                }
                return Global.ControlSystem.VersiPorts[dc.PortNumber];
            }

            if (!(DeviceManager.GetDeviceForKey(dc.PortDeviceKey) is IIOPorts ioPortDevice))
            {
                Debug.LogError("GetVersiportDigitalInput: Device {key} is not a valid device", dc.PortDeviceKey);
                return null;
            }

            if (dc.PortNumber > ioPortDevice.NumberOfVersiPorts)
            {
                Debug.LogError("GetVersiportDigitalInput: Device {key} does not contain versiport {port}", dc.PortDeviceKey, dc.PortNumber);
                return null;
            }

            return ioPortDevice.VersiPorts[dc.PortNumber];
        }
    }


    /// <summary>
    /// Factory class for GenericVersiportDigitalInputDevice
    /// </summary>
    public class GenericVersiportDigitalInputDeviceFactory : EssentialsDeviceFactory<GenericVersiportDigitalInputDevice>
    {
        /// <summary>
        /// Constructor for GenericVersiportDigitalInputDeviceFactory
        /// </summary>
        public GenericVersiportDigitalInputDeviceFactory()
        {
            TypeNames = new List<string>() { "versiportinput" };
        }

        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogDebug("Factory Attempting to create new Generic Versiport Device");

            var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());

            if (props == null) return null;

            var portDevice = new GenericVersiportDigitalInputDevice(dc.Key, dc.Name, GenericVersiportDigitalInputDevice.GetVersiportDigitalInput, props);

            return portDevice;
        }
    }
}