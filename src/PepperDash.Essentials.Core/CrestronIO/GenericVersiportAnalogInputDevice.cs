

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
    public class GenericVersiportAnalogInputDevice : EssentialsBridgeableDevice, IAnalogInput
    {
        public Versiport InputPort { get; private set; }

        public IntFeedback InputValueFeedback { get; private set; }
        public IntFeedback InputMinimumChangeFeedback { get; private set; }

        Func<int> InputValueFeedbackFunc
        {
            get
            {
                return () => InputPort.AnalogIn;
            }
        }

        Func<int> InputMinimumChangeFeedbackFunc
        {
            get { return () => InputPort.AnalogMinChange; }
        } 

        public GenericVersiportAnalogInputDevice(string key, string name, Func<IOPortConfig, Versiport> postActivationFunc, IOPortConfig config) :
            base(key, name)
        {
            InputValueFeedback = new IntFeedback(InputValueFeedbackFunc);
            InputMinimumChangeFeedback = new IntFeedback(InputMinimumChangeFeedbackFunc);

            AddPostActivationAction(() =>
            {
                InputPort = postActivationFunc(config);

                InputPort.Register();

                InputPort.SetVersiportConfiguration(eVersiportConfiguration.AnalogInput);
                InputPort.AnalogMinChange = (ushort)(config.MinimumChange > 0 ? config.MinimumChange : 655);
                if (config.DisablePullUpResistor)
                    InputPort.DisablePullUpResistor = true;

                InputPort.VersiportChange += InputPort_VersiportChange;

                Debug.Console(1, this, "Created GenericVersiportAnalogInputDevice on port '{0}'.  DisablePullUpResistor: '{1}'", config.PortNumber, InputPort.DisablePullUpResistor);

            });

        }

        /// <summary>
        /// Set minimum voltage change for device to update voltage changed method
        /// </summary>
        /// <param name="value">valid values range from 0 - 65535, representing the full 100% range of the processor voltage source.  Check processor documentation for details</param>
        public void SetMinimumChange(ushort value)
        {
            InputPort.AnalogMinChange = value;
        }

        void InputPort_VersiportChange(Versiport port, VersiportEventArgs args)
        {
			Debug.Console(1, this, "Versiport change: {0}", args.Event);

            if(args.Event == eVersiportEvent.AnalogInChange)
                InputValueFeedback.FireUpdate();
            if (args.Event == eVersiportEvent.AnalogMinChangeChange)
                InputMinimumChangeFeedback.FireUpdate();
        }


        #region Bridge Linking

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
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            try
            {
                Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

                // Link feedback for input state
                InputValueFeedback.LinkInputSig(trilist.UShortInput[joinMap.InputValue.JoinNumber]);
                InputMinimumChangeFeedback.LinkInputSig(trilist.UShortInput[joinMap.MinimumChange.JoinNumber]);
                trilist.SetUShortSigAction(joinMap.MinimumChange.JoinNumber, SetMinimumChange);

            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Unable to link device '{0}'.  Input is null", Key);
                Debug.Console(1, this, "Error: {0}", e);
            }

            trilist.OnlineStatusChange += (d, args) =>
            {
                if (!args.DeviceOnLine) return;
                InputValueFeedback.FireUpdate();
                InputMinimumChangeFeedback.FireUpdate();
            };

        }

        void trilist_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion


        public static Versiport GetVersiportDigitalInput(IOPortConfig dc)
        {
         
            IIOPorts ioPortDevice;

            if (dc.PortDeviceKey.Equals("processor"))
            {
                if (!Global.ControlSystem.SupportsVersiport)
                {
                    Debug.Console(0, "GetVersiportAnalogInput: Processor does not support Versiports");
                    return null;
                }
                ioPortDevice = Global.ControlSystem;
            }
            else
            {
                var ioPortDev = DeviceManager.GetDeviceForKey(dc.PortDeviceKey) as IIOPorts;
                if (ioPortDev == null)
                {
                    Debug.Console(0, "GetVersiportAnalogInput: Device {0} is not a valid device", dc.PortDeviceKey);
                    return null;
                }
                ioPortDevice = ioPortDev;
            }
            if (ioPortDevice == null)
            {
                Debug.Console(0, "GetVersiportAnalogInput: Device '0' is not a valid IIOPorts Device", dc.PortDeviceKey);
                return null;
            }

            if (dc.PortNumber > ioPortDevice.NumberOfVersiPorts)
            {
                Debug.Console(0, "GetVersiportAnalogInput: Device {0} does not contain a port {1}", dc.PortDeviceKey, dc.PortNumber);
                return null;
            }
            if(!ioPortDevice.VersiPorts[dc.PortNumber].SupportsAnalogInput)
            {
                Debug.Console(0, "GetVersiportAnalogInput: Device {0} does not support AnalogInput on port {1}", dc.PortDeviceKey, dc.PortNumber);
                return null;
            }


            return ioPortDevice.VersiPorts[dc.PortNumber];


        }
    }


    public class GenericVersiportAbalogInputDeviceFactory : EssentialsDeviceFactory<GenericVersiportAnalogInputDevice>
    {
        public GenericVersiportAbalogInputDeviceFactory()
        {
            TypeNames = new List<string>() { "versiportanaloginput" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Generic Versiport Device");

            var props = JsonConvert.DeserializeObject<IOPortConfig>(dc.Properties.ToString());

            if (props == null) return null;

            var portDevice = new GenericVersiportAnalogInputDevice(dc.Key, dc.Name, GenericVersiportAnalogInputDevice.GetVersiportDigitalInput, props);

            return portDevice;
        }
    }
}