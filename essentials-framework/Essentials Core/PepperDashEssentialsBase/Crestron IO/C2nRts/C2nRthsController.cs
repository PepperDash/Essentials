using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core.CrestronIO
{
    public class C2nRthsController:CrestronGenericBaseDevice
    {
        private C2nRths _device;

        public IntFeedback TemperatureFeedback { get; private set; }
        public IntFeedback HumidityFeedback { get; private set; }

        public C2nRthsController(string key, string name, GenericBase hardware) : base(key, name, hardware)
        {
            _device = hardware as C2nRths;

            TemperatureFeedback = new IntFeedback(() => _device.TemperatureFeedback.UShortValue);
            HumidityFeedback = new IntFeedback(() => _device.HumidityFeedback.UShortValue);

            _device.BaseEvent += DeviceOnBaseEvent;
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
    }
}