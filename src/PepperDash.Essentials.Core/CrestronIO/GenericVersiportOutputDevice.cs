

using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Represents a generic digital input deviced tied to a versiport
    /// </summary>
    public class GenericVersiportDigitalOutputDevice : EssentialsBridgeableDevice, IDigitalOutput, IHasFeedback
    {
        private Versiport outputPort;

        /// <summary>
        /// Gets or sets the OutputStateFeedback
        /// </summary>
        public BoolFeedback OutputStateFeedback { get; private set; }

        /// <inheritdoc />
        public FeedbackCollection<Feedback> Feedbacks { get; private set; } = new FeedbackCollection<Feedback>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericVersiportDigitalOutputDevice"/> class.
        /// </summary>
        public GenericVersiportDigitalOutputDevice(string key, string name, Func<IOPortConfig, Versiport> postActivationFunc, IOPortConfig config) :
            base(key, name)
        {
            OutputStateFeedback = new BoolFeedback("outputState", () => outputPort.DigitalOut);

            AddPostActivationAction(() =>
            {
                outputPort = postActivationFunc(config);

                outputPort.Register();


                if (!outputPort.SupportsDigitalOutput)
                {
                    this.LogError("Device does not support configuration as a Digital Output");
                    return;
                }

                outputPort.SetVersiportConfiguration(eVersiportConfiguration.DigitalOutput);


                outputPort.VersiportChange += OutputPort_VersiportChange;

            });
        }

        void OutputPort_VersiportChange(Versiport port, VersiportEventArgs args)
        {
            this.LogDebug("Versiport change: {event}", args.Event);

            if (args.Event == eVersiportEvent.DigitalOutChange)
                OutputStateFeedback.FireUpdate();
        }

        /// <summary>
        /// Set value of the versiport digital output
        /// </summary>
        /// <param name="state">value to set the output to</param>        
        public void SetOutput(bool state)
        {
            if (!outputPort.SupportsDigitalOutput)
            {
                this.LogError("Versiport does not support Digital Output Mode");
                return;
            }

            outputPort.DigitalOut = state;
        }

        #region Bridge Linking

        /// <summary>
        /// LinkToApi method
        /// </summary>
        /// <inheritdoc />
        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new IDigitalOutputJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<IDigitalOutputJoinMap>(joinMapSerialized);

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
                OutputStateFeedback.LinkInputSig(trilist.BooleanInput[joinMap.OutputState.JoinNumber]);
                trilist.SetBoolSigAction(joinMap.OutputState.JoinNumber, SetOutput);
            }
            catch (Exception e)
            {
                this.LogError("Unable to link device: {message}", e.Message);
                this.LogDebug(e, "Stack Trace: ");
            }
        }

        #endregion


        /// <summary>
        /// GetVersiportDigitalOutput method
        /// </summary>
        public static Versiport GetVersiportDigitalOutput(IOPortConfig dc)
        {
            if (dc.PortDeviceKey.Equals("processor"))
            {
                if (!Global.ControlSystem.SupportsVersiport)
                {
                    Debug.LogError("GetVersiportDigitalOutput: Processor does not support Versiports");
                    return null;
                }
                return Global.ControlSystem.VersiPorts[dc.PortNumber];
            }

            if (!(DeviceManager.GetDeviceForKey(dc.PortDeviceKey) is IIOPorts ioPortDevice))
            {
                Debug.LogError("GetVersiportDigitalOutput: Device {key} is not a valid device", dc.PortDeviceKey);
                return null;
            }

            if (dc.PortNumber > ioPortDevice.NumberOfVersiPorts)
            {
                Debug.LogMessage(LogEventLevel.Information, "GetVersiportDigitalOutput: Device {0} does not contain a port {1}", dc.PortDeviceKey, dc.PortNumber);
                return null;
            }
            return ioPortDevice.VersiPorts[dc.PortNumber];
        }
    }


    /// <summary>
    /// Represents a GenericVersiportDigitalOutputDeviceFactory
    /// </summary>
    public class GenericVersiportDigitalOutputDeviceFactory : EssentialsDeviceFactory<GenericVersiportDigitalInputDevice>
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="GenericVersiportDigitalOutputDeviceFactory"/> class.
        /// </summary>
        public GenericVersiportDigitalOutputDeviceFactory()
        {
            TypeNames = new List<string>() { "versiportoutput" };
        }

        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogDebug("Factory Attempting to create new Generic Versiport Device");

            var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());

            if (props == null) return null;

            var portDevice = new GenericVersiportDigitalOutputDevice(dc.Key, dc.Name, GenericVersiportDigitalOutputDevice.GetVersiportDigitalOutput, props);

            return portDevice;
        }
    }
}