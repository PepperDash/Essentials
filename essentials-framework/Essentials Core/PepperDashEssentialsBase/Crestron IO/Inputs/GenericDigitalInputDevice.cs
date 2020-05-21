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
    [Description("Wrapper class for Digital Input")]
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


        public GenericDigitalInputDevice(string key, string name, Func<IOPortConfig, DigitalInput> postActivationFunc,
            IOPortConfig config)
            : base(key, name)
        {

            AddPostActivationAction(() =>
            {
                InputPort = postActivationFunc(config);

                InputPort.Register();

                InputPort.StateChange += InputPort_StateChange;

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
            IDigitalInputPorts ioPortDevice;

            if (dc.PortDeviceKey.Equals("processor"))
            {
                if (!Global.ControlSystem.SupportsDigitalInput)
                {
                    Debug.Console(0, "GetDigitalInput: Processor does not support Digital Inputs");
                    return null;
                }
                ioPortDevice = Global.ControlSystem;
            }
            else
            {
                var ioPortDev = DeviceManager.GetDeviceForKey(dc.PortDeviceKey) as IDigitalInputPorts;
                if (ioPortDev == null)
                {
                    Debug.Console(0, "GetDigitalInput: Device {0} is not a valid device", dc.PortDeviceKey);
                    return null;
                }
                ioPortDevice = ioPortDev;
            }
            if (ioPortDevice == null)
            {
                Debug.Console(0, "GetDigitalInput: Device '0' is not a valid IRelayPorts Device", dc.PortDeviceKey);
                return null;
            }

            if (dc.PortNumber > ioPortDevice.NumberOfDigitalInputPorts)
            {
                Debug.Console(0, "GetDigitalInput: Device {0} does not contain a port {1}", dc.PortDeviceKey, dc.PortNumber);
            }

            return ioPortDevice.DigitalInputPorts[dc.PortNumber];


        }

        #endregion

        #region Bridge Linking

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

        #endregion

        #region Factory

        public class GenericDigitalInputDeviceFactory : EssentialsDeviceFactory<GenericDigitalInputDevice>
        {
            public GenericDigitalInputDeviceFactory()
            {
                TypeNames = new List<string>() { "digitalinput" };
            }

            public override EssentialsDevice BuildDevice(DeviceConfig dc)
            {
                Debug.Console(1, "Factory Attempting to create new Generic Relay Device");

                var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());

                if (props == null) return null;

                var portDevice = new GenericDigitalInputDevice(dc.Key, dc.Name, GetDigitalInput, props);

                return portDevice;
            }
        }

        #endregion

    }


}