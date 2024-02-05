

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
    public class GenericVersiportDigitalOutputDevice : EssentialsBridgeableDevice, IDigitalOutput
    {
        public Versiport OutputPort { get; private set; }

        public BoolFeedback OutputStateFeedback { get; private set; }

        Func<bool> OutputStateFeedbackFunc
        {
            get
            {
                return () => OutputPort.DigitalOut;
            }
        }

        public GenericVersiportDigitalOutputDevice(string key, string name, Func<IOPortConfig, Versiport> postActivationFunc, IOPortConfig config) :
            base(key, name)
        {
            OutputStateFeedback = new BoolFeedback(OutputStateFeedbackFunc);

            AddPostActivationAction(() =>
            {
                OutputPort = postActivationFunc(config);

                OutputPort.Register();


                if (!OutputPort.SupportsDigitalOutput)
                {
                    Debug.Console(0, this, "Device does not support configuration as a Digital Output");
                    return;
                }

                OutputPort.SetVersiportConfiguration(eVersiportConfiguration.DigitalOutput);


                OutputPort.VersiportChange += OutputPort_VersiportChange;

            });

        }

        void OutputPort_VersiportChange(Versiport port, VersiportEventArgs args)
        {
			Debug.Console(1, this, "Versiport change: {0}", args.Event);

            if(args.Event == eVersiportEvent.DigitalOutChange)
                OutputStateFeedback.FireUpdate();
        }

        /// <summary>
        /// Set value of the versiport digital output
        /// </summary>
        /// <param name="state">value to set the output to</param>
        public void SetOutput(bool state)
        {
                if (OutputPort.SupportsDigitalOutput)
                {
                    Debug.Console(0, this, "Passed the Check");

                    OutputPort.DigitalOut = state;

                }
                else
                {
                    Debug.Console(0, this, "Versiport does not support Digital Output Mode");
                }

        }

        #region Bridge Linking

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
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            try
            {
                Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

                // Link feedback for input state
                OutputStateFeedback.LinkInputSig(trilist.BooleanInput[joinMap.OutputState.JoinNumber]);
                trilist.SetBoolSigAction(joinMap.OutputState.JoinNumber, SetOutput);
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Unable to link device '{0}'.  Input is null", Key);
                Debug.Console(1, this, "Error: {0}", e);
            }
        }

        #endregion


        public static Versiport GetVersiportDigitalOutput(IOPortConfig dc)
        {

                IIOPorts ioPortDevice;

                if (dc.PortDeviceKey.Equals("processor"))
                {
                    if (!Global.ControlSystem.SupportsVersiport)
                    {
                        Debug.Console(0, "GetVersiportDigitalOuptut: Processor does not support Versiports");
                        return null;
                    }
                    ioPortDevice = Global.ControlSystem;
                }
                else
                {
                    var ioPortDev = DeviceManager.GetDeviceForKey(dc.PortDeviceKey) as IIOPorts;
                    if (ioPortDev == null)
                    {
                        Debug.Console(0, "GetVersiportDigitalOuptut: Device {0} is not a valid device", dc.PortDeviceKey);
                        return null;
                    }
                    ioPortDevice = ioPortDev;
                }
                if (ioPortDevice == null)
                {
                    Debug.Console(0, "GetVersiportDigitalOuptut: Device '0' is not a valid IOPorts Device", dc.PortDeviceKey);
                    return null;
                }

                if (dc.PortNumber > ioPortDevice.NumberOfVersiPorts)
                {
                    Debug.Console(0, "GetVersiportDigitalOuptut: Device {0} does not contain a port {1}", dc.PortDeviceKey, dc.PortNumber);
                }
                var port = ioPortDevice.VersiPorts[dc.PortNumber];
                return port;

        }
    }


    public class GenericVersiportDigitalOutputDeviceFactory : EssentialsDeviceFactory<GenericVersiportDigitalInputDevice>
    {
        public GenericVersiportDigitalOutputDeviceFactory()
        {
            TypeNames = new List<string>() { "versiportoutput" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Versiport Device");

            var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());

            if (props == null) return null;

            var portDevice = new GenericVersiportDigitalOutputDevice(dc.Key, dc.Name, GenericVersiportDigitalOutputDevice.GetVersiportDigitalOutput, props);

            return portDevice;
        }
    }
}