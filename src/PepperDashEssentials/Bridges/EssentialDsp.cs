﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.DM;
using PepperDash.Core;
using PepperDash.Essentials.Core.Routing;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.EthernetCommunication;
using Crestron.SimplSharpPro.DM;

namespace PepperDash.Essentials {
	public class EssentialDsp : PepperDash.Core.Device {
		public EssentialDspProperties Properties;
		public List<BridgeApiEisc> BridgeApiEiscs;
		private PepperDash.Essentials.Devices.Common.DSP.QscDsp Dsp;
		private EssentialDspApiMap ApiMap = new EssentialDspApiMap();
		public EssentialDsp(string key, string name, JToken properties)
			: base(key, name) {
			Properties = JsonConvert.DeserializeObject<EssentialDspProperties>(properties.ToString());


			}
		public override bool CustomActivate() {
			// Create EiscApis 

			try
			{
				ICommunicationMonitor comm = null;
				foreach (var device in DeviceManager.AllDevices) 
				{
					if (device.Key == this.Properties.connectionDeviceKey) 
					{
						if (!(device is ICommunicationMonitor))
						{
							comm = device as ICommunicationMonitor;
						}
						Debug.Console(2, "deviceKey {0} Matches", device.Key);
						Dsp = DeviceManager.GetDeviceForKey(device.Key) as PepperDash.Essentials.Devices.Common.DSP.QscDsp;
						break;
					} 
					else 	
					{
						Debug.Console(2, "deviceKey {0} doesn't match", device.Key);
						
					}
				}
				if (Properties.EiscApiIpids != null && Dsp != null)
				{
					foreach (string Ipid in Properties.EiscApiIpids)
					{
						var ApiEisc = new BridgeApiEisc(Ipid);
						Debug.Console(2, "Connecting EiscApi {0} to {1}", ApiEisc.Ipid, Dsp.Name);
						ushort x = 1;
						if (comm != null)
						{
							comm.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.Online]);
						}
						foreach (var channel in Dsp.LevelControlPoints)
						{
							//var QscChannel = channel.Value as PepperDash.Essentials.Devices.Common.DSP.QscDspLevelControl;
							Debug.Console(2, "QscChannel {0} connect", x);
								
							var genericChannel = channel.Value as IBasicVolumeWithFeedback;
							if (channel.Value.Enabled)
							{
								ApiEisc.Eisc.StringInput[ApiMap.channelName[x]].StringValue = channel.Value.LevelCustomName;
								ApiEisc.Eisc.UShortInput[ApiMap.channelType[x]].UShortValue = (ushort)channel.Value.Type;

								genericChannel.MuteFeedback.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.channelMuteToggle[x]]);
								genericChannel.VolumeLevelFeedback.LinkInputSig(ApiEisc.Eisc.UShortInput[ApiMap.channelVolume[x]]);

								ApiEisc.Eisc.SetSigTrueAction(ApiMap.channelMuteToggle[x], () => genericChannel.MuteToggle());
								ApiEisc.Eisc.SetSigTrueAction(ApiMap.channelMuteOn[x], () => genericChannel.MuteOn());
								ApiEisc.Eisc.SetSigTrueAction(ApiMap.channelMuteOff[x], () => genericChannel.MuteOff());

								ApiEisc.Eisc.SetBoolSigAction(ApiMap.channelVolumeUp[x], b => genericChannel.VolumeUp(b));
								ApiEisc.Eisc.SetBoolSigAction(ApiMap.channelVolumeDown[x], b => genericChannel.VolumeDown(b));

								ApiEisc.Eisc.SetUShortSigAction(ApiMap.channelVolume[x], u => genericChannel.SetVolume(u));
								ApiEisc.Eisc.SetStringSigAction(ApiMap.presetString, s => Dsp.RunPreset(s));
							}
							x++;

						}
						x = 1;
						foreach (var preset in Dsp.PresetList)
						{
							ApiEisc.Eisc.StringInput[ApiMap.presets[x]].StringValue = preset.label;
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.presets[x], () => Dsp.RunPresetNumber(x));
							x++;
						}
						foreach (var dialer in Dsp.Dialers)
						{
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.Keypad0, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num0));
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.Keypad1, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num1));
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.Keypad2, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num2));
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.Keypad3, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num3));
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.Keypad4, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num4));
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.Keypad5, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num5));
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.Keypad6, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num6));
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.Keypad7, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num7));
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.Keypad8, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num8));
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.Keypad9, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num9));
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.KeypadStar, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Star));
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.KeypadPound, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Pound));
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.KeypadClear, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Clear));
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.KeypadBackspace, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Backspace));

							ApiEisc.Eisc.SetSigTrueAction(ApiMap.Dial, () => dialer.Value.Dial());
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.DoNotDisturbToggle, () => dialer.Value.DoNotDisturbToggle());
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.DoNotDisturbOn, () => dialer.Value.DoNotDisturbOn());
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.DoNotDisturbOff, () => dialer.Value.DoNotDisturbOff());
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.AutoAnswerToggle, () => dialer.Value.AutoAnswerToggle());
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.AutoAnswerOn, () => dialer.Value.AutoAnswerOn());
							ApiEisc.Eisc.SetSigTrueAction(ApiMap.AutoAnswerOff, () => dialer.Value.AutoAnswerOff());

							dialer.Value.DoNotDisturbFeedback.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.DoNotDisturbToggle]);
							dialer.Value.DoNotDisturbFeedback.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.DoNotDisturbOn]);
							dialer.Value.DoNotDisturbFeedback.LinkComplementInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.DoNotDisturbOff]);

							dialer.Value.AutoAnswerFeedback.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.AutoAnswerToggle]);
							dialer.Value.AutoAnswerFeedback.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.AutoAnswerOn]);
							dialer.Value.AutoAnswerFeedback.LinkComplementInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.AutoAnswerOff]);

							dialer.Value.OffHookFeedback.LinkInputSig(ApiEisc.Eisc.BooleanInput[ApiMap.Dial]);
							dialer.Value.DialStringFeedback.LinkInputSig(ApiEisc.Eisc.StringInput[ApiMap.DialString]);
						}
					}
				}

				


				Debug.Console(2, "Name {0} Activated", this.Name);
				return true;
				}
			catch (Exception e) {
				Debug.Console(0, "Bridge {0}", e);
				return false;
				}
			}
		}
	public class EssentialDspProperties {
		public string connectionDeviceKey;
		public string[] EiscApiIpids;


		}


	public class EssentialDspApiMap {
		public ushort Online = 1;
		public ushort presetString = 2000;
		public Dictionary<uint, ushort> channelMuteToggle;
		public Dictionary<uint, ushort> channelMuteOn;
		public Dictionary<uint, ushort> channelMuteOff;
		public Dictionary<uint, ushort> channelVolume;
		public Dictionary<uint, ushort> channelType;
		public Dictionary<uint, ushort> channelName;
		public Dictionary<uint, ushort> channelVolumeUp;
		public Dictionary<uint, ushort> channelVolumeDown;
		public Dictionary<uint, ushort> presets;
		public ushort DialString = 3100;
		public ushort Keypad0 = 3110;
		public ushort Keypad1 = 3111;
		public ushort Keypad2 = 3112;
		public ushort Keypad3 = 3113;
		public ushort Keypad4 = 3114;
		public ushort Keypad5 = 3115;
		public ushort Keypad6 = 3116;
		public ushort Keypad7 = 3117;
		public ushort Keypad8 = 3118;
		public ushort Keypad9 = 3119;
		public ushort KeypadStar = 3120;
		public ushort KeypadPound = 3121;
		public ushort KeypadClear = 3122;
		public ushort KeypadBackspace = 3123;
		public ushort Dial = 3124;
		public ushort DoNotDisturbToggle = 3132;
		public ushort DoNotDisturbOn = 3133;
		public ushort DoNotDisturbOff = 3134;
		public ushort AutoAnswerToggle = 3127;
		public ushort AutoAnswerOn = 3125;
		public ushort AutoAnswerOff = 3126;

		public EssentialDspApiMap() {
			channelMuteToggle = new Dictionary<uint, ushort>();
			channelMuteOn = new Dictionary<uint, ushort>();
			channelMuteOff = new Dictionary<uint, ushort>();
			channelVolume = new Dictionary<uint, ushort>();
			channelName = new Dictionary<uint, ushort>();
			channelType = new Dictionary<uint, ushort>();
			presets = new Dictionary<uint, ushort>();
			channelVolumeUp = new Dictionary<uint, ushort>();
			channelVolumeDown = new Dictionary<uint, ushort>();
			for (uint x = 1; x <= 100; x++) {
				uint tempNum = x;
				presets[tempNum] = (ushort)(tempNum + 100);
				channelMuteToggle[tempNum] = (ushort)(tempNum + 400);
				channelMuteOn[tempNum] = (ushort)(tempNum + 600);
				channelMuteOff[tempNum] = (ushort)(tempNum + 800);
				channelVolume[tempNum] = (ushort)(tempNum + 200);
				channelName[tempNum] = (ushort)(tempNum + 200);
				channelType[tempNum] = (ushort)(tempNum + 400);
				channelVolumeUp[tempNum] = (ushort)(tempNum + 1000);
				channelVolumeDown[tempNum] = (ushort)(tempNum + 1200);
				}
			}
		}
	}
	