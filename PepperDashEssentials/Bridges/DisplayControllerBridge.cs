using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;

namespace PepperDash.Essentials.Bridges
{
	public static class DisplayControllerApiExtensions
	{
		public static void LinkToApi(this PepperDash.Essentials.Core.TwoWayDisplayBase displayDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
		{
			var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as DisplayControllerJoinMap;

			if (joinMap == null)
			{
				joinMap = new DisplayControllerJoinMap();
			}
			
			joinMap.OffsetJoinNumbers(joinStart);
			Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			Debug.Console(0, "Linking to Bridge Type {0}", displayDevice.GetType().Name.ToString());

			var commMonitor = displayDevice as ICommunicationMonitor;
			commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
			

			// Poewer Off
			trilist.SetSigTrueAction(joinMap.PowerOff, () => displayDevice.PowerOff());
			displayDevice.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PowerOff]);

			// Poewer On
			trilist.SetSigTrueAction(joinMap.PowerOn, () => displayDevice.PowerOn());
			displayDevice.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn]);


			trilist.SetUShortSigAction(joinMap.InputSelect, (a) =>
			{
				if (a == 0)
				{
					displayDevice.PowerOff();
				}
				else if (a > 0 && a < displayDevice.InputPorts.Count)
				{
					displayDevice.ExecuteSwitch(displayDevice.InputPorts.ElementAt(a - 1).Selector);
				}
			});
			// GenericLighitng Actions & FeedBack

			// int sceneIndex = 1;
			/*
			foreach (var scene in displayDevice.LightingScenes)
			{
				var tempIndex = sceneIndex - 1;
				//trilist.SetSigTrueAction((uint)(joinMap.LightingSceneOffset + sceneIndex), () => displayDevice.SelectScene(displayDevice.LightingScenes[tempIndex]));
				scene.IsActiveFeedback.LinkInputSig(trilist.BooleanInput[(uint)(joinMap.LightingSceneOffset + sceneIndex)]);
				trilist.StringInput[(uint)(joinMap.LightingSceneOffset + sceneIndex)].StringValue = scene.Name;
				trilist.BooleanInput[(uint)(joinMap.ButtonVisibilityOffset + sceneIndex)].BoolValue = true;
				sceneIndex++;
			}

			if (displayDevice.GetType().Name.ToString() == "LutronQuantumArea")
			{
				var lutronDevice = displayDevice as PepperDash.Essentials.Devices.Common.Environment.Lutron.LutronQuantumArea;
				lutronDevice.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
				trilist.SetStringSigAction(joinMap.IntegrationIdSet, s => lutronDevice.IntegrationId = s);
			}
			*/ 
			//ApiEisc.Eisc.SetStringSigAction(ApiMap.integrationID, (s) => { lutronLights.IntegrationId = s; });


			/*
			var lutronLights = displayDevice as PepperDash.Essentials.Devices.Common.Environment.Lutron.LutronQuantumArea;

			
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
	public class DisplayControllerJoinMap : JoinMapBase
	{
		public uint IsOnline { get; set; }
		public uint PowerOff { get; set; }
		public uint InputSelect { get; set; }
		public uint PowerOn { get; set; }
		public uint SelectScene { get; set; }
		public uint LightingSceneOffset { get; set; }
		public uint ButtonVisibilityOffset { get; set; }
		public uint IntegrationIdSet { get; set; }

		public DisplayControllerJoinMap()
		{
			// Digital
			IsOnline = 1;
			PowerOff = 1;
			PowerOn = 2;
			InputSelect = 1;
			IntegrationIdSet = 1;
			LightingSceneOffset = 10;
			ButtonVisibilityOffset = 40;
			// Analog
		}

		public override void OffsetJoinNumbers(uint joinStart)
		{
			var joinOffset = joinStart - 1;

			IsOnline = IsOnline + joinOffset;
			PowerOff = PowerOff + joinOffset;
			PowerOn = PowerOn + joinOffset;
			SelectScene = SelectScene + joinOffset;
			LightingSceneOffset = LightingSceneOffset + joinOffset;
			ButtonVisibilityOffset = ButtonVisibilityOffset + joinOffset;
			


		}
	}
}