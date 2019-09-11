using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Bridges
{
    public static class GenericLightingApiExtensions
    {
        public static void LinkToApi(this PepperDash.Essentials.Core.Lighting.LightingBase lightingDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            GenericLightingJoinMap joinMap = new GenericLightingJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<GenericLightingJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            Debug.Console(0, "Linking to Lighting Type {0}", lightingDevice.GetType().Name.ToString());

            // GenericLighitng Actions & FeedBack
            trilist.SetUShortSigAction(joinMap.SelectScene, u => lightingDevice.SelectScene(lightingDevice.LightingScenes[u]));

            int sceneIndex = 1;
            foreach (var scene in lightingDevice.LightingScenes)
            {
                var tempIndex = sceneIndex - 1;
                trilist.SetSigTrueAction((uint)(joinMap.LightingSceneOffset + sceneIndex), () => lightingDevice.SelectScene(lightingDevice.LightingScenes[tempIndex]));
                scene.IsActiveFeedback.LinkInputSig(trilist.BooleanInput[(uint)(joinMap.LightingSceneOffset + sceneIndex)]);
                trilist.StringInput[(uint)(joinMap.LightingSceneOffset + sceneIndex)].StringValue = scene.Name;
                trilist.BooleanInput[(uint)(joinMap.ButtonVisibilityOffset + sceneIndex)].BoolValue = true;
                sceneIndex++;
            }

            if (lightingDevice.GetType().Name.ToString() == "LutronQuantumArea")
            {
                var lutronDevice = lightingDevice as PepperDash.Essentials.Devices.Common.Environment.Lutron.LutronQuantumArea;
                lutronDevice.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
                trilist.SetStringSigAction(joinMap.IntegrationIdSet, s => lutronDevice.IntegrationId = s);
            }

            //ApiEisc.Eisc.SetStringSigAction(ApiMap.integrationID, (s) => { lutronLights.IntegrationId = s; });


            /*
            var lutronLights = lightingDevice as PepperDash.Essentials.Devices.Common.Environment.Lutron.LutronQuantumArea;

			
            for (uint i = 1; i <= lightingBase.CircuitCount; i++)
            {
                var circuit = i;
                lightingBase.CircuitNameFeedbacks[circuit - 1].LinkInputSig(trilist.StringInput[joinMap.CircuitNames + circuit]);
                lightingBase.CircuitIsCritical[circuit - 1].LinkInputSig(trilist.BooleanInput[joinMap.CircuitIsCritical + circuit]);
                lightingBase.CircuitState[circuit - 1].LinkInputSig(trilist.BooleanInput[joinMap.CircuitState + circuit]);
                trilist.SetSigTrueAction(joinMap.CircuitCycle + circuit, () => lightingBase.CycleCircuit(circuit - 1));
                trilist.SetSigTrueAction(joinMap.CircuitOnCmd + circuit, () => lightingBase.TurnOnCircuit(circuit - 1));
                trilist.SetSigTrueAction(joinMap.CircuitOffCmd + circuit, () => lightingBase.TurnOffCircuit(circuit - 1));

            }
             */
        }
    }
}