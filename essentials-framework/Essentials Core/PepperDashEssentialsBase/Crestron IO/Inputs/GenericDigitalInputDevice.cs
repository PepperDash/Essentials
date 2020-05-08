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


namespace PepperDash.Essentials.Core.CrestronIO
{
    public class GenericDigitalInputDevice : EssentialsBridgeableDevice, IDigitalInput
    {
        public DigitalInput InputPort { get; private set; }

        public BoolFeedback InputStateFeedback { get; private set; }

        Func<bool> InputStateFeedbackFunc
        {
            get
            {
                return () => InputPort.State;
            }
        }

        public GenericDigitalInputDevice(string key, DigitalInput inputPort):
            base(key)
        {
            InputStateFeedback = new BoolFeedback(InputStateFeedbackFunc);

            InputPort = inputPort;

            InputPort.StateChange += InputPort_StateChange;

        }

        void InputPort_StateChange(DigitalInput digitalInput, DigitalInputEventArgs args)
        {
            InputStateFeedback.FireUpdate();
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new IDigitalInputJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<IDigitalInputJoinMap>(joinMapSerialized);

            bridge.AddJoinMap(Key, joinMap);

            try
            {
                Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

                // Link feedback for input state
                InputStateFeedback.LinkInputSig(trilist.BooleanInput[joinMap.InputState.JoinNumber]);
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Unable to link device '{0}'.  Input is null", Key);
                Debug.Console(1, this, "Error: {0}", e);
            }
        }
    }

    public class GenericDigitalInputDeviceFactory : EssentialsDeviceFactory<GenericDigitalInputDevice>
    {
        public GenericDigitalInputDeviceFactory()
        {
            TypeNames = new List<string>() { "digitalinput" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Digtal Input Device");

            var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());

            IDigitalInputPorts portDevice;

            if (props.PortDeviceKey == "processor")
                portDevice = Global.ControlSystem as IDigitalInputPorts;
            else
                portDevice = DeviceManager.GetDeviceForKey(props.PortDeviceKey) as IDigitalInputPorts;

            if (portDevice == null)
                Debug.Console(0, "ERROR: Unable to add digital input device with key '{0}'. Port Device does not support digital inputs", dc.Key);
            else
            {
                var cs = (portDevice as CrestronControlSystem);
                if (cs == null)
                {
                    Debug.Console(0, "ERROR: Port device for [{0}] is not control system", props.PortDeviceKey);
                    return null;
                }

                if (cs.SupportsVersiport)
                {
                    Debug.Console(1, "Attempting to add Digital Input device to Versiport port '{0}'", props.PortNumber);

                    if (props.PortNumber > cs.NumberOfVersiPorts)
                    {
                        Debug.Console(0, "WARNING: Cannot add Vesiport {0} on {1}. Out of range",
                            props.PortNumber, props.PortDeviceKey);
                        return null;
                    }

                    Versiport vp = cs.VersiPorts[props.PortNumber];

                    if (!vp.Registered)
                    {
                        var regSuccess = vp.Register();
                        if (regSuccess == eDeviceRegistrationUnRegistrationResponse.Success)
                        {
                            Debug.Console(1, "Successfully Created Digital Input Device on Versiport");
                            return new GenericVersiportDigitalInputDevice(dc.Key, vp, props);
                        }
                        else
                        {
                            Debug.Console(0, "WARNING: Attempt to register versiport {0} on device with key '{1}' failed: {2}",
                                props.PortNumber, props.PortDeviceKey, regSuccess);
                            return null;
                        }
                    }
                }
                else if (cs.SupportsDigitalInput)
                {
                    Debug.Console(1, "Attempting to add Digital Input device to Digital Input port '{0}'", props.PortNumber);

                    if (props.PortNumber > cs.NumberOfDigitalInputPorts)
                    {
                        Debug.Console(0, "WARNING: Cannot register DIO port {0} on {1}. Out of range",
                            props.PortNumber, props.PortDeviceKey);
                        return null;
                    }

                    DigitalInput digitalInput = cs.DigitalInputPorts[props.PortNumber];

                    if (!digitalInput.Registered)
                    {
                        if (digitalInput.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
                        {
                            Debug.Console(1, "Successfully Created Digital Input Device on Digital Input");
                            return new GenericDigitalInputDevice(dc.Key, digitalInput);
                        }
                        else
                            Debug.Console(0, "WARNING: Attempt to register digital input {0} on device with key '{1}' failed.",
                                props.PortNumber, props.PortDeviceKey);
                    }
                }
            }
            return null;
        }
    }

}