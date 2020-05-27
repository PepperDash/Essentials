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
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Presets;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Devices.Common
{
    [Description("Wrapper class for an IR Set Top Box")]
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

	    public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
	    {
            var joinMap = new SetTopBoxControllerJoinMap(joinStart);
            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<SetTopBoxControllerJoinMap>(joinMapSerialized);

            bridge.AddJoinMap(Key, joinMap);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to Display: {0}", Name);

            trilist.StringInput[joinMap.Name.JoinNumber].StringValue = Name;

            var stbBase = this as ISetTopBoxControls;

            trilist.BooleanInput[joinMap.HasDpad.JoinNumber].BoolValue = stbBase.HasDpad;
            trilist.BooleanInput[joinMap.HasNumeric.JoinNumber].BoolValue = stbBase.HasNumeric;
            trilist.BooleanInput[joinMap.HasDvr.JoinNumber].BoolValue = stbBase.HasDvr;
            trilist.BooleanInput[joinMap.HasPresets.JoinNumber].BoolValue = stbBase.HasPresets;

            trilist.SetBoolSigAction(joinMap.DvrList.JoinNumber, stbBase.DvrList);
            trilist.SetBoolSigAction(joinMap.Replay.JoinNumber, stbBase.Replay);

            trilist.SetStringSigAction(joinMap.LoadPresets.JoinNumber, stbBase.LoadPresets);

	        var stbPower = this as IPower;

            trilist.SetSigTrueAction(joinMap.PowerOn.JoinNumber, stbPower.PowerOn);
            trilist.SetSigTrueAction(joinMap.PowerOff.JoinNumber, stbPower.PowerOff);
            trilist.SetSigTrueAction(joinMap.PowerToggle.JoinNumber, stbPower.PowerToggle);

	        var stbDPad = this as IDPad;

            trilist.SetBoolSigAction(joinMap.Up.JoinNumber, stbDPad.Up);
            trilist.SetBoolSigAction(joinMap.Down.JoinNumber, stbDPad.Down);
            trilist.SetBoolSigAction(joinMap.Left.JoinNumber, stbDPad.Left);
            trilist.SetBoolSigAction(joinMap.Right.JoinNumber, stbDPad.Right);
            trilist.SetBoolSigAction(joinMap.Select.JoinNumber, stbDPad.Select);
            trilist.SetBoolSigAction(joinMap.Menu.JoinNumber, stbDPad.Menu);
            trilist.SetBoolSigAction(joinMap.Exit.JoinNumber, stbDPad.Exit);

	        var stbChannel = this as IChannel;
            trilist.SetBoolSigAction(joinMap.ChannelUp.JoinNumber, stbChannel.ChannelUp);
            trilist.SetBoolSigAction(joinMap.ChannelDown.JoinNumber, stbChannel.ChannelDown);
            trilist.SetBoolSigAction(joinMap.LastChannel.JoinNumber, stbChannel.LastChannel);
            trilist.SetBoolSigAction(joinMap.Guide.JoinNumber, stbChannel.Guide);
            trilist.SetBoolSigAction(joinMap.Info.JoinNumber, stbChannel.Info);
            trilist.SetBoolSigAction(joinMap.Exit.JoinNumber, stbChannel.Exit);

	        var stbColor = this as IColor;
            trilist.SetBoolSigAction(joinMap.Red.JoinNumber, stbColor.Red);
            trilist.SetBoolSigAction(joinMap.Green.JoinNumber, stbColor.Green);
            trilist.SetBoolSigAction(joinMap.Yellow.JoinNumber, stbColor.Yellow);
            trilist.SetBoolSigAction(joinMap.Blue.JoinNumber, stbColor.Blue);

	        var stbKeypad = this as ISetTopBoxNumericKeypad;

	        trilist.StringInput[joinMap.KeypadAccessoryButton1Label.JoinNumber].StringValue = stbKeypad.KeypadAccessoryButton1Label;
            trilist.StringInput[joinMap.KeypadAccessoryButton2Label.JoinNumber].StringValue = stbKeypad.KeypadAccessoryButton2Label;

            trilist.BooleanInput[joinMap.HasKeypadAccessoryButton1.JoinNumber].BoolValue = stbKeypad.HasKeypadAccessoryButton1;
            trilist.BooleanInput[joinMap.HasKeypadAccessoryButton2.JoinNumber].BoolValue = stbKeypad.HasKeypadAccessoryButton2;

            trilist.SetBoolSigAction(joinMap.Digit0.JoinNumber, stbKeypad.Digit0);
            trilist.SetBoolSigAction(joinMap.Digit1.JoinNumber, stbKeypad.Digit1);
            trilist.SetBoolSigAction(joinMap.Digit2.JoinNumber, stbKeypad.Digit2);
            trilist.SetBoolSigAction(joinMap.Digit3.JoinNumber, stbKeypad.Digit3);
            trilist.SetBoolSigAction(joinMap.Digit4.JoinNumber, stbKeypad.Digit4);
            trilist.SetBoolSigAction(joinMap.Digit5.JoinNumber, stbKeypad.Digit5);
            trilist.SetBoolSigAction(joinMap.Digit6.JoinNumber, stbKeypad.Digit6);
            trilist.SetBoolSigAction(joinMap.Digit7.JoinNumber, stbKeypad.Digit7);
            trilist.SetBoolSigAction(joinMap.Digit8.JoinNumber, stbKeypad.Digit8);
            trilist.SetBoolSigAction(joinMap.Digit9.JoinNumber, stbKeypad.Digit9);
            trilist.SetBoolSigAction(joinMap.KeypadAccessoryButton1Press.JoinNumber, stbKeypad.KeypadAccessoryButton1);
            trilist.SetBoolSigAction(joinMap.KeypadAccessoryButton2Press.JoinNumber, stbKeypad.KeypadAccessoryButton1);
            trilist.SetBoolSigAction(joinMap.Dash.JoinNumber, stbKeypad.Dash);
            trilist.SetBoolSigAction(joinMap.KeypadEnter.JoinNumber, stbKeypad.KeypadEnter);

	        var stbTransport = this as ITransport;
            trilist.SetBoolSigAction(joinMap.Play.JoinNumber, stbTransport.Play);
            trilist.SetBoolSigAction(joinMap.Pause.JoinNumber, stbTransport.Pause);
            trilist.SetBoolSigAction(joinMap.Rewind.JoinNumber, stbTransport.Rewind);
            trilist.SetBoolSigAction(joinMap.FFwd.JoinNumber, stbTransport.FFwd);
            trilist.SetBoolSigAction(joinMap.ChapMinus.JoinNumber, stbTransport.ChapMinus);
            trilist.SetBoolSigAction(joinMap.ChapPlus.JoinNumber, stbTransport.ChapPlus);
            trilist.SetBoolSigAction(joinMap.Stop.JoinNumber, stbTransport.Stop);
            trilist.SetBoolSigAction(joinMap.Record.JoinNumber, stbTransport.Record);
	    }
	}

    public class IRSetTopBoxBaseFactory : EssentialsDeviceFactory<IRSetTopBoxBase>
    {
        public IRSetTopBoxBaseFactory()
        {
            TypeNames = new List<string>() { "settopbox" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new SetTopBox Device");
            var irCont = IRPortHelper.GetIrOutputPortController(dc);
            var config = dc.Properties.ToObject<SetTopBoxPropertiesConfig>();
            var stb = new IRSetTopBoxBase(dc.Key, dc.Name, irCont, config);

            var listName = dc.Properties.Value<string>("presetsList");
            if (listName != null)
                stb.LoadPresets(listName);
            return stb;

        }
    }

}