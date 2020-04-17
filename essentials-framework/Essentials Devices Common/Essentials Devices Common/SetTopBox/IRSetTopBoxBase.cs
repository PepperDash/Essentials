using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Presets;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Devices.Common
{
	public class IRSetTopBoxBase : EssentialsBridgeableDevice, ISetTopBoxControls, IRoutingOutputs, IUsageTracking, IPower
	{
		public IrOutputPortController IrPort { get; private set; }

		public uint DisplayUiType { get { return DisplayUiConstants.TypeDirecTv; } }


        public bool HasPresets { get; set; }
        public bool HasDvr { get; set; }
        public bool HasDpad { get; set; }
        public bool HasNumeric { get; set; }

        public DevicePresetsModel PresetsModel { get; private set; }

		public IRSetTopBoxBase(string key, string name, IrOutputPortController portCont,
            SetTopBoxPropertiesConfig props)
			: base(key, name)
		{
			IrPort = portCont;
			DeviceManager.AddDevice(portCont);

            HasPresets = props.HasPresets;
            HasDvr = props.HasDvr;
            HasDpad = props.HasDpad;
            HasNumeric = props.HasNumeric;

			HasKeypadAccessoryButton1 = true;
			KeypadAccessoryButton1Command = "Dash";
			KeypadAccessoryButton1Label = "-";

			HasKeypadAccessoryButton2 = true;
			KeypadAccessoryButton2Command = "NumericEnter";
			KeypadAccessoryButton2Label = "Enter";

			AnyVideoOut = new RoutingOutputPort(RoutingPortNames.AnyVideoOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Hdmi, null, this);
			AnyAudioOut = new RoutingOutputPort(RoutingPortNames.AnyAudioOut, eRoutingSignalType.Audio,
				eRoutingPortConnectionType.DigitalAudio, null, this);
			OutputPorts = new RoutingPortCollection<RoutingOutputPort> { AnyVideoOut, AnyAudioOut };

		}

		public void LoadPresets(string filePath)
		{
			PresetsModel = new DevicePresetsModel(Key + "-presets", this, filePath);
			DeviceManager.AddDevice(PresetsModel);
		}


		#region ISetTopBoxControls Members

		public void DvrList(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_DVR, pressRelease);
		}

		public void Replay(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_REPLAY, pressRelease);
		}

		#endregion

		#region IDPad Members

		public void Up(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_UP_ARROW, pressRelease);
		}

		public void Down(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_DN_ARROW, pressRelease);
		}

		public void Left(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_LEFT_ARROW, pressRelease);
		}

		public void Right(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_RIGHT_ARROW, pressRelease);
		}

		public void Select(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_ENTER, pressRelease);
		}

		public void Menu(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_MENU, pressRelease);
		}

		public void Exit(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_EXIT, pressRelease);
		}

		#endregion

		#region INumericKeypad Members

		public void Digit0(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_0, pressRelease);
		}

		public void Digit1(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_1, pressRelease);
		}

		public void Digit2(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_2, pressRelease);
		}

		public void Digit3(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_3, pressRelease);
		}

		public void Digit4(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_4, pressRelease);
		}

		public void Digit5(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_5, pressRelease);
		}

		public void Digit6(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_6, pressRelease);
		}

		public void Digit7(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_7, pressRelease);
		}

		public void Digit8(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_8, pressRelease);
		}

		public void Digit9(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_9, pressRelease);
		}

		/// <summary>
		/// Defaults to true
		/// </summary>
		public bool HasKeypadAccessoryButton1 { get; set; }

		/// <summary>
		/// Defaults to "-"
		/// </summary>
		public string KeypadAccessoryButton1Label { get; set; }
		

		/// <summary>
		/// Defaults to "Dash"
		/// </summary>
		public string KeypadAccessoryButton1Command { get; set; }

		public void KeypadAccessoryButton1(bool pressRelease)
		{
			IrPort.PressRelease(KeypadAccessoryButton1Command, pressRelease);
		}

		/// <summary>
		/// Defaults to true
		/// </summary>
		public bool HasKeypadAccessoryButton2 { get; set; }

		/// <summary>
		/// Defaults to "Enter"
		/// </summary>
		public string KeypadAccessoryButton2Label { get; set; }


		/// <summary>
		/// Defaults to "Enter"
		/// </summary>
		public string KeypadAccessoryButton2Command { get; set; }

		public void KeypadAccessoryButton2(bool pressRelease)
		{
			IrPort.PressRelease(KeypadAccessoryButton2Command, pressRelease);
		}

		#endregion

		#region ISetTopBoxNumericKeypad Members

		/// <summary>
		/// Corresponds to "dash" IR command
		/// </summary>
		public void Dash(bool pressRelease)
		{
			IrPort.PressRelease("dash", pressRelease);
		}

		/// <summary>
		/// Corresponds to "numericEnter" IR command
		/// </summary>
		public void KeypadEnter(bool pressRelease)
		{
			IrPort.PressRelease("numericEnter", pressRelease);
		}

		#endregion

		#region IChannelFunctions Members

		public void ChannelUp(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_CH_PLUS, pressRelease);
		}

		public void ChannelDown(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_CH_MINUS, pressRelease);
		}

		public void LastChannel(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_LAST, pressRelease);
		}

		public void Guide(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_GUIDE, pressRelease);
		}

		public void Info(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_INFO, pressRelease);
		}

		#endregion

		#region IColorFunctions Members

		public void Red(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_RED, pressRelease);
		}

		public void Green(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_GREEN, pressRelease);
		}

		public void Yellow(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_YELLOW, pressRelease);
		}

		public void Blue(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_BLUE, pressRelease);
		}

		#endregion

		#region IRoutingOutputs Members

		public RoutingOutputPort AnyVideoOut { get; private set; }
		public RoutingOutputPort AnyAudioOut { get; private set; }
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		#endregion

		#region ITransport Members

		public void ChapMinus(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_REPLAY, pressRelease);
		}

		public void ChapPlus(bool pressRelease)
		{
		}

		public void FFwd(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_FSCAN, pressRelease);
		}

		public void Pause(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_RSCAN, pressRelease);
		}

		public void Play(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_PLAY, pressRelease);
		}

		public void Record(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_RECORD, pressRelease);
		}

		public void Rewind(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_RSCAN, pressRelease);
		}

		public void Stop(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_STOP, pressRelease);
		}

		#endregion

        #region IUsageTracking Members

        public UsageTracking UsageTracker { get; set; }

        #endregion

        #region IPower Members

        public void PowerOn()
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_POWER_ON, true);
            IrPort.PressRelease(IROutputStandardCommands.IROut_POWER_ON, false);

        }

        public void PowerOff()
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_POWER_OFF, true);
            IrPort.PressRelease(IROutputStandardCommands.IROut_POWER_OFF, false);

        }

        public void PowerToggle()
        {
            throw new NotImplementedException(); 
        }

        public BoolFeedback PowerIsOnFeedback
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

	    public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApi bridge)
	    {
            var joinMap = new SetTopBoxControllerJoinMap();
            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<SetTopBoxControllerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to Display: {0}", Name);

            trilist.StringInput[joinMap.Name].StringValue = Name;

            var stbBase = this as ISetTopBoxControls;

	        trilist.BooleanInput[joinMap.HasDpad].BoolValue = stbBase.HasDpad;
	        trilist.BooleanInput[joinMap.HasNumeric].BoolValue = stbBase.HasNumeric;
	        trilist.BooleanInput[joinMap.HasDvr].BoolValue = stbBase.HasDvr;
	        trilist.BooleanInput[joinMap.HasPresets].BoolValue = stbBase.HasPresets;

	        trilist.SetBoolSigAction(joinMap.DvrList, stbBase.DvrList);
	        trilist.SetBoolSigAction(joinMap.Replay, stbBase.Replay);

	        trilist.SetStringSigAction(joinMap.LoadPresets, stbBase.LoadPresets);

	        var stbPower = this as IPower;

	        trilist.SetSigTrueAction(joinMap.PowerOn, stbPower.PowerOn);
	        trilist.SetSigTrueAction(joinMap.PowerOff, stbPower.PowerOff);
	        trilist.SetSigTrueAction(joinMap.PowerToggle, stbPower.PowerToggle);

	        var stbDPad = this as IDPad;

	        trilist.SetBoolSigAction(joinMap.Up, stbDPad.Up);
	        trilist.SetBoolSigAction(joinMap.Down, stbDPad.Down);
	        trilist.SetBoolSigAction(joinMap.Left, stbDPad.Left);
	        trilist.SetBoolSigAction(joinMap.Right, stbDPad.Right);
	        trilist.SetBoolSigAction(joinMap.Select, stbDPad.Select);
	        trilist.SetBoolSigAction(joinMap.Menu, stbDPad.Menu);
	        trilist.SetBoolSigAction(joinMap.Exit, stbDPad.Exit);

	        var stbChannel = this as IChannel;
	        trilist.SetBoolSigAction(joinMap.ChannelUp, stbChannel.ChannelUp);
	        trilist.SetBoolSigAction(joinMap.ChannelDown, stbChannel.ChannelDown);
	        trilist.SetBoolSigAction(joinMap.LastChannel, stbChannel.LastChannel);
	        trilist.SetBoolSigAction(joinMap.Guide, stbChannel.Guide);
	        trilist.SetBoolSigAction(joinMap.Info, stbChannel.Info);
	        trilist.SetBoolSigAction(joinMap.Exit, stbChannel.Exit);

	        var stbColor = this as IColor;
	        trilist.SetBoolSigAction(joinMap.Red, stbColor.Red);
	        trilist.SetBoolSigAction(joinMap.Green, stbColor.Green);
	        trilist.SetBoolSigAction(joinMap.Yellow, stbColor.Yellow);
	        trilist.SetBoolSigAction(joinMap.Blue, stbColor.Blue);

	        var stbKeypad = this as ISetTopBoxNumericKeypad;

	        trilist.StringInput[joinMap.KeypadAccessoryButton1Label].StringValue = stbKeypad.KeypadAccessoryButton1Label;
	        trilist.StringInput[joinMap.KeypadAccessoryButton2Label].StringValue = stbKeypad.KeypadAccessoryButton2Label;

	        trilist.BooleanInput[joinMap.HasKeypadAccessoryButton1].BoolValue = stbKeypad.HasKeypadAccessoryButton1;
	        trilist.BooleanInput[joinMap.HasKeypadAccessoryButton2].BoolValue = stbKeypad.HasKeypadAccessoryButton2;

	        trilist.SetBoolSigAction(joinMap.Digit0, stbKeypad.Digit0);
	        trilist.SetBoolSigAction(joinMap.Digit1, stbKeypad.Digit1);
	        trilist.SetBoolSigAction(joinMap.Digit2, stbKeypad.Digit2);
	        trilist.SetBoolSigAction(joinMap.Digit3, stbKeypad.Digit3);
	        trilist.SetBoolSigAction(joinMap.Digit4, stbKeypad.Digit4);
	        trilist.SetBoolSigAction(joinMap.Digit5, stbKeypad.Digit5);
	        trilist.SetBoolSigAction(joinMap.Digit6, stbKeypad.Digit6);
	        trilist.SetBoolSigAction(joinMap.Digit7, stbKeypad.Digit7);
	        trilist.SetBoolSigAction(joinMap.Digit8, stbKeypad.Digit8);
	        trilist.SetBoolSigAction(joinMap.Digit9, stbKeypad.Digit9);
	        trilist.SetBoolSigAction(joinMap.KeypadAccessoryButton1Press, stbKeypad.KeypadAccessoryButton1);
	        trilist.SetBoolSigAction(joinMap.KeypadAccessoryButton2Press, stbKeypad.KeypadAccessoryButton1);
	        trilist.SetBoolSigAction(joinMap.Dash, stbKeypad.Dash);
	        trilist.SetBoolSigAction(joinMap.KeypadEnter, stbKeypad.KeypadEnter);

	        var stbTransport = this as ITransport;
	        trilist.SetBoolSigAction(joinMap.Play, stbTransport.Play);
	        trilist.SetBoolSigAction(joinMap.Pause, stbTransport.Pause);
	        trilist.SetBoolSigAction(joinMap.Rewind, stbTransport.Rewind);
	        trilist.SetBoolSigAction(joinMap.FFwd, stbTransport.FFwd);
	        trilist.SetBoolSigAction(joinMap.ChapMinus, stbTransport.ChapMinus);
	        trilist.SetBoolSigAction(joinMap.ChapPlus, stbTransport.ChapPlus);
	        trilist.SetBoolSigAction(joinMap.Stop, stbTransport.Stop);
	        trilist.SetBoolSigAction(joinMap.Record, stbTransport.Record);
	    }
	}
}