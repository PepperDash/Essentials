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
using PepperDash_Essentials_Core.Devices;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Represents a generic device controlled by relays
    /// </summary>
    public class GenericRelayDevice : EssentialsBridgeableDevice, ISwitchedOutput
    {
        public Relay RelayOutput { get; private set; }

        public BoolFeedback OutputIsOnFeedback { get; private set; }

        public GenericRelayDevice(string key, Relay relay):
            base(key)
        {
            OutputIsOnFeedback = new BoolFeedback(new Func<bool>(() => RelayOutput.State));

            RelayOutput = relay;
            RelayOutput.Register();

            RelayOutput.StateChange += new RelayEventHandler(RelayOutput_StateChange);
        }

        void RelayOutput_StateChange(Relay relay, RelayEventArgs args)
        {
            OutputIsOnFeedback.FireUpdate();
        }

        public void OpenRelay()
        {
            RelayOutput.State = false;
        }

        public void CloseRelay()
        {
            RelayOutput.State = true;
        }

        public void ToggleRelayState()
        {
            if (RelayOutput.State == true)
                OpenRelay();
            else
                CloseRelay();
        }

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

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApi bridge)
        {
            var joinMap = new GenericRelayControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<GenericRelayControllerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            if (RelayOutput == null)
            {
                Debug.Console(1, this, "Unable to link device '{0}'.  Relay is null", Key);
                return;
            }

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            trilist.SetBoolSigAction(joinMap.Relay, b =>
            {
                if (b)
                    CloseRelay();
                else
                    OpenRelay();
            });

            // feedback for relay state

            OutputIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Relay]);
        }
    }

    public class GenericRelayDeviceFactory : EssentialsDeviceFactory<GenericRelayDevice>
    {
        public GenericRelayDeviceFactory()
        {
            TypeNames = new List<string>() { "relayoutput" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Relay Device");

            var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());
            var key = dc.Key;

            IRelayPorts portDevice;

            if (props.PortDeviceKey == "processor")
                portDevice = Global.ControlSystem as IRelayPorts;
            else
                portDevice = DeviceManager.GetDeviceForKey(props.PortDeviceKey) as IRelayPorts;

            if (portDevice == null)
                Debug.Console(0, "Unable to add relay device with key '{0}'. Port Device does not support relays", key);
            else
            {
                var cs = (portDevice as CrestronControlSystem);

                if (cs != null)
                {
                    // The relay is on a control system processor
                    if (!cs.SupportsRelay || props.PortNumber > cs.NumberOfRelayPorts)
                    {
                        Debug.Console(0, "Port Device: {0} does not support relays or does not have enough relays");
                        return null;
                    }
                }
                else
                {
                    // The relay is on another device type

                    if (props.PortNumber > portDevice.NumberOfRelayPorts)
                    {
                        Debug.Console(0, "Port Device: {0} does not have enough relays");
                        return null;
                    }
                }

                Relay relay = portDevice.RelayPorts[props.PortNumber];

                if (!relay.Registered)
                {
                    if (relay.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
                        return new GenericRelayDevice(key, relay);
                    else
                        Debug.Console(0, "Attempt to register relay {0} on device with key '{1}' failed.", props.PortNumber, props.PortDeviceKey);
                }
                else
                {
                    return new GenericRelayDevice(key, relay);
                }

                // Future: Check if portDevice is 3-series card or other non control system that supports versiports
            }

            return null;

        }
    }

}