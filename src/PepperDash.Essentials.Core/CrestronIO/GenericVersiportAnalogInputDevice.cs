

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
    public class GenericVersiportAnalogInputDevice : EssentialsBridgeableDevice, IAnalogInput, IHasFeedback
    {
        private Versiport inputPort;

        /// <inheritdoc />
        public IntFeedback InputValueFeedback { get; private set; }

        /// <summary>
        /// Get the InputMinimumChangeFeedback
        /// </summary>
        /// <remarks>
        /// Updates when the analog input minimum change value changes
        /// </remarks>
        public IntFeedback InputMinimumChangeFeedback { get; private set; }

        /// <inheritdoc />
        public FeedbackCollection<Feedback> Feedbacks { get; private set; } = new FeedbackCollection<Feedback>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericVersiportAnalogInputDevice"/> class.
        /// </summary>
        /// <param name="key">key for the device</param>
        /// <param name="name">name for the device</param>
        /// <param name="postActivationFunc">function to call after activation</param>
        /// <param name="config">IO port configuration</param>
        public GenericVersiportAnalogInputDevice(string key, string name, Func<IOPortConfig, Versiport> postActivationFunc, IOPortConfig config) :
            base(key, name)
        {
            InputValueFeedback = new IntFeedback("inputValue", () => inputPort.AnalogIn);
            InputMinimumChangeFeedback = new IntFeedback("inputMinimumChange", () => inputPort.AnalogMinChange);

            AddPostActivationAction(() =>
            {
                inputPort = postActivationFunc(config);

                inputPort.Register();

                inputPort.SetVersiportConfiguration(eVersiportConfiguration.AnalogInput);
                inputPort.AnalogMinChange = (ushort)(config.MinimumChange > 0 ? config.MinimumChange : 655);
                if (config.DisablePullUpResistor)
                    inputPort.DisablePullUpResistor = true;

                inputPort.VersiportChange += InputPort_VersiportChange;

                this.LogDebug("Created GenericVersiportAnalogInputDevice on port {port}.  DisablePullUpResistor: {pullUpResistorDisabled}", config.PortNumber, inputPort.DisablePullUpResistor);
            });

        }

        /// <summary>
        /// Set minimum voltage change for device to update voltage changed method
        /// </summary>
        /// <param name="value">valid values range from 0 - 65535, representing the full 100% range of the processor voltage source.  Check processor documentation for details</param>        
        public void SetMinimumChange(ushort value)
        {
            inputPort.AnalogMinChange = value;
        }

        void InputPort_VersiportChange(Versiport port, VersiportEventArgs args)
        {
            this.LogDebug("Versiport change: {event}", args.Event);

            if (args.Event == eVersiportEvent.AnalogInChange)
                InputValueFeedback.FireUpdate();
            if (args.Event == eVersiportEvent.AnalogMinChangeChange)
                InputMinimumChangeFeedback.FireUpdate();
        }


        #region Bridge Linking

        /// <inheritdoc />
        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new IAnalogInputJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<IAnalogInputJoinMap>(joinMapSerialized);

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
                this.LogDebug("Linking to Trilist '{trilistId}'", trilist.ID.ToString("X"));

                // Link feedback for input state
                InputValueFeedback.LinkInputSig(trilist.UShortInput[joinMap.InputValue.JoinNumber]);
                InputMinimumChangeFeedback.LinkInputSig(trilist.UShortInput[joinMap.MinimumChange.JoinNumber]);
                trilist.SetUShortSigAction(joinMap.MinimumChange.JoinNumber, SetMinimumChange);

            }
            catch (Exception e)
            {
                this.LogError("Unable to link device {key}: {message}", Key, e.Message);
                this.LogDebug(e, "Stack Trace: ");
            }

            trilist.OnlineStatusChange += (d, args) =>
            {
                if (!args.DeviceOnLine) return;
                InputValueFeedback.FireUpdate();
                InputMinimumChangeFeedback.FireUpdate();
            };

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
                    Debug.LogError("GetVersiportAnalogInput: Processor does not support Versiports");
                    return null;
                }
                return Global.ControlSystem.VersiPorts[dc.PortNumber];
            }

            if (!(DeviceManager.GetDeviceForKey(dc.PortDeviceKey) is IIOPorts ioPortDevice))
            {
                Debug.LogError("GetVersiportAnalogInput: Device {key} is not a valid device", dc.PortDeviceKey);
                return null;
            }

            if (dc.PortNumber > ioPortDevice.NumberOfVersiPorts)
            {
                Debug.LogError("GetVersiportAnalogInput: Device {key} does not contain a port {port}", dc.PortDeviceKey, dc.PortNumber);
                return null;
            }
            if (!ioPortDevice.VersiPorts[dc.PortNumber].SupportsAnalogInput)
            {
                Debug.LogError("GetVersiportAnalogInput: Device {key} does not support AnalogInput on port {port}", dc.PortDeviceKey, dc.PortNumber);
                return null;
            }

            return ioPortDevice.VersiPorts[dc.PortNumber];
        }
    }


    /// <summary>
    /// Factory for creating GenericVersiportAnalogInputDevice devices
    /// </summary>
    public class GenericVersiportAnalogInputDeviceFactory : EssentialsDeviceFactory<GenericVersiportAnalogInputDevice>
    {
        /// <summary>
        /// Constructor for GenericVersiportAnalogInputDeviceFactory
        /// </summary>
        public GenericVersiportAnalogInputDeviceFactory()
        {
            TypeNames = new List<string>() { "versiportanaloginput" };
        }

        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogDebug("Factory Attempting to create new Generic Versiport Device");

            var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());

            if (props == null) return null;

            var portDevice = new GenericVersiportAnalogInputDevice(dc.Key, dc.Name, GenericVersiportAnalogInputDevice.GetVersiportDigitalInput, props);

            return portDevice;
        }
    }
}