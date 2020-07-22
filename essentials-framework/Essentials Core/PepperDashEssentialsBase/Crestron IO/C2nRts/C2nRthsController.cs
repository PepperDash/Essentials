using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;


namespace PepperDash.Essentials.Core.CrestronIO
{
    [Description("Wrapper class for the C2N-RTHS sensor")]
    public class C2nRthsController : CrestronGenericBridgeableBaseDevice
    {
        private C2nRths _device;

        public IntFeedback TemperatureFeedback { get; private set; }
        public IntFeedback HumidityFeedback { get; private set; }

        public C2nRthsController(string key, Func<DeviceConfig, C2nRths> preActivationFunc,
            DeviceConfig config)
            : base(key, config.Name)
        {

            AddPreActivationAction(() =>
            {
                _device = preActivationFunc(config);

                RegisterCrestronGenericBase(_device);

                TemperatureFeedback = new IntFeedback(() => _device.TemperatureFeedback.UShortValue);
                HumidityFeedback = new IntFeedback(() => _device.HumidityFeedback.UShortValue);

                if (_device != null) _device.BaseEvent += DeviceOnBaseEvent;
            });
        }


        private void DeviceOnBaseEvent(GenericBase device, BaseEventArgs args)
        {
            switch (args.EventId)
            {
                case C2nRths.TemperatureFeedbackEventId:
                    TemperatureFeedback.FireUpdate();
                    break;
                case C2nRths.HumidityFeedbackEventId:
                    HumidityFeedback.FireUpdate();
                    break;
            }
        }

        public void SetTemperatureFormat(bool setToC)
        {
            _device.TemperatureFormat.BoolValue = setToC;
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new C2nRthsControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<C2nRthsControllerJoinMap>(joinMapSerialized);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));


            trilist.SetBoolSigAction(joinMap.TemperatureFormat.JoinNumber, SetTemperatureFormat);



            IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
            TemperatureFeedback.LinkInputSig(trilist.UShortInput[joinMap.Temperature.JoinNumber]);
            HumidityFeedback.LinkInputSig(trilist.UShortInput[joinMap.Humidity.JoinNumber]);

            trilist.StringInput[joinMap.Name.JoinNumber].StringValue = Name;

            trilist.OnlineStatusChange += (d, args) =>
            {
                if (!args.DeviceOnLine) return;

                UpdateFeedbacksWhenOnline();

                trilist.StringInput[joinMap.Name.JoinNumber].StringValue = Name;
            };
        }

        private void UpdateFeedbacksWhenOnline()
        {
            IsOnline.FireUpdate();
            TemperatureFeedback.FireUpdate();
            HumidityFeedback.FireUpdate();
        }

        #region PreActivation

        private static C2nRths GetC2nRthsDevice(DeviceConfig dc)
        {
            var control = CommFactory.GetControlPropertiesConfig(dc);
            var cresnetId = control.CresnetIdInt;
            var branchId = control.ControlPortNumber;
            var parentKey = string.IsNullOrEmpty(control.ControlPortDevKey) ? "processor" : control.ControlPortDevKey;

            if (parentKey.Equals("processor", StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new C2nRths", parentKey);
                return new C2nRths(cresnetId, Global.ControlSystem);
            }
            var cresnetBridge = DeviceManager.GetDeviceForKey(parentKey) as IHasCresnetBranches;

            if (cresnetBridge != null)
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new C2nRths", parentKey);
                return new C2nRths(cresnetId, cresnetBridge.CresnetBranches[branchId]);
            }
            Debug.Console(0, "Device {0} is not a valid cresnet master", parentKey);
            return null;
        }
        #endregion

        public class C2nRthsControllerFactory : EssentialsDeviceFactory<C2nRthsController>
        {
            public C2nRthsControllerFactory()
            {
                TypeNames = new List<string>() { "c2nrths" };
            }

            public override EssentialsDevice BuildDevice(DeviceConfig dc)
            {
                Debug.Console(1, "Factory Attempting to create new C2N-RTHS Device");

                return new C2nRthsController(dc.Key, GetC2nRthsDevice, dc);
            }
        }
    }
}