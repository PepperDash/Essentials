using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.CrestronIO;

namespace PepperDash.Essentials.Bridges
{
    public static class C2nRthsControllerApiExtensions
    {
        public static void LinkToApi(this C2nRthsController device, BasicTriList triList, uint joinStart,
            string joinMapKey)
        {
            var joinMap = new C2nRthsControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<C2nRthsControllerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, device, "Linking to Trilist '{0}'", triList.ID.ToString("X"));

            triList.SetBoolSigAction(joinMap.TemperatureFormat, device.SetTemperatureFormat);

            device.TemperatureFeedback.LinkInputSig(triList.UShortInput[joinMap.Temperature]);
            device.HumidityFeedback.LinkInputSig(triList.UShortInput[joinMap.Temperature]);

            triList.StringInput[joinMap.Name].StringValue = device.Name;


        }
         
    }
}