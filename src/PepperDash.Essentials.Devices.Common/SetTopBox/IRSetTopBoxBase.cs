using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Presets;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common
{
    /// <summary>
    /// Represents a IRSetTopBoxBase
    /// Wrapper class for an IR Set Top Box
    /// </summary>
    [Description("Wrapper class for an IR Set Top Box")]
    public class IRSetTopBoxBase : EssentialsBridgeableDevice, ISetTopBoxControls, IRoutingSource, IRoutingOutputs, IUsageTracking, IHasPowerControl, ITvPresetsProvider
    {
        /// <summary>
        /// Gets or sets the IrPort
        /// </summary>
		public IrOutputPortController IrPort { get; private set; }

        /// <summary>
        /// Gets or sets the DisplayUiType
        /// </summary>
        public uint DisplayUiType { get { return DisplayUiConstants.TypeDirecTv; } }
        /// <summary>
        /// Gets or sets the IrPulseTime
        /// </summary>
        public ushort IrPulseTime { get; set; }

        /// <summary>
        /// Gets or sets the HasPresets
        /// </summary>
        public bool HasPresets { get; set; }
        /// <summary>
        /// Gets or sets the HasDvr
        /// </summary>
        public bool HasDvr { get; set; }
        /// <summary>
        /// Gets or sets the HasDpad
        /// </summary>
        public bool HasDpad { get; set; }
        /// <summary>
        /// Gets or sets the HasNumeric
        /// </summary>
        public bool HasNumeric { get; set; }

        /// <summary>
        /// Gets or sets the TvPresets
        /// </summary>
        public DevicePresetsModel TvPresets { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IRSetTopBoxBase"/> class
        /// </summary>
        /// <param name="key">The unique identifier for the device</param>
        /// <param name="name">The name of the device</param>
        /// <param name="portCont">The IR output port controller</param>
        /// <param name="props">The properties configuration</param>
        public IRSetTopBoxBase(string key, string name, IrOutputPortController portCont,
            SetTopBoxPropertiesConfig props)
            : base(key, name)
        {
            IrPort = portCont;
            IrPulseTime = 200;

            if (props.IrPulseTime > 0)
            {
                IrPulseTime = (ushort)props.IrPulseTime;
            }

            DeviceManager.AddDevice(portCont);

            HasPresets = props.HasPresets;
            HasDvr = props.HasDvr;
            HasDpad = props.HasDpad;
            HasNumeric = props.HasNumeric;

            HasKeypadAccessoryButton1 = true;
            KeypadAccessoryButton1Command = "Dash";
            KeypadAccessoryButton1Label = "-";

            HasKeypadAccessoryButton2 = true;
            KeypadAccessoryButton2Command = "KEYPAD_ENTER";
            KeypadAccessoryButton2Label = "Enter";

            AnyVideoOut = new RoutingOutputPort(RoutingPortNames.AnyVideoOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, null, this);
            AnyAudioOut = new RoutingOutputPort(RoutingPortNames.AnyAudioOut, eRoutingSignalType.Audio,
                eRoutingPortConnectionType.DigitalAudio, null, this);
            OutputPorts = new RoutingPortCollection<RoutingOutputPort> { AnyVideoOut, AnyAudioOut };
        }

        /// <summary>
        /// LoadPresets method
        /// </summary>
        public void LoadPresets(string filePath)
        {
            TvPresets = new DevicePresetsModel(Key + "-presets", this, filePath);
            DeviceManager.AddDevice(TvPresets);
        }


        #region ISetTopBoxControls Members

        /// <summary>
        /// DvrList method
        /// </summary>
        public void DvrList(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_DVR, pressRelease);
        }

        /// <summary>
        /// Replay method
        /// </summary>
        public void Replay(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_REPLAY, pressRelease);
        }

        #endregion

        #region IDPad Members

        /// <summary>
        /// Up method
        /// </summary>
        public void Up(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_UP_ARROW, pressRelease);
        }

        /// <summary>
        /// Down method
        /// </summary>
        public void Down(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_DN_ARROW, pressRelease);
        }

        /// <summary>
        /// Left method
        /// </summary>
        public void Left(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_LEFT_ARROW, pressRelease);
        }

        /// <summary>
        /// Right method
        /// </summary>
        public void Right(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_RIGHT_ARROW, pressRelease);
        }

        /// <summary>
        /// Select method
        /// </summary>
        public void Select(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_ENTER, pressRelease);
        }

        /// <summary>
        /// Menu method
        /// </summary>
        public void Menu(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_MENU, pressRelease);
        }

        /// <summary>
        /// Exit method
        /// </summary>
        public void Exit(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_EXIT, pressRelease);
        }

        #endregion

        #region INumericKeypad Members

        /// <summary>
        /// Digit0 method
        /// </summary>
        public void Digit0(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_0, pressRelease);
        }

        /// <summary>
        /// Digit1 method
        /// </summary>
        public void Digit1(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_1, pressRelease);
        }

        /// <summary>
        /// Digit2 method
        /// </summary>
        public void Digit2(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_2, pressRelease);
        }

        /// <summary>
        /// Digit3 method
        /// </summary>
        public void Digit3(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_3, pressRelease);
        }

        /// <summary>
        /// Digit4 method
        /// </summary>
        public void Digit4(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_4, pressRelease);
        }

        /// <summary>
        /// Digit5 method
        /// </summary>
        public void Digit5(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_5, pressRelease);
        }

        /// <summary>
        /// Digit6 method
        /// </summary>
        public void Digit6(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_6, pressRelease);
        }

        /// <summary>
        /// Digit7 method
        /// </summary>
        public void Digit7(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_7, pressRelease);
        }

        /// <summary>
        /// Digit8 method
        /// </summary>
        public void Digit8(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_8, pressRelease);
        }

        /// <summary>
        /// Digit9 method
        /// </summary>
        public void Digit9(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_9, pressRelease);
        }

        /// <summary>
        /// Gets or sets the HasKeypadAccessoryButton1
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

        /// <summary>
        /// Presses the KeypadAccessoryButton1
        /// </summary>
        public void KeypadAccessoryButton1(bool pressRelease)
        {
            IrPort.PressRelease(KeypadAccessoryButton1Command, pressRelease);
        }

        /// <summary>
        /// Gets or sets the HasKeypadAccessoryButton2
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

        /// <summary>
        /// Presses the KeypadAccessoryButton2
        /// </summary>
        public void KeypadAccessoryButton2(bool pressRelease)
        {
            IrPort.PressRelease(KeypadAccessoryButton2Command, pressRelease);
        }

        #endregion

        #region ISetTopBoxNumericKeypad Members

        /// <summary>
        /// Dash method
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

        /// <summary>   
        /// ChannelUp method
        /// </summary>
        public void ChannelUp(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_CH_PLUS, pressRelease);
        }

        /// <summary>
        /// ChannelDown method
        /// </summary>
        public void ChannelDown(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_CH_MINUS, pressRelease);
        }

        /// <summary>
        /// LastChannel method
        /// </summary>
        public void LastChannel(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_LAST, pressRelease);
        }

        /// <summary>
        /// Guide method
        /// </summary>
        public void Guide(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_GUIDE, pressRelease);
        }

        /// <summary>
        /// Info method
        /// </summary>
        public void Info(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_INFO, pressRelease);
        }

        #endregion

        #region IColorFunctions Members

        /// <summary>
        /// Red method
        /// </summary>
        public void Red(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_RED, pressRelease);
        }

        /// <summary>
        /// Green method
        /// </summary>
        public void Green(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_GREEN, pressRelease);
        }

        /// <summary>
        /// Yellow method
        /// </summary>
        public void Yellow(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_YELLOW, pressRelease);
        }

        /// <summary>
        /// Blue method
        /// </summary>
        public void Blue(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_BLUE, pressRelease);
        }

        #endregion

        #region IRoutingOutputs Members

        /// <summary>
        /// Gets or sets the AnyVideoOut
        /// </summary>
        public RoutingOutputPort AnyVideoOut { get; private set; }
        /// <summary>
        /// Gets or sets the AnyAudioOut
        /// </summary>
        public RoutingOutputPort AnyAudioOut { get; private set; }
        /// <summary>
        /// Gets or sets the OutputPorts
        /// </summary>
        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        #endregion

        #region ITransport Members

        /// <summary>
        /// ChapMinus method
        /// </summary>
        public void ChapMinus(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_REPLAY, pressRelease);
        }

        /// <summary>
        /// ChapPlus method
        /// </summary>
        public void ChapPlus(bool pressRelease)
        {
        }

        /// <summary>
        /// FFwd method
        /// </summary>
        public void FFwd(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_FSCAN, pressRelease);
        }

        /// <summary>
        /// Pause method
        /// </summary>
        public void Pause(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_RSCAN, pressRelease);
        }

        /// <summary>
        /// Play method
        /// </summary>
        public void Play(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_PLAY, pressRelease);
        }

        /// <summary>
        /// Record method
        /// </summary>
        public void Record(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_RECORD, pressRelease);
        }

        /// <summary>
        /// Rewind method
        /// </summary>
        public void Rewind(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_RSCAN, pressRelease);
        }

        /// <summary>
        /// Stop method
        /// </summary>
        public void Stop(bool pressRelease)
        {
            IrPort.PressRelease(IROutputStandardCommands.IROut_STOP, pressRelease);
        }

        #endregion

        #region IUsageTracking Members

        /// <summary>
        /// Gets or sets the UsageTracker
        /// </summary>
        public UsageTracking UsageTracker { get; set; }

        #endregion

        #region IPower Members

        /// <summary>
        /// PowerOn method
        /// </summary>
        public void PowerOn()
        {
            IrPort.Pulse(IROutputStandardCommands.IROut_POWER_ON, IrPulseTime);
        }

        /// <summary>
        /// PowerOff method
        /// </summary>
        public void PowerOff()
        {
            IrPort.Pulse(IROutputStandardCommands.IROut_POWER_OFF, IrPulseTime);
        }

        /// <summary>
        /// PowerToggle method
        /// </summary>
        public void PowerToggle()
        {
            IrPort.Pulse(IROutputStandardCommands.IROut_POWER, IrPulseTime);
        }

        #endregion

        /// <summary>
        /// LinkToApi method
        /// </summary>
        /// <inheritdoc />
        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new SetTopBoxControllerJoinMap(joinStart);
            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<SetTopBoxControllerJoinMap>(joinMapSerialized);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Information, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            Debug.LogMessage(LogEventLevel.Debug, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.LogMessage(LogEventLevel.Information, "Linking to SetTopBox: {0}", Name);

            trilist.OnlineStatusChange += new OnlineStatusChangeEventHandler((o, a) =>
            {
                if (a.DeviceOnLine)
                {
                    trilist.StringInput[joinMap.Name.JoinNumber].StringValue = Name;
                }
            });


            if (this is ISetTopBoxControls stbBase)
            {
                trilist.BooleanInput[joinMap.HasDpad.JoinNumber].BoolValue = stbBase.HasDpad;
                trilist.BooleanInput[joinMap.HasNumeric.JoinNumber].BoolValue = stbBase.HasNumeric;
                trilist.BooleanInput[joinMap.HasDvr.JoinNumber].BoolValue = stbBase.HasDvr;
                trilist.BooleanInput[joinMap.HasPresets.JoinNumber].BoolValue = stbBase.HasPresets;

                trilist.SetBoolSigAction(joinMap.DvrList.JoinNumber, stbBase.DvrList);
                trilist.SetBoolSigAction(joinMap.Replay.JoinNumber, stbBase.Replay);

                trilist.SetStringSigAction(joinMap.LoadPresets.JoinNumber, stbBase.LoadPresets);
            }

            if (this is IHasPowerControl stbPower)
            {
                trilist.SetSigTrueAction(joinMap.PowerOn.JoinNumber, stbPower.PowerOn);
                trilist.SetSigTrueAction(joinMap.PowerOff.JoinNumber, stbPower.PowerOff);
                trilist.SetSigTrueAction(joinMap.PowerToggle.JoinNumber, stbPower.PowerToggle);
            }

            if (this is IDPad stbDPad)
            {
                trilist.SetBoolSigAction(joinMap.Up.JoinNumber, stbDPad.Up);
                trilist.SetBoolSigAction(joinMap.Down.JoinNumber, stbDPad.Down);
                trilist.SetBoolSigAction(joinMap.Left.JoinNumber, stbDPad.Left);
                trilist.SetBoolSigAction(joinMap.Right.JoinNumber, stbDPad.Right);
                trilist.SetBoolSigAction(joinMap.Select.JoinNumber, stbDPad.Select);
                trilist.SetBoolSigAction(joinMap.Menu.JoinNumber, stbDPad.Menu);
                trilist.SetBoolSigAction(joinMap.Exit.JoinNumber, stbDPad.Exit);
            }

            if (this is IChannel stbChannel)
            {
                trilist.SetBoolSigAction(joinMap.ChannelUp.JoinNumber, stbChannel.ChannelUp);
                trilist.SetBoolSigAction(joinMap.ChannelDown.JoinNumber, stbChannel.ChannelDown);
                trilist.SetBoolSigAction(joinMap.LastChannel.JoinNumber, stbChannel.LastChannel);
                trilist.SetBoolSigAction(joinMap.Guide.JoinNumber, stbChannel.Guide);
                trilist.SetBoolSigAction(joinMap.Info.JoinNumber, stbChannel.Info);
                trilist.SetBoolSigAction(joinMap.Exit.JoinNumber, stbChannel.Exit);
            }

            if (this is IColor stbColor)
            {
                trilist.SetBoolSigAction(joinMap.Red.JoinNumber, stbColor.Red);
                trilist.SetBoolSigAction(joinMap.Green.JoinNumber, stbColor.Green);
                trilist.SetBoolSigAction(joinMap.Yellow.JoinNumber, stbColor.Yellow);
                trilist.SetBoolSigAction(joinMap.Blue.JoinNumber, stbColor.Blue);
            }

            if (this is ISetTopBoxNumericKeypad stbKeypad)
            {
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
            }

            if (this is ITransport stbTransport)
            {
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
    }

}