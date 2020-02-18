using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Bridges
{
    public static class StatusSignDeviceApiExtensions
    {
        public static void LinkToApi(this StatusSignController ssDevice, BasicTriList trilist, uint joinStart,
            string joinMapKey)
        {
            var joinMap = new StatusSignControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<StatusSignControllerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, ssDevice, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            trilist.SetBoolSigAction(joinMap.RedControl, b => EnableControl(trilist, joinMap, ssDevice));
            trilist.SetBoolSigAction(joinMap.GreenControl, b => EnableControl(trilist, joinMap, ssDevice));
            trilist.SetBoolSigAction(joinMap.BlueControl, b => EnableControl(trilist, joinMap, ssDevice));

            trilist.SetUShortSigAction(joinMap.RedLed, u => SetColor(trilist, joinMap, ssDevice));
            trilist.SetUShortSigAction(joinMap.GreenLed, u => SetColor(trilist, joinMap, ssDevice));
            trilist.SetUShortSigAction(joinMap.BlueLed, u => SetColor(trilist, joinMap, ssDevice));

            trilist.StringInput[joinMap.Name].StringValue = ssDevice.Name;

            ssDevice.IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            ssDevice.RedLedEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.RedControl]);
            ssDevice.BlueLedEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.BlueControl]);
            ssDevice.GreenLedEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.GreenControl]);

            ssDevice.RedLedBrightnessFeedback.LinkInputSig(trilist.UShortInput[joinMap.RedLed]);
            ssDevice.BlueLedBrightnessFeedback.LinkInputSig(trilist.UShortInput[joinMap.BlueLed]);
            ssDevice.GreenLedBrightnessFeedback.LinkInputSig(trilist.UShortInput[joinMap.GreenLed]);

        }

        private static void EnableControl(BasicTriList triList, StatusSignControllerJoinMap joinMap,
            StatusSignController device)
        {
            var redEnable = triList.BooleanOutput[joinMap.RedControl].BoolValue;
            var greenEnable = triList.BooleanOutput[joinMap.GreenControl].BoolValue;
            var blueEnable = triList.BooleanOutput[joinMap.BlueControl].BoolValue;
            device.EnableLedControl(redEnable, greenEnable, blueEnable);
        }

        private static void SetColor(BasicTriList triList, StatusSignControllerJoinMap joinMap,
            StatusSignController device)
        {
            var redBrightness = triList.UShortOutput[joinMap.RedLed].UShortValue;
            var greenBrightness = triList.UShortOutput[joinMap.GreenLed].UShortValue;
            var blueBrightness = triList.UShortOutput[joinMap.BlueLed].UShortValue;

            device.SetColor(redBrightness, greenBrightness, blueBrightness);
        }
    }
}