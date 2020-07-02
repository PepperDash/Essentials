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
    [Description("Wrapper class for the Crestron StatusSign device")]
    public class StatusSignController : CrestronGenericBridgeableBaseDevice
    {
        private StatusSign _device;

        public BoolFeedback RedLedEnabledFeedback { get; private set; }
        public BoolFeedback GreenLedEnabledFeedback { get; private set; }
        public BoolFeedback BlueLedEnabledFeedback { get; private set; }

        public IntFeedback RedLedBrightnessFeedback { get; private set; }
        public IntFeedback GreenLedBrightnessFeedback { get; private set; }
        public IntFeedback BlueLedBrightnessFeedback { get; private set; }

        public StatusSignController(string key, Func<DeviceConfig, StatusSign> preActivationFunc, DeviceConfig config) : base(key, config.Name)
        {
            AddPreActivationAction(() =>
            {
                _device = preActivationFunc(config);

                RegisterCrestronGenericBase(_device);

                RedLedEnabledFeedback =
                new BoolFeedback(
                    () =>
                        _device.Leds[(uint)StatusSign.Led.eLedColor.Red]
                            .ControlFeedback.BoolValue);
                GreenLedEnabledFeedback =
                    new BoolFeedback(
                        () =>
                            _device.Leds[(uint)StatusSign.Led.eLedColor.Green]
                                .ControlFeedback.BoolValue);
                BlueLedEnabledFeedback =
                    new BoolFeedback(
                        () =>
                            _device.Leds[(uint)StatusSign.Led.eLedColor.Blue]
                                .ControlFeedback.BoolValue);

                RedLedBrightnessFeedback =
                    new IntFeedback(() => (int)_device.Leds[(uint)StatusSign.Led.eLedColor.Red].BrightnessFeedback);
                GreenLedBrightnessFeedback =
                    new IntFeedback(() => (int)_device.Leds[(uint)StatusSign.Led.eLedColor.Green].BrightnessFeedback);
                BlueLedBrightnessFeedback =
                    new IntFeedback(() => (int)_device.Leds[(uint)StatusSign.Led.eLedColor.Blue].BrightnessFeedback);

                if (_device != null) _device.BaseEvent += _device_BaseEvent;

            });
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

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new StatusSignControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<StatusSignControllerJoinMap>(joinMapSerialized);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            trilist.SetBoolSigAction(joinMap.RedControl.JoinNumber, b => EnableControl(trilist, joinMap, this));
            trilist.SetBoolSigAction(joinMap.GreenControl.JoinNumber, b => EnableControl(trilist, joinMap, this));
            trilist.SetBoolSigAction(joinMap.BlueControl.JoinNumber, b => EnableControl(trilist, joinMap, this));

            trilist.SetUShortSigAction(joinMap.RedLed.JoinNumber, u => SetColor(trilist, joinMap, this));
            trilist.SetUShortSigAction(joinMap.GreenLed.JoinNumber, u => SetColor(trilist, joinMap, this));
            trilist.SetUShortSigAction(joinMap.BlueLed.JoinNumber, u => SetColor(trilist, joinMap, this));

            trilist.StringInput[joinMap.Name.JoinNumber].StringValue = Name;

            IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
            RedLedEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RedControl.JoinNumber]);
            BlueLedEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.BlueControl.JoinNumber]);
            GreenLedEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.GreenControl.JoinNumber]);

            RedLedBrightnessFeedback.LinkInputSig(trilist.UShortInput[joinMap.RedLed.JoinNumber]);
            BlueLedBrightnessFeedback.LinkInputSig(trilist.UShortInput[joinMap.BlueLed.JoinNumber]);
            GreenLedBrightnessFeedback.LinkInputSig(trilist.UShortInput[joinMap.GreenLed.JoinNumber]);
        }

        private static void EnableControl(BasicTriList triList, StatusSignControllerJoinMap joinMap,
            StatusSignController device)
        {
            var redEnable = triList.BooleanOutput[joinMap.RedControl.JoinNumber].BoolValue;
            var greenEnable = triList.BooleanOutput[joinMap.GreenControl.JoinNumber].BoolValue;
            var blueEnable = triList.BooleanOutput[joinMap.BlueControl.JoinNumber].BoolValue;
            device.EnableLedControl(redEnable, greenEnable, blueEnable);
        }

        private static void SetColor(BasicTriList triList, StatusSignControllerJoinMap joinMap,
            StatusSignController device)
        {
            var redBrightness = triList.UShortOutput[joinMap.RedLed.JoinNumber].UShortValue;
            var greenBrightness = triList.UShortOutput[joinMap.GreenLed.JoinNumber].UShortValue;
            var blueBrightness = triList.UShortOutput[joinMap.BlueLed.JoinNumber].UShortValue;

            device.SetColor(redBrightness, greenBrightness, blueBrightness);
        }

        #region PreActivation

        private static StatusSign GetStatusSignDevice(DeviceConfig dc)
        {
            var control = CommFactory.GetControlPropertiesConfig(dc);
            var cresnetId = control.CresnetIdInt;
            var branchId = control.ControlPortNumber;
            var parentKey = string.IsNullOrEmpty(control.ControlPortDevKey) ? "processor" : control.ControlPortDevKey;

            if (parentKey.Equals("processor", StringComparison.CurrentCultureIgnoreCase))
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new StatusSign", parentKey);
                return new StatusSign(cresnetId, Global.ControlSystem);
            }
            var cresnetBridge = DeviceManager.GetDeviceForKey(parentKey) as ICresnetBridge;

            if (cresnetBridge != null)
            {
                Debug.Console(0, "Device {0} is a valid cresnet master - creating new StatusSign", parentKey);
                return new StatusSign(cresnetId, cresnetBridge.Branches[branchId]);
            }
            Debug.Console(0, "Device {0} is not a valid cresnet master", parentKey);
            return null;
        }
        #endregion

        public class StatusSignControllerFactory : EssentialsDeviceFactory<StatusSignController>
        {
            public StatusSignControllerFactory()
            {
                TypeNames = new List<string>() { "statussign" };
            }

            public override EssentialsDevice BuildDevice(DeviceConfig dc)
            {
                Debug.Console(1, "Factory Attempting to create new StatusSign Device");

                var control = CommFactory.GetControlPropertiesConfig(dc);
                var cresnetId = control.CresnetIdInt;

                return new StatusSignController(dc.Key, GetStatusSignDevice, dc);
            }
        }
    }

    
}