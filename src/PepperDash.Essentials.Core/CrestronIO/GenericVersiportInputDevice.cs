

using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges;


using Newtonsoft.Json;
using Serilog.Events;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Represents a generic digital input deviced tied to a versiport
    /// </summary>
    public class GenericVersiportDigitalInputDevice : EssentialsBridgeableDevice, IDigitalInput, IPartitionStateProvider
    {
        public Versiport InputPort { get; private set; }

        public BoolFeedback InputStateFeedback { get; private set; }

        Func<bool> InputStateFeedbackFunc
        {
            get
            {
                return () => InputPort.DigitalIn;
            }
        }

        /// <summary>
        /// Gets or sets the PartitionPresentFeedback
        /// </summary>
        public BoolFeedback PartitionPresentFeedback { get; }

        public bool PartitionPresent => !InputStateFeedbackFunc();

        public GenericVersiportDigitalInputDevice(string key, string name, Func<IOPortConfig, Versiport> postActivationFunc, IOPortConfig config) :
            base(key, name)
        {
            InputStateFeedback = new BoolFeedback(InputStateFeedbackFunc);
            PartitionPresentFeedback = new BoolFeedback(() => !InputStateFeedbackFunc());

            AddPostActivationAction(() =>
            {
                InputPort = postActivationFunc(config);

                InputPort.Register();

                InputPort.SetVersiportConfiguration(eVersiportConfiguration.DigitalInput);
                if (config.DisablePullUpResistor)
                    InputPort.DisablePullUpResistor = true;

                InputPort.VersiportChange += InputPort_VersiportChange;

                InputStateFeedback.FireUpdate();
                PartitionPresentFeedback.FireUpdate();

                Debug.LogMessage(LogEventLevel.Debug, this, "Created GenericVersiportDigitalInputDevice on port '{0}'.  DisablePullUpResistor: '{1}'", config.PortNumber, InputPort.DisablePullUpResistor);

            });

        }

        void InputPort_VersiportChange(Versiport port, VersiportEventArgs args)
        {
			Debug.LogMessage(LogEventLevel.Debug, this, "Versiport change: {0}", args.Event);

            if(args.Event == eVersiportEvent.DigitalInChange)
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
                Debug.LogMessage(LogEventLevel.Information, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            try
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

                // Link feedback for input state
                InputStateFeedback.LinkInputSig(trilist.BooleanInput[joinMap.InputState.JoinNumber]);
            }
            catch (Exception e)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Unable to link device '{0}'.  Input is null", Key);
                Debug.LogMessage(LogEventLevel.Debug, this, "Error: {0}", e);
            }
        }

        #endregion


        /// <summary>
        /// GetVersiportDigitalInput method
        /// </summary>
        public static Versiport GetVersiportDigitalInput(IOPortConfig dc)
        {
         
            IIOPorts ioPortDevice;

            if (dc.PortDeviceKey.Equals("processor"))
            {
                if (!Global.ControlSystem.SupportsVersiport)
                {
                    Debug.LogMessage(LogEventLevel.Information, "GetVersiportDigitalInput: Processor does not support Versiports");
                    return null;
                }
                ioPortDevice = Global.ControlSystem;
            }
            else
            {
                var ioPortDev = DeviceManager.GetDeviceForKey(dc.PortDeviceKey) as IIOPorts;
                if (ioPortDev == null)
                {
                    Debug.LogMessage(LogEventLevel.Information, "GetVersiportDigitalInput: Device {0} is not a valid device", dc.PortDeviceKey);
                    return null;
                }
                ioPortDevice = ioPortDev;
            }
            if (ioPortDevice == null)
            {
                Debug.LogMessage(LogEventLevel.Information, "GetVersiportDigitalInput: Device '0' is not a valid IIOPorts Device", dc.PortDeviceKey);
                return null;
            }

            if (dc.PortNumber > ioPortDevice.NumberOfVersiPorts)
            {
                Debug.LogMessage(LogEventLevel.Information, "GetVersiportDigitalInput: Device {0} does not contain a port {1}", dc.PortDeviceKey, dc.PortNumber);
            }

            return ioPortDevice.VersiPorts[dc.PortNumber];


        }
    }


    /// <summary>
    /// Represents a GenericVersiportDigitalInputDeviceFactory
    /// </summary>
    public class GenericVersiportDigitalInputDeviceFactory : EssentialsDeviceFactory<GenericVersiportDigitalInputDevice>
    {
        public GenericVersiportDigitalInputDeviceFactory()
        {
            TypeNames = new List<string>() { "versiportinput" };
        }

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Generic Versiport Device");

            var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());

            if (props == null) return null;

            var portDevice = new GenericVersiportDigitalInputDevice(dc.Key, dc.Name, GenericVersiportDigitalInputDevice.GetVersiportDigitalInput, props);

            return portDevice;
        }
    }
}