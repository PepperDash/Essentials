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

		public static BasicTriList _TriList;
		public static DisplayControllerJoinMap JoinMap;
		public static int InputNumber;
		public static IntFeedback InputNumberFeedback;
		public static List<string> InputKeys = new List<string>();
		public static void LinkToApi(this PepperDash.Essentials.Core.TwoWayDisplayBase displayDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
		{


				JoinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as DisplayControllerJoinMap;
				_TriList = trilist;

				if (JoinMap == null)
				{
					JoinMap = new DisplayControllerJoinMap();
				}

				JoinMap.OffsetJoinNumbers(joinStart);
				Debug.Console(1, "Linking to Trilist '{0}'", _TriList.ID.ToString("X"));
				Debug.Console(0, "Linking to Bridge Type {0}", displayDevice.GetType().Name.ToString());

				_TriList.StringInput[JoinMap.Name].StringValue = displayDevice.GetType().Name.ToString();
			
				InputNumberFeedback = new IntFeedback(() => { return InputNumber; });
				InputNumberFeedback.LinkInputSig(_TriList.UShortInput[JoinMap.InputSelect]);
				var commMonitor = displayDevice as ICommunicationMonitor;
				commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[JoinMap.IsOnline]);

				// Poewer Off
				trilist.SetSigTrueAction(JoinMap.PowerOff, () =>
					{
						InputNumber = 102;
						InputNumberFeedback.FireUpdate();
						displayDevice.PowerOff();
					});

				displayDevice.PowerIsOnFeedback.OutputChange += new EventHandler<FeedbackEventArgs>(PowerIsOnFeedback_OutputChange);
				displayDevice.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[JoinMap.PowerOff]);

				// PowerOn
				trilist.SetSigTrueAction(JoinMap.PowerOn, () =>
					{
						InputNumber = 0;
						InputNumberFeedback.FireUpdate();
						displayDevice.PowerOn();
					});

				
				displayDevice.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[JoinMap.PowerOn]);

				int count = 1;
				foreach (var input in displayDevice.InputPorts)
				{
					InputKeys.Add(input.Key.ToString());
					var tempKey = InputKeys.ElementAt(count - 1);
					trilist.SetSigTrueAction((ushort)(JoinMap.InputSelectOffset + count), () => { displayDevice.ExecuteSwitch(displayDevice.InputPorts[tempKey].Selector); });
					trilist.StringInput[(ushort)(JoinMap.InputNamesOffset + count)].StringValue = input.Key.ToString();
					count++;
				}

				displayDevice.CurrentInputFeedback.OutputChange += new EventHandler<FeedbackEventArgs>(CurrentInputFeedback_OutputChange);
				trilist.SetUShortSigAction(JoinMap.InputSelect, (a) =>
				{
					if (a == 0)
					{
						displayDevice.PowerOff();
						InputNumber = 0;
					}
					else if (a > 0 && a < displayDevice.InputPorts.Count && a != InputNumber)
					{
						displayDevice.ExecuteSwitch(displayDevice.InputPorts.ElementAt(a - 1).Selector);
						InputNumber = a;
					}
					else if (a == 102)
					{
						displayDevice.PowerToggle();

					}
					InputNumberFeedback.FireUpdate();
				});
				 

			}

		static void CurrentInputFeedback_OutputChange(object sender, FeedbackEventArgs e)
		{

			Debug.Console(0, "CurrentInputFeedback_OutputChange {0}", e.StringValue);

		}

		static void PowerIsOnFeedback_OutputChange(object sender, FeedbackEventArgs e)
		{

			// Debug.Console(0, "PowerIsOnFeedback_OutputChange {0}",  e.BoolValue);
			if (!e.BoolValue)
			{
				InputNumber = 102;
				InputNumberFeedback.FireUpdate();

			}
			else
			{
				InputNumber = 0;
				InputNumberFeedback.FireUpdate();
			}
		}




	}
	public class DisplayControllerJoinMap : JoinMapBase
	{
		public uint Name { get; set; }
		public uint InputNamesOffset { get; set; }
		public uint InputSelectOffset { get; set; }
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
			IsOnline = 50;
			PowerOff = 1;
			PowerOn = 2;
			InputSelect = 4;
			IntegrationIdSet = 1;
			LightingSceneOffset = 10;
			ButtonVisibilityOffset = 40;
			Name = 1;
			InputNamesOffset = 10;
			InputSelectOffset = 4;
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
			Name = Name + joinOffset;
			InputNamesOffset = InputNamesOffset + joinOffset;
			InputSelectOffset = InputSelectOffset + joinOffset;

		}
	}
}