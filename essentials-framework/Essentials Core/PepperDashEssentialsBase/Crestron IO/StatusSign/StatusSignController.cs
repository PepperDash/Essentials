using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.CrestronIO
{
    public class StatusSignController:CrestronGenericBaseDevice
    {
        private StatusSign _device;

        public BoolFeedback RedLedEnabledFeedback { get; private set; }
        public BoolFeedback GreenLedEnabledFeedback { get; private set; }
        public BoolFeedback BlueLedEnabledFeedback { get; private set; }

        public IntFeedback RedLedBrightnessFeedback { get; private set; }
        public IntFeedback GreenLedBrightnessFeedback { get; private set; }
        public IntFeedback BlueLedBrightnessFeedback { get; private set; }

        public StatusSignController(string key, string name, GenericBase hardware) : base(key, name, hardware)
        {
            _device = hardware as StatusSign;

            RedLedEnabledFeedback =
                new BoolFeedback(
                    () =>
                        _device.Leds[(uint) StatusSign.Led.eLedColor.Red]
                            .ControlFeedback.BoolValue);
            GreenLedEnabledFeedback =
                new BoolFeedback(
                    () =>
                        _device.Leds[(uint) StatusSign.Led.eLedColor.Green]
                            .ControlFeedback.BoolValue);
            BlueLedEnabledFeedback =
                new BoolFeedback(
                    () =>
                        _device.Leds[(uint) StatusSign.Led.eLedColor.Blue]
                            .ControlFeedback.BoolValue);

            RedLedBrightnessFeedback =
                new IntFeedback(() => (int) _device.Leds[(uint) StatusSign.Led.eLedColor.Red].BrightnessFeedback);
            GreenLedBrightnessFeedback =
                new IntFeedback(() => (int) _device.Leds[(uint) StatusSign.Led.eLedColor.Green].BrightnessFeedback);
            BlueLedBrightnessFeedback =
                new IntFeedback(() => (int) _device.Leds[(uint) StatusSign.Led.eLedColor.Blue].BrightnessFeedback);

            _device.BaseEvent += _device_BaseEvent;
        }

        void _device_BaseEvent(GenericBase device, BaseEventArgs args)
        {
            switch (args.EventId)
            {
                case StatusSign.LedBrightnessFeedbackEventId:
                    RedLedBrightnessFeedback.FireUpdate();
                    GreenLedBrightnessFeedback.FireUpdate();
                    BlueLedBrightnessFeedback.FireUpdate();
                    break;
                case StatusSign.LedControlFeedbackEventId:
                    RedLedEnabledFeedback.FireUpdate();
                    GreenLedEnabledFeedback.FireUpdate();
                    BlueLedEnabledFeedback.FireUpdate();
                    break;
            }
        }

        public void EnableLedControl(bool red, bool green, bool blue)
        {
            _device.Leds[(uint) StatusSign.Led.eLedColor.Red].Control.BoolValue = red;
            _device.Leds[(uint)StatusSign.Led.eLedColor.Green].Control.BoolValue = green;
            _device.Leds[(uint)StatusSign.Led.eLedColor.Blue].Control.BoolValue = blue;
        }

        public void SetColor(uint red, uint green, uint blue)
        {
            try
            {
                _device.Leds[(uint)StatusSign.Led.eLedColor.Red].Brightness =
                    (StatusSign.Led.eBrightnessPercentageValues)SimplSharpDeviceHelper.PercentToUshort(red);
            }
            catch (InvalidOperationException)
            {
                Debug.Console(1, this, "Error converting value to Red LED brightness. value: {0}", red);
            }
            try
            {
                _device.Leds[(uint)StatusSign.Led.eLedColor.Green].Brightness =
                    (StatusSign.Led.eBrightnessPercentageValues)SimplSharpDeviceHelper.PercentToUshort(green);
            }
            catch (InvalidOperationException)
            {
                Debug.Console(1, this, "Error converting value to Green LED brightness. value: {0}", green);
            }

            try
            {
                _device.Leds[(uint)StatusSign.Led.eLedColor.Blue].Brightness =
                    (StatusSign.Led.eBrightnessPercentageValues)SimplSharpDeviceHelper.PercentToUshort(blue);
            }
            catch (InvalidOperationException)
            {
                Debug.Console(1, this, "Error converting value to Blue LED brightness. value: {0}", blue);
            }
        }
    }
}