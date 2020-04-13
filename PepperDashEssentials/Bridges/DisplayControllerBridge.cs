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


namespace PepperDash.Essentials.Core.Bridges
{
	public static class DisplayControllerApiExtensions
	{
		public static void LinkToApi(this PepperDash.Essentials.Core.DisplayBase displayDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
		{
            int inputNumber = 0;
            IntFeedback inputNumberFeedback;
            List<string> inputKeys = new List<string>();

            DisplayControllerJoinMap joinMap = new DisplayControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);
            
            if(!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DisplayControllerJoinMap>(joinMapSerialized);

			joinMap.OffsetJoinNumbers(joinStart);

			Debug.Console(1, "Linking to Trilist '{0}'",trilist.ID.ToString("X"));
			Debug.Console(0, "Linking to Display: {0}", displayDevice.Name);

            trilist.StringInput[joinMap.Name].StringValue = displayDevice.Name;			

			var commMonitor = displayDevice as ICommunicationMonitor;
            if (commMonitor != null)
            {
                commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            }

            inputNumberFeedback = new IntFeedback(() => { return inputNumber; });

            // Two way feedbacks
            var twoWayDisplay = displayDevice as PepperDash.Essentials.Core.TwoWayDisplayBase;
            if (twoWayDisplay != null)
            {
                trilist.SetBool(joinMap.IsTwoWayDisplay, true);

                twoWayDisplay.CurrentInputFeedback.OutputChange += new EventHandler<FeedbackEventArgs>(CurrentInputFeedback_OutputChange);


                inputNumberFeedback.LinkInputSig(trilist.UShortInput[joinMap.InputSelect]);
            }

			// Power Off
			trilist.SetSigTrueAction(joinMap.PowerOff, () =>
				{
					inputNumber = 102;
					inputNumberFeedback.FireUpdate();
					displayDevice.PowerOff();
				});

			displayDevice.PowerIsOnFeedback.OutputChange += new EventHandler<FeedbackEventArgs>( (o,a) => {
                if (!a.BoolValue)
                {
                    inputNumber = 102;
                    inputNumberFeedback.FireUpdate();

                }
                else
                {
                    inputNumber = 0;
                    inputNumberFeedback.FireUpdate();
                }
            });

			displayDevice.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PowerOff]);

			// PowerOn
			trilist.SetSigTrueAction(joinMap.PowerOn, () =>
				{
					inputNumber = 0;
					inputNumberFeedback.FireUpdate();
					displayDevice.PowerOn();
				});

			
			displayDevice.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn]);

			int count = 1;
			foreach (var input in displayDevice.InputPorts)
			{
				inputKeys.Add(input.Key.ToString());
				var tempKey = inputKeys.ElementAt(count - 1);
				trilist.SetSigTrueAction((ushort)(joinMap.InputSelectOffset + count), () => { displayDevice.ExecuteSwitch(displayDevice.InputPorts[tempKey].Selector); });
                Debug.Console(2, displayDevice, "Setting Input Select Action on Digital Join {0} to Input: {1}", joinMap.InputSelectOffset + count, displayDevice.InputPorts[tempKey].Key.ToString());
				trilist.StringInput[(ushort)(joinMap.InputNamesOffset + count)].StringValue = input.Key.ToString();
				count++;
			}

            Debug.Console(2, displayDevice, "Setting Input Select Action on Analog Join {0}", joinMap.InputSelect);
			trilist.SetUShortSigAction(joinMap.InputSelect, (a) =>
			{
				if (a == 0)
				{
					displayDevice.PowerOff();
					inputNumber = 0;
				}
				else if (a > 0 && a < displayDevice.InputPorts.Count && a != inputNumber)
				{
					displayDevice.ExecuteSwitch(displayDevice.InputPorts.ElementAt(a - 1).Selector);
					inputNumber = a;
				}
				else if (a == 102)
				{
					displayDevice.PowerToggle();

				}
                if (twoWayDisplay != null)
				    inputNumberFeedback.FireUpdate();
			});


            var volumeDisplay = displayDevice as IBasicVolumeControls;
            if (volumeDisplay != null)
            {
                trilist.SetBoolSigAction(joinMap.VolumeUp, (b) => volumeDisplay.VolumeUp(b));
                trilist.SetBoolSigAction(joinMap.VolumeDown, (b) => volumeDisplay.VolumeDown(b));
                trilist.SetSigTrueAction(joinMap.VolumeMute, () => volumeDisplay.MuteToggle());

                var volumeDisplayWithFeedback = volumeDisplay as IBasicVolumeWithFeedback;
                if(volumeDisplayWithFeedback != null)
                {
                    trilist.SetUShortSigAction(joinMap.VolumeLevel, new Action<ushort>((u) => volumeDisplayWithFeedback.SetVolume(u)));
                    volumeDisplayWithFeedback.VolumeLevelFeedback.LinkInputSig(trilist.UShortInput[joinMap.VolumeLevel]);
                    volumeDisplayWithFeedback.MuteFeedback.LinkInputSig(trilist.BooleanInput[joinMap.VolumeMute]);
                }
            }
		}

		static void CurrentInputFeedback_OutputChange(object sender, FeedbackEventArgs e)
		{

			Debug.Console(0, "CurrentInputFeedback_OutputChange {0}", e.StringValue);

		}

	}
   
}