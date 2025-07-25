

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using System.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common
{
    [Description("Wrapper class for an IR-Controlled AppleTV")]
 /// <summary>
 /// Represents a AppleTV
 /// </summary>
	public class AppleTV : EssentialsBridgeableDevice, IDPad, ITransport, IUiDisplayInfo, IRoutingSource, IRoutingOutputs

    {
  /// <summary>
  /// Gets or sets the IrPort
  /// </summary>
		public IrOutputPortController IrPort { get; private set; }
        public const string StandardDriverName = "Apple_AppleTV_4th_Gen_Essentials.ir";
  /// <summary>
  /// Gets or sets the DisplayUiType
  /// </summary>
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

            PrintExpectedIrCommands();
		}

        /// <summary>
        /// PrintExpectedIrCommands method
        /// </summary>
        public void PrintExpectedIrCommands()
        {
            var cmds = typeof (AppleTvIrCommands).GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var value in cmds.Select(cmd => cmd.GetValue(null)).OfType<string>())
            {
                Debug.LogMessage(LogEventLevel.Verbose, this, "Expected IR Function Name: {0}", value);
            }
        }

        #region IDPad Members

  /// <summary>
  /// Up method
  /// </summary>
		public void Up(bool pressRelease)
		{
			IrPort.PressRelease(AppleTvIrCommands.Up, pressRelease);
		}

  /// <summary>
  /// Down method
  /// </summary>
		public void Down(bool pressRelease)
		{
			IrPort.PressRelease(AppleTvIrCommands.Down, pressRelease);
		}

  /// <summary>
  /// Left method
  /// </summary>
		public void Left(bool pressRelease)
		{
			IrPort.PressRelease(AppleTvIrCommands.Left, pressRelease);
		}

  /// <summary>
  /// Right method
  /// </summary>
		public void Right(bool pressRelease)
		{
			IrPort.PressRelease(AppleTvIrCommands.Right, pressRelease);
		}

  /// <summary>
  /// Select method
  /// </summary>
		public void Select(bool pressRelease)
		{
			IrPort.PressRelease(AppleTvIrCommands.Enter, pressRelease);
		}

  /// <summary>
  /// Menu method
  /// </summary>
		public void Menu(bool pressRelease)
		{
			IrPort.PressRelease(AppleTvIrCommands.Menu, pressRelease);
		}

  /// <summary>
  /// Exit method
  /// </summary>
		public void Exit(bool pressRelease)
		{

		}

		#endregion

		#region ITransport Members

  /// <summary>
  /// Play method
  /// </summary>
		public void Play(bool pressRelease)
		{
			IrPort.PressRelease(AppleTvIrCommands.PlayPause, pressRelease);
		}

  /// <summary>
  /// Pause method
  /// </summary>
		public void Pause(bool pressRelease)
		{
            IrPort.PressRelease(AppleTvIrCommands.PlayPause, pressRelease);
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		/// <param name="pressRelease"></param>
  /// <summary>
  /// Rewind method
  /// </summary>
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
  /// <summary>
  /// Gets or sets the OutputPorts
  /// </summary>
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		#endregion

     /// <summary>
     /// LinkToApi method
     /// </summary>
	    public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
	    {
            var joinMap = new AppleTvJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<AppleTvJoinMap>(joinMapSerialized);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Information, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            Debug.LogMessage(LogEventLevel.Debug, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.LogMessage(LogEventLevel.Information, "Linking to Bridge Type {0}", GetType().Name);

            trilist.SetBoolSigAction(joinMap.UpArrow.JoinNumber, Up);
            trilist.SetBoolSigAction(joinMap.DnArrow.JoinNumber, Down);
            trilist.SetBoolSigAction(joinMap.LeftArrow.JoinNumber, Left);
            trilist.SetBoolSigAction(joinMap.RightArrow.JoinNumber, Right);
            trilist.SetBoolSigAction(joinMap.Select.JoinNumber, Select);
            trilist.SetBoolSigAction(joinMap.Menu.JoinNumber, Menu);
            trilist.SetBoolSigAction(joinMap.PlayPause.JoinNumber, Play);
	    }
	}

    /// <summary>
    /// Represents a AppleTVFactory
    /// </summary>
    public class AppleTVFactory : EssentialsDeviceFactory<AppleTV>
    {
        public AppleTVFactory()
        {
            TypeNames = new List<string>() { "appletv" };
        }

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new AppleTV Device");
            var irCont = IRPortHelper.GetIrOutputPortController(dc);
            return new AppleTV(dc.Key, dc.Name, irCont);
        }
    }

    public static class AppleTvIrCommands
    {
        
        public static string Up = "+";
        public static string Down = "-";
        public static string Left = IROutputStandardCommands.IROut_TRACK_MINUS;
        public static string Right = IROutputStandardCommands.IROut_TRACK_PLUS;
        public static string Enter = IROutputStandardCommands.IROut_ENTER;
        public static string PlayPause = "PLAY/PAUSE";
        public static string Rewind = "REWIND";
        public static string Menu = "Menu";
        public static string FastForward = "FASTFORWARD";
    }
}