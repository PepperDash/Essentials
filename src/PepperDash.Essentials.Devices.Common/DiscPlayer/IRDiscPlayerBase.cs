extern alias Full;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using Full.Newtonsoft.Json;
using Full.Newtonsoft.Json.Linq;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Devices.Common
{
    public class IRBlurayBase : EssentialsBridgeableDevice, IDiscPlayerControls, IUiDisplayInfo, IRoutingOutputs, IUsageTracking
	{
		public IrOutputPortController IrPort { get; private set; }

		public uint DisplayUiType { get { return DisplayUiConstants.TypeBluray; } }

		public IRBlurayBase(string key, string name, IrOutputPortController portCont)
			: base(key, name)
		{
			IrPort = portCont;
			DeviceManager.AddDevice(portCont);

			HasKeypadAccessoryButton1 = true;
			KeypadAccessoryButton1Command = "Clear";
			KeypadAccessoryButton1Label = "Clear";

			HasKeypadAccessoryButton2 = true;
			KeypadAccessoryButton2Command = "NumericEnter";
			KeypadAccessoryButton2Label = "Enter";

			PowerIsOnFeedback = new BoolFeedback(() => _PowerIsOn);

			HdmiOut = new RoutingOutputPort(RoutingPortNames.HdmiOut, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.Hdmi, null, this);
			AnyAudioOut = new RoutingOutputPort(RoutingPortNames.AnyAudioOut, eRoutingSignalType.Audio, 
				eRoutingPortConnectionType.DigitalAudio, null, this);
			OutputPorts = new RoutingPortCollection<RoutingOutputPort> { HdmiOut, AnyAudioOut };
		}

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new IRBlurayBaseJoinMap(joinStart);
            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<IRBlurayBaseJoinMap>(joinMapSerialized);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to SetTopBox: {0}", Name);


            trilist.OnlineStatusChange += new OnlineStatusChangeEventHandler((o, a) =>
                {
                    if (a.DeviceOnLine)
                    {
                        trilist.StringInput[joinMap.Name.JoinNumber].StringValue = Name;
                    }
                });

            var powerDev = this as IHasPowerControl;
            if (powerDev != null)
            {
                trilist.SetSigTrueAction(joinMap.PowerOn.JoinNumber, powerDev.PowerOn);
                trilist.SetSigTrueAction(joinMap.PowerOff.JoinNumber, powerDev.PowerOff);
                trilist.SetSigTrueAction(joinMap.PowerToggle.JoinNumber, powerDev.PowerToggle);
            }

            var dpadDev = this as IDPad;
            if (dpadDev != null)
            {
                trilist.SetBoolSigAction(joinMap.Up.JoinNumber, dpadDev.Up);
                trilist.SetBoolSigAction(joinMap.Down.JoinNumber, dpadDev.Down);
                trilist.SetBoolSigAction(joinMap.Left.JoinNumber, dpadDev.Left);
                trilist.SetBoolSigAction(joinMap.Right.JoinNumber, dpadDev.Right);
                trilist.SetBoolSigAction(joinMap.Select.JoinNumber, dpadDev.Select);
                trilist.SetBoolSigAction(joinMap.Menu.JoinNumber, dpadDev.Menu);
                trilist.SetBoolSigAction(joinMap.Exit.JoinNumber, dpadDev.Exit);
            }

            var channelDev = this as IChannel;
            if (channelDev != null)
            {
                trilist.SetBoolSigAction(joinMap.ChannelUp.JoinNumber, channelDev.ChannelUp);
                trilist.SetBoolSigAction(joinMap.ChannelDown.JoinNumber, channelDev.ChannelDown);
                trilist.SetBoolSigAction(joinMap.LastChannel.JoinNumber, channelDev.LastChannel);
                trilist.SetBoolSigAction(joinMap.Guide.JoinNumber, channelDev.Guide);
                trilist.SetBoolSigAction(joinMap.Info.JoinNumber, channelDev.Info);
                trilist.SetBoolSigAction(joinMap.Exit.JoinNumber, channelDev.Exit);
            }

            var colorDev = this as IColor;
            if (colorDev != null)
            {
                trilist.SetBoolSigAction(joinMap.Red.JoinNumber, colorDev.Red);
                trilist.SetBoolSigAction(joinMap.Green.JoinNumber, colorDev.Green);
                trilist.SetBoolSigAction(joinMap.Yellow.JoinNumber, colorDev.Yellow);
                trilist.SetBoolSigAction(joinMap.Blue.JoinNumber, colorDev.Blue);
            }

            var keypadDev = this as ISetTopBoxNumericKeypad;
            if (keypadDev != null)
            {
                trilist.StringInput[joinMap.KeypadAccessoryButton1Label.JoinNumber].StringValue = keypadDev.KeypadAccessoryButton1Label;
                trilist.StringInput[joinMap.KeypadAccessoryButton2Label.JoinNumber].StringValue = keypadDev.KeypadAccessoryButton2Label;

                trilist.BooleanInput[joinMap.HasKeypadAccessoryButton1.JoinNumber].BoolValue = keypadDev.HasKeypadAccessoryButton1;
                trilist.BooleanInput[joinMap.HasKeypadAccessoryButton2.JoinNumber].BoolValue = keypadDev.HasKeypadAccessoryButton2;

                trilist.SetBoolSigAction(joinMap.Digit0.JoinNumber, keypadDev.Digit0);
                trilist.SetBoolSigAction(joinMap.Digit1.JoinNumber, keypadDev.Digit1);
                trilist.SetBoolSigAction(joinMap.Digit2.JoinNumber, keypadDev.Digit2);
                trilist.SetBoolSigAction(joinMap.Digit3.JoinNumber, keypadDev.Digit3);
                trilist.SetBoolSigAction(joinMap.Digit4.JoinNumber, keypadDev.Digit4);
                trilist.SetBoolSigAction(joinMap.Digit5.JoinNumber, keypadDev.Digit5);
                trilist.SetBoolSigAction(joinMap.Digit6.JoinNumber, keypadDev.Digit6);
                trilist.SetBoolSigAction(joinMap.Digit7.JoinNumber, keypadDev.Digit7);
                trilist.SetBoolSigAction(joinMap.Digit8.JoinNumber, keypadDev.Digit8);
                trilist.SetBoolSigAction(joinMap.Digit9.JoinNumber, keypadDev.Digit9);
                trilist.SetBoolSigAction(joinMap.KeypadAccessoryButton1Press.JoinNumber, keypadDev.KeypadAccessoryButton1);
                trilist.SetBoolSigAction(joinMap.KeypadAccessoryButton2Press.JoinNumber, keypadDev.KeypadAccessoryButton1);
                trilist.SetBoolSigAction(joinMap.KeypadEnter.JoinNumber, keypadDev.KeypadEnter);
            }

            var transportDev = this as ITransport;
            if (transportDev != null)
            {
                trilist.SetBoolSigAction(joinMap.Play.JoinNumber, transportDev.Play);
                trilist.SetBoolSigAction(joinMap.Pause.JoinNumber, transportDev.Pause);
                trilist.SetBoolSigAction(joinMap.Rewind.JoinNumber, transportDev.Rewind);
                trilist.SetBoolSigAction(joinMap.FFwd.JoinNumber, transportDev.FFwd);
                trilist.SetBoolSigAction(joinMap.ChapMinus.JoinNumber, transportDev.ChapMinus);
                trilist.SetBoolSigAction(joinMap.ChapPlus.JoinNumber, transportDev.ChapPlus);
                trilist.SetBoolSigAction(joinMap.Stop.JoinNumber, transportDev.Stop);
                trilist.SetBoolSigAction(joinMap.Record.JoinNumber, transportDev.Record);
            }
        }


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

		public RoutingOutputPort HdmiOut { get; private set; }
		public RoutingOutputPort AnyAudioOut { get; private set; }
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		#endregion

		#region IPower Members

		public void PowerOn()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_POWER_ON, 200);
			_PowerIsOn = true;
		}

		public void PowerOff()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_POWER_OFF, 200);
			_PowerIsOn = false;
		}

		public void PowerToggle()
		{
			IrPort.Pulse(IROutputStandardCommands.IROut_POWER, 200);
			_PowerIsOn = false;
		}

		public BoolFeedback PowerIsOnFeedback { get; set; }
		bool _PowerIsOn;

		#endregion

		#region ITransport Members

		public void Play(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_PLAY, pressRelease);
		}

		public void Pause(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_PAUSE, pressRelease);
		}

		public void Rewind(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_RSCAN, pressRelease);
		}

		public void FFwd(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_FSCAN, pressRelease);
		}

		public void ChapMinus(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_TRACK_MINUS, pressRelease);
		}

		public void ChapPlus(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_TRACK_PLUS, pressRelease);
		}

		public void Stop(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_STOP, pressRelease);
		}

		public void Record(bool pressRelease)
		{
		}

		#endregion

        #region IUsageTracking Members

        public UsageTracking UsageTracker { get; set; }

        #endregion
    }

    public class IRBlurayBaseFactory : EssentialsDeviceFactory<IRBlurayBase>
    {
        public IRBlurayBaseFactory()
        {
            TypeNames = new List<string>() { "discplayer", "bluray" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new IRBlurayPlayer Device");

            if (dc.Properties["control"]["method"].Value<string>() == "ir")
            {
                var irCont = IRPortHelper.GetIrOutputPortController(dc);
                return new IRBlurayBase(dc.Key, dc.Name, irCont);
            }
            else if (dc.Properties["control"]["method"].Value<string>() == "com")
            {
                Debug.Console(0, "[{0}] COM Device type not implemented YET!", dc.Key);
            }

            return null;
        }
    }

}