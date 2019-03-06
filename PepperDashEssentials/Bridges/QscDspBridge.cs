using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.Devices.Common.DSP;

namespace PepperDash.Essentials.Bridges
{
	public static class QscDspDeviceApiExtensions
	{
		public static void LinkToApi(this QscDsp DspDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
		{
			var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as QscDspDeviceJoinMap;

			if (joinMap == null)
				joinMap = new QscDspDeviceJoinMap();

			joinMap.OffsetJoinNumbers(joinStart);
			Debug.Console(1, DspDevice, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			ushort x = 1;
			var comm = DspDevice as ICommunicationMonitor;
			DspDevice.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
			foreach (var channel in DspDevice.LevelControlPoints)
			{
				//var QscChannel = channel.Value as PepperDash.Essentials.Devices.Common.DSP.QscDspLevelControl;
				Debug.Console(2, "QscChannel {0} connect", x);

				var genericChannel = channel.Value as IBasicVolumeWithFeedback;
				if (channel.Value.Enabled)
				{
					trilist.StringInput[joinMap.ChannelName + x].StringValue = channel.Value.LevelCustomName;
					trilist.UShortInput[joinMap.ChannelType + x].UShortValue = (ushort)channel.Value.Type;

					genericChannel.MuteFeedback.LinkInputSig(trilist.BooleanInput[joinMap.ChannelMuteToggle + x]);
					genericChannel.VolumeLevelFeedback.LinkInputSig(trilist.UShortInput[joinMap.ChannelVolume + x]);

					trilist.SetSigTrueAction(joinMap.ChannelMuteToggle + x, () => genericChannel.MuteToggle());
					trilist.SetSigTrueAction(joinMap.ChannelMuteOn + x, () => genericChannel.MuteOn());
					trilist.SetSigTrueAction(joinMap.ChannelMuteOff + x, () => genericChannel.MuteOff());

					trilist.SetBoolSigAction(joinMap.ChannelVolumeUp + x, b => genericChannel.VolumeUp(b));
					trilist.SetBoolSigAction(joinMap.ChannelVolumeDown + x, b => genericChannel.VolumeDown(b));

					trilist.SetUShortSigAction(joinMap.ChannelVolume + x, u => genericChannel.SetVolume(u));
					
				}
				x++;
			}
			x = 1;
			trilist.SetStringSigAction(joinMap.PresetStringCmd, s => DspDevice.RunPreset(s));
			foreach (var preset in DspDevice.PresetList)
			{
				trilist.StringInput[joinMap.Presets + x].StringValue = preset.label;
				trilist.SetSigTrueAction(joinMap.Presets + x, () => DspDevice.RunPresetNumber(x));
				x++;
			}
			foreach (var dialer in DspDevice.Dialers)
			{
				trilist.SetSigTrueAction(joinMap.Keypad0, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num0));
				trilist.SetSigTrueAction(joinMap.Keypad1, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num1));
				trilist.SetSigTrueAction(joinMap.Keypad2, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num2));
				trilist.SetSigTrueAction(joinMap.Keypad3, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num3));
				trilist.SetSigTrueAction(joinMap.Keypad4, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num4));
				trilist.SetSigTrueAction(joinMap.Keypad5, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num5));
				trilist.SetSigTrueAction(joinMap.Keypad6, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num6));
				trilist.SetSigTrueAction(joinMap.Keypad7, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num7));
				trilist.SetSigTrueAction(joinMap.Keypad8, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num8));
				trilist.SetSigTrueAction(joinMap.Keypad9, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Num9));
				trilist.SetSigTrueAction(joinMap.KeypadStar, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Star));
				trilist.SetSigTrueAction(joinMap.KeypadPound, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Pound));
				trilist.SetSigTrueAction(joinMap.KeypadClear, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Clear));
				trilist.SetSigTrueAction(joinMap.KeypadBackspace, () => dialer.Value.SendKeypad(PepperDash.Essentials.Devices.Common.DSP.QscDspDialer.eKeypadKeys.Backspace));

				trilist.SetSigTrueAction(joinMap.Dial, () => dialer.Value.Dial());
				trilist.SetSigTrueAction(joinMap.DoNotDisturbToggle, () => dialer.Value.DoNotDisturbToggle());
				trilist.SetSigTrueAction(joinMap.DoNotDisturbOn, () => dialer.Value.DoNotDisturbOn());
				trilist.SetSigTrueAction(joinMap.DoNotDisturbOff, () => dialer.Value.DoNotDisturbOff());
				trilist.SetSigTrueAction(joinMap.AutoAnswerToggle, () => dialer.Value.AutoAnswerToggle());
				trilist.SetSigTrueAction(joinMap.AutoAnswerOn, () => dialer.Value.AutoAnswerOn());
				trilist.SetSigTrueAction(joinMap.AutoAnswerOff, () => dialer.Value.AutoAnswerOff());

				dialer.Value.DoNotDisturbFeedback.LinkInputSig(trilist.BooleanInput[joinMap.DoNotDisturbToggle]);
				dialer.Value.DoNotDisturbFeedback.LinkInputSig(trilist.BooleanInput[joinMap.DoNotDisturbOn]);
				dialer.Value.DoNotDisturbFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.DoNotDisturbOff]);

				dialer.Value.AutoAnswerFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AutoAnswerToggle]);
				dialer.Value.AutoAnswerFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AutoAnswerOn]);
				dialer.Value.AutoAnswerFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.AutoAnswerOff]);

				dialer.Value.OffHookFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Dial]);
				dialer.Value.DialStringFeedback.LinkInputSig(trilist.StringInput[joinMap.DialStringCmd]);
			}

			

		}
	}
	public class QscDspDeviceJoinMap : JoinMapBase
	{
		public uint IsOnline { get; set; }
		public uint PresetStringCmd { get; set; }
		public uint ChannelMuteToggle { get; set; }
		public uint ChannelMuteOn { get; set; }
		public uint ChannelMuteOff { get; set; }
		public uint ChannelVolume { get; set; }
		public uint ChannelType { get; set; }
		public uint ChannelName { get; set; }
		public uint ChannelVolumeUp { get; set; }
		public uint ChannelVolumeDown { get; set; }
		public uint Presets { get; set; }
		public uint DialStringCmd { get; set; }
		public uint Keypad0 { get; set; }
		public uint Keypad1 { get; set; }
		public uint Keypad2 { get; set; }
		public uint Keypad3 { get; set; }
		public uint Keypad4 { get; set; }
		public uint Keypad5 { get; set; }
		public uint Keypad6 { get; set; }
		public uint Keypad7 { get; set; }
		public uint Keypad8 { get; set; }
		public uint Keypad9 { get; set; }
		public uint KeypadStar { get; set; }
		public uint KeypadPound { get; set; }
		public uint KeypadClear { get; set; }
		public uint KeypadBackspace { get; set; }
		public uint Dial { get; set; }
		public uint DoNotDisturbToggle { get; set; }
		public uint DoNotDisturbOn { get; set; }
		public uint DoNotDisturbOff { get; set; }
		public uint AutoAnswerToggle { get; set; }
		public uint AutoAnswerOn { get; set; }
		public uint AutoAnswerOff { get; set; }

		public uint CallPreset { get; set; }
		public uint PresetFeedback { get; set; }

		public QscDspDeviceJoinMap()
		{
			// Arrays
			ChannelName = 200;
			ChannelMuteToggle = 400;
			ChannelMuteOn = 600; 
			ChannelMuteOff = 800;
			ChannelVolume = 200;
			ChannelVolumeUp = 1000;
			ChannelVolumeDown = 1200; 
			ChannelType = 400;
			Presets = 100; 

			// SIngleJoins
			IsOnline = 1;
			PresetStringCmd = 2000;
			DialStringCmd = 3100;
			Keypad0 = 3110;
			Keypad1 = 3111; 
			Keypad2 = 3112;
			Keypad3 = 3113; 
			Keypad4 = 3114;
			Keypad5 = 3115;
			Keypad6 = 3116;
			Keypad7 = 3117;
			Keypad8 = 3118;
			Keypad9 = 3119;
			KeypadStar = 3120;
			KeypadPound = 3121;
			KeypadClear = 3122;
			KeypadBackspace = 3123;
			DoNotDisturbToggle = 3132;
			DoNotDisturbOn = 3133;
			DoNotDisturbOff = 3134;
			AutoAnswerToggle = 3127;
			AutoAnswerOn = 3125;
			AutoAnswerOff = 3126; 
			Dial = 3124;


		}

		public override void OffsetJoinNumbers(uint joinStart)
		{
			var joinOffset = joinStart - 1;
			ChannelName = ChannelName + joinOffset; 
			ChannelMuteToggle = ChannelMuteToggle + joinOffset;
			ChannelMuteOn = ChannelMuteOn + joinOffset;
			ChannelMuteOff = ChannelMuteOff + joinOffset;
			ChannelVolume = ChannelVolume + joinOffset;
			ChannelVolumeUp = ChannelVolumeUp + joinOffset;
			ChannelVolumeDown = ChannelVolumeDown + joinOffset;
			ChannelType = ChannelType + joinOffset;
			Presets = Presets + joinOffset;

			IsOnline = IsOnline + joinOffset; 
			PresetStringCmd = PresetStringCmd + joinOffset;
			DialStringCmd = DialStringCmd + joinOffset;
			Keypad0 = Keypad0 + joinOffset;
			Keypad1 = Keypad1 + joinOffset;
			Keypad2 = Keypad2 + joinOffset;
			Keypad3 = Keypad3 + joinOffset;
			Keypad4 = Keypad4 + joinOffset;
			Keypad5 = Keypad5 + joinOffset;
			Keypad6 = Keypad6 + joinOffset; 
			Keypad7 = Keypad7 + joinOffset; 
			Keypad8 = Keypad8 + joinOffset; 
			Keypad9 = Keypad9 + joinOffset; 
			KeypadStar = KeypadStar + joinOffset;
			KeypadPound = KeypadPound + joinOffset;
			KeypadClear = KeypadClear + joinOffset;
			KeypadBackspace = KeypadBackspace + joinOffset;
			DoNotDisturbToggle = DoNotDisturbToggle + joinOffset;
			DoNotDisturbOn = DoNotDisturbOn + joinOffset;
			DoNotDisturbOff = DoNotDisturbOff + joinOffset;
			AutoAnswerToggle = AutoAnswerToggle + joinOffset;
			AutoAnswerOn = AutoAnswerOn + joinOffset;
			AutoAnswerOff = AutoAnswerOff + joinOffset;
			Dial = Dial + joinOffset;
		}
	}

}