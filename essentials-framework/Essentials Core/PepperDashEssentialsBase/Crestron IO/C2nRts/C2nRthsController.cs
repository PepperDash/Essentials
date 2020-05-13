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
        private readonly C2nRths _device;

        public IntFeedback TemperatureFeedback { get; private set; }
        public IntFeedback HumidityFeedback { get; private set; }

        public C2nRthsController(string key, string name, GenericBase hardware) : base(key, name, hardware)
        {
            _device = hardware as C2nRths;

            TemperatureFeedback = new IntFeedback(() => _device.TemperatureFeedback.UShortValue);
            HumidityFeedback = new IntFeedback(() => _device.HumidityFeedback.UShortValue);

            if (_device != null) _device.BaseEvent += DeviceOnBaseEvent;
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

            bridge.AddJoinMap(Key, joinMap);

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));


            trilist.SetBoolSigAction(joinMap.TemperatureFormat.JoinNumber, SetTemperatureFormat);



            IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
            TemperatureFeedback.LinkInputSig(trilist.UShortInput[joinMap.Temperature.JoinNumber]);
            HumidityFeedback.LinkInputSig(trilist.UShortInput[joinMap.Humidity.JoinNumber]);

            trilist.StringInput[joinMap.Name.JoinNumber].StringValue = Name;
        }
    }

    public class C2nRthsControllerFactory : EssentialsDeviceFactory<C2nRthsController>
    {
        public C2nRthsControllerFactory()
        {
            TypeNames = new List<string>() { "c2nrths" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new C2N-RTHS Device");

            var control = CommFactory.GetControlPropertiesConfig(dc);
            var cresnetId = control.CresnetIdInt;

            return new C2nRthsController(dc.Key, dc.Name, new C2nRths(cresnetId, Global.ControlSystem));
        }
    }
}