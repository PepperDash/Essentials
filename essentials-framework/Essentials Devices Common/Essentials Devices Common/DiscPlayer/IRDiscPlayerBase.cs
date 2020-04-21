using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Devices.Common
{
	public class IRBlurayBase : EssentialsDevice, IDiscPlayerControls, IUiDisplayInfo, IRoutingOutputs, IUsageTracking
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