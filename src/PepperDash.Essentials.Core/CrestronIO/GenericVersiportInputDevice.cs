

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges;


using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Represents a generic digital input deviced tied to a versiport
    /// </summary>
    public class GenericVersiportDigitalInputDevice : EssentialsBridgeableDevice, IDigitalInput
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

        public GenericVersiportDigitalInputDevice(string key, string name, Func<IOPortConfig, Versiport> postActivationFunc, IOPortConfig config) :
            base(key, name)
        {
            InputStateFeedback = new BoolFeedback(InputStateFeedbackFunc);

            AddPostActivationAction(() =>
            {
                InputPort = postActivationFunc(config);

                InputPort.Register();

                InputPort.SetVersiportConfiguration(eVersiportConfiguration.DigitalInput);
                if (config.DisablePullUpResistor)
                    InputPort.DisablePullUpResistor = true;

                InputPort.VersiportChange += InputPort_VersiportChange;



                Debug.Console(1, this, "Created GenericVersiportDigitalInputDevice on port '{0}'.  DisablePullUpResistor: '{1}'", config.PortNumber, InputPort.DisablePullUpResistor);

            });

        }

        void InputPort_VersiportChange(Versiport port, VersiportEventArgs args)
        {
			Debug.Console(1, this, "Versiport change: {0}", args.Event);

            if(args.Event == eVersiportEvent.DigitalInChange)
                InputStateFeedback.FireUpdate();
        }


        #region Bridge Linking

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
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

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


        public static Versiport GetVersiportDigitalInput(IOPortConfig dc)
        {
         
            IIOPorts ioPortDevice;

            if (dc.PortDeviceKey.Equals("processor"))
            {
                if (!Global.ControlSystem.SupportsVersiport)
                {
                    Debug.Console(0, "GetVersiportDigitalInput: Processor does not support Versiports");
                    return null;
                }
                ioPortDevice = Global.ControlSystem;
            }
            else
            {
                var ioPortDev = DeviceManager.GetDeviceForKey(dc.PortDeviceKey) as IIOPorts;
                if (ioPortDev == null)
                {
                    Debug.Console(0, "GetVersiportDigitalInput: Device {0} is not a valid device", dc.PortDeviceKey);
                    return null;
                }
                ioPortDevice = ioPortDev;
            }
            if (ioPortDevice == null)
            {
                Debug.Console(0, "GetVersiportDigitalInput: Device '0' is not a valid IIOPorts Device", dc.PortDeviceKey);
                return null;
            }

            if (dc.PortNumber > ioPortDevice.NumberOfVersiPorts)
            {
                Debug.Console(0, "GetVersiportDigitalInput: Device {0} does not contain a port {1}", dc.PortDeviceKey, dc.PortNumber);
            }

            return ioPortDevice.VersiPorts[dc.PortNumber];


        }
    }


    public class GenericVersiportDigitalInputDeviceFactory : EssentialsDeviceFactory<GenericVersiportDigitalInputDevice>
    {
        public GenericVersiportDigitalInputDeviceFactory()
        {
            TypeNames = new List<string>() { "versiportinput" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Versiport Device");

            var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());

            if (props == null) return null;

            var portDevice = new GenericVersiportDigitalInputDevice(dc.Key, dc.Name, GenericVersiportDigitalInputDevice.GetVersiportDigitalInput, props);

            return portDevice;
        }
    }
}