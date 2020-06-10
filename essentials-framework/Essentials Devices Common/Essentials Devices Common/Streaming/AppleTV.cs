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
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Devices.Common
{
    [Description("Wrapper class for an IR-Controlled AppleTV")]
	public class AppleTV : EssentialsBridgeableDevice, IDPad, ITransport, IUiDisplayInfo, IRoutingOutputs
	{
		public IrOutputPortController IrPort { get; private set; }
		public const string StandardDriverName = "Apple AppleTV-v2.ir";
		public uint DisplayUiType { get { return DisplayUiConstants.TypeAppleTv; } }

		public AppleTV(string key, string name, IrOutputPortController portCont)
			: base(key, name)
		{
			IrPort = portCont;
			DeviceManager.AddDevice(portCont);

			HdmiOut = new RoutingOutputPort(RoutingPortNames.HdmiOut, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.Hdmi, null, this);
			AnyAudioOut = new RoutingOutputPort(RoutingPortNames.AnyAudioOut, eRoutingSignalType.Audio, 
				eRoutingPortConnectionType.DigitalAudio, null, this);
			OutputPorts = new RoutingPortCollection<RoutingOutputPort> { HdmiOut, AnyAudioOut };
		}


		#region IDPad Members

		public void Up(bool pressRelease)
		{
			IrPort.PressRelease("+", pressRelease);
		}

		public void Down(bool pressRelease)
		{
			IrPort.PressRelease("-", pressRelease);
		}

		public void Left(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_TRACK_MINUS, pressRelease);
		}

		public void Right(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_TRACK_PLUS, pressRelease);
		}

		public void Select(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_ENTER, pressRelease);
		}

		public void Menu(bool pressRelease)
		{
			IrPort.PressRelease("Menu", pressRelease);
		}

		public void Exit(bool pressRelease)
		{

		}

		#endregion

		#region ITransport Members

		public void Play(bool pressRelease)
		{
			IrPort.PressRelease("PLAY/PAUSE", pressRelease);
		}

		public void Pause(bool pressRelease)
		{
			IrPort.PressRelease("PLAY/PAUSE", pressRelease);
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		/// <param name="pressRelease"></param>
		public void Rewind(bool pressRelease)
		{
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		/// <param name="pressRelease"></param>
		public void FFwd(bool pressRelease)
		{
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		/// <param name="pressRelease"></param>
		public void ChapMinus(bool pressRelease)
		{
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		/// <param name="pressRelease"></param>
		public void ChapPlus(bool pressRelease)
		{
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		/// <param name="pressRelease"></param>
		public void Stop(bool pressRelease)
		{
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		/// <param name="pressRelease"></param>
		public void Record(bool pressRelease)
		{
		}

		#endregion

		#region IRoutingOutputs Members

		public RoutingOutputPort HdmiOut { get; private set; }
		public RoutingOutputPort AnyAudioOut { get; private set; }
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		#endregion

	    public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
	    {
            var joinMap = new AppleTvJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<AppleTvJoinMap>(joinMapSerialized);

            bridge.AddJoinMap(Key, joinMap);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to Bridge Type {0}", GetType().Name);

            trilist.SetBoolSigAction(joinMap.UpArrow.JoinNumber, Up);
            trilist.SetBoolSigAction(joinMap.DnArrow.JoinNumber, Down);
            trilist.SetBoolSigAction(joinMap.LeftArrow.JoinNumber, Left);
            trilist.SetBoolSigAction(joinMap.RightArrow.JoinNumber, Right);
            trilist.SetBoolSigAction(joinMap.Select.JoinNumber, Select);
            trilist.SetBoolSigAction(joinMap.Menu.JoinNumber, Menu);
            trilist.SetBoolSigAction(joinMap.PlayPause.JoinNumber, Play);
	    }
	}

    public class AppleTVFactory : EssentialsDeviceFactory<AppleTV>
    {
        public AppleTVFactory()
        {
            TypeNames = new List<string>() { "appletv" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new AppleTV Device");
            var irCont = IRPortHelper.GetIrOutputPortController(dc);
            return new AppleTV(dc.Key, dc.Name, irCont);
        }
    }

}