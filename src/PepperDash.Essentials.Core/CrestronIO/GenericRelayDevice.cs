

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Represents a generic device controlled by relays
    /// </summary>
    [Description("Wrapper class for a Relay")]
    public class GenericRelayDevice : EssentialsBridgeableDevice, ISwitchedOutput
    {
        /// <summary>
        /// The RelayOutput controlled by this device
        /// </summary>
        public Relay RelayOutput { get; private set; }

        /// <summary>
        /// Feedback to indicate whether the output is on
        /// </summary>
        public BoolFeedback OutputIsOnFeedback { get; private set; }

        //Maintained for compatibility with PepperDash.Essentials.Core.Devices.CrestronProcessor
        /// <summary>
        /// Constructor for GenericRelayDevice
        /// </summary>
        /// <param name="key">key of the device</param>
        /// <param name="relay">Relay output controlled by this device</param>
        public GenericRelayDevice(string key, Relay relay) :
            base(key)
        {
            OutputIsOnFeedback = new BoolFeedback(new Func<bool>(() => RelayOutput.State));

            RelayOutput = relay;
            RelayOutput.Register();

            RelayOutput.StateChange += RelayOutput_StateChange;
        }

        /// <summary>
        /// Constructor for GenericRelayDevice
        /// </summary>
        /// <param name="key">key of the device</param>
        /// <param name="name">name of the device</param>
        /// <param name="postActivationFunc">function to get the relay output</param>
        /// <param name="config">IO port configuration</param>
        public GenericRelayDevice(string key, string name, Func<IOPortConfig, Relay> postActivationFunc,
            IOPortConfig config)
            : base(key, name)
        {
            OutputIsOnFeedback = new BoolFeedback(() => RelayOutput.State);

            AddPostActivationAction(() =>
            {
                RelayOutput = postActivationFunc(config);

                if (RelayOutput == null)
                {
                    Debug.LogMessage(LogEventLevel.Information, this, "Unable to get parent relay device for device key {0} and port {1}", config.PortDeviceKey, config.PortNumber);
                    return;
                }

                RelayOutput.Register();

                RelayOutput.StateChange += RelayOutput_StateChange;
            });
        }

        #region PreActivate

        private static Relay GetRelay(IOPortConfig dc)
        {

            IRelayPorts relayDevice;

            if(dc.PortDeviceKey.Equals("processor"))
            {
                if (!Global.ControlSystem.SupportsRelay)
                {
                    Debug.LogMessage(LogEventLevel.Information, "Processor does not support relays");
                    return null;
                }
                relayDevice = Global.ControlSystem;

                return relayDevice.RelayPorts[dc.PortNumber];
            }
            
            var essentialsDevice = DeviceManager.GetDeviceForKey(dc.PortDeviceKey);
            if (essentialsDevice == null)
            {
                Debug.LogMessage(LogEventLevel.Information, "Device {0} was not found in Device Manager. Check configuration or for errors with device.", dc.PortDeviceKey);
                return null;
            }

            relayDevice = essentialsDevice as IRelayPorts;
            
            if (relayDevice == null)
            {
                Debug.LogMessage(LogEventLevel.Information, "Device {0} is not a valid relay parent. Please check configuration.", dc.PortDeviceKey);
                return null;
            }

            if (dc.PortNumber <= relayDevice.NumberOfRelayPorts)
            {
                return relayDevice.RelayPorts[dc.PortNumber];
            }

            Debug.LogMessage(LogEventLevel.Information, "Device {0} does not contain a port {1}", dc.PortDeviceKey, dc.PortNumber);
            return null;
        }

        #endregion

        #region Events

        void RelayOutput_StateChange(Relay relay, RelayEventArgs args)
        {
            OutputIsOnFeedback.FireUpdate();
        }

        #endregion

        #region Methods

        /// <summary>
        /// OpenRelay method
        /// </summary>
        public void OpenRelay()
        {
            RelayOutput.State = false;
        }

        /// <summary>
        /// CloseRelay method
        /// </summary>
        public void CloseRelay()
        {
            RelayOutput.State = true;
        }

        /// <summary>
        /// ToggleRelayState method
        /// </summary>
        public void ToggleRelayState()
        {
            if (RelayOutput.State == true)
                OpenRelay();
            else
                CloseRelay();
        }

        #endregion

        #region ISwitchedOutput Members

        void ISwitchedOutput.On()
        {
            CloseRelay();
        }

        void ISwitchedOutput.Off()
        {
            OpenRelay();
        }

        #endregion

        #region Bridge Linking

        /// <summary>
        /// LinkToApi method
        /// </summary>
        /// <inheritdoc />
        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new GenericRelayControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<GenericRelayControllerJoinMap>(joinMapSerialized);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Information, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            if (RelayOutput == null)
            {
                Debug.LogMessage(LogEventLevel.Debug, this, "Unable to link device '{0}'.  Relay is null", Key);
                return;
            }

            Debug.LogMessage(LogEventLevel.Debug, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            trilist.SetBoolSigAction(joinMap.Relay.JoinNumber, b =>
            {
                if (b)
                    CloseRelay();
                else
                    OpenRelay();
            });

            // feedback for relay state

            OutputIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Relay.JoinNumber]);
        }

        #endregion

        #region Factory

        /// <summary>
        /// Represents a GenericRelayDeviceFactory
        /// </summary>
        public class GenericRelayDeviceFactory : EssentialsDeviceFactory<GenericRelayDevice>
        {
            /// <summary>
            /// Constructor for GenericRelayDeviceFactory
            /// </summary>
            public GenericRelayDeviceFactory()
            {
                TypeNames = new List<string>() { "relayoutput" };
            }

            /// <summary>
            /// BuildDevice method
            /// </summary>
            /// <inheritdoc />
            public override EssentialsDevice BuildDevice(DeviceConfig dc)
            {
                Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Generic Relay Device");

                var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());

                if (props == null) return null;

                var portDevice = new GenericRelayDevice(dc.Key, dc.Name, GetRelay, props);

                return portDevice;
            }
        }

        #endregion


    }


}