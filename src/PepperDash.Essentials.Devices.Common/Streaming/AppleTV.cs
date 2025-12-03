using System.Linq;
using System.Reflection;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common
{
    /// <summary>
    /// Represents a AppleTV
    /// Wrapper class for an IR-Controlled AppleTV
    /// </summary>
    [Description("Wrapper class for an IR-Controlled AppleTV")]
    public class AppleTV : EssentialsBridgeableDevice, IDPad, ITransport, IUiDisplayInfo, IRoutingSource, IRoutingOutputs

    {
        /// <summary>
        /// Gets or sets the IrPort
        /// </summary>
		public IrOutputPortController IrPort { get; private set; }

        /// <summary>
        /// Standard Driver Name
        /// </summary>
        public const string StandardDriverName = "Apple_AppleTV_4th_Gen_Essentials.ir";
        /// <summary>
        /// Gets or sets the DisplayUiType
        /// </summary>
        public uint DisplayUiType { get { return DisplayUiConstants.TypeAppleTv; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppleTV"/> class
        /// </summary>
        /// <param name="key">The device key</param>
        /// <param name="name">The device name</param>
        /// <param name="portCont">The IR output port controller</param>
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
            var cmds = typeof(AppleTvIrCommands).GetFields(BindingFlags.Public | BindingFlags.Static);

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

        /// <summary>
        /// Gets the HdmiOut
        /// </summary>
        public RoutingOutputPort HdmiOut { get; private set; }

        /// <summary>
        /// Gets the AnyAudioOut
        /// </summary>
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
}