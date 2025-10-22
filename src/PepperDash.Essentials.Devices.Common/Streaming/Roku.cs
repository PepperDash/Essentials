using Crestron.SimplSharpPro;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Devices.Common
{
	/// <summary>
	/// Represents a Roku2
	/// Wrapper class for an IR-Controlled Roku
	/// </summary>
	[Description("Wrapper class for an IR-Controlled Roku")]
	public class Roku2 : EssentialsDevice, IDPad, ITransport, IUiDisplayInfo, IRoutingSource, IRoutingOutputs
	{
		/// <summary>
		/// Gets or sets the IrPort
		/// </summary>
		[Api]
		public IrOutputPortController IrPort { get; private set; }

		/// <summary>
		/// Standard Driver Name
		/// </summary>
		public const string StandardDriverName = "Roku XD_S.ir";
		/// <summary>
		/// Gets or sets the DisplayUiType
		/// </summary>
		[Api]
		public uint DisplayUiType { get { return DisplayUiConstants.TypeRoku; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Roku2"/> class
		/// </summary>
		public Roku2(string key, string name, IrOutputPortController portCont)
			: base(key, name)
		{
			IrPort = portCont;
			DeviceManager.AddDevice(portCont); ;

			HdmiOut = new RoutingOutputPort(RoutingPortNames.HdmiOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Hdmi, null, this);
			OutputPorts = new RoutingPortCollection<RoutingOutputPort> { HdmiOut };
		}

		#region IDPad Members

		/// <summary>
		/// Up method
		/// </summary>
		[Api]
		public void Up(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_UP_ARROW, pressRelease);
		}

		/// <summary>
		/// Down method
		/// </summary>
		[Api]
		public void Down(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_DN_ARROW, pressRelease);
		}

		/// <summary>
		/// Left method
		/// </summary>
		[Api]
		public void Left(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_LEFT_ARROW, pressRelease);
		}

		/// <summary>
		/// Right method
		/// </summary>
		[Api]
		public void Right(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_RIGHT_ARROW, pressRelease);
		}


		/// <summary>
		/// Select method
		/// </summary>
		[Api]
		public void Select(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_ENTER, pressRelease);
		}

		/// <summary>
		/// Menu method
		/// </summary>
		[Api]
		public void Menu(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_MENU, pressRelease);
		}

		/// <summary>
		/// Exit method
		/// </summary>
		[Api]
		public void Exit(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_EXIT, pressRelease);
		}

		#endregion

		#region ITransport Members

		/// <summary>
		/// Play method
		/// </summary>
		[Api]
		public void Play(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_PLAY, pressRelease);
		}

		/// <summary>
		/// Pause method
		/// </summary>
		[Api]
		public void Pause(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_PAUSE, pressRelease);
		}

		/// <summary>
		/// Rewind method
		/// </summary>
		[Api]
		public void Rewind(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_RSCAN, pressRelease);
		}

		/// <summary>
		/// FFwd method
		/// </summary>
		[Api]
		public void FFwd(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_FSCAN, pressRelease);
		}

		/// <summary>
		/// ChapMinus method - Not implemented
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
		/// HdmiOut
		/// </summary>
		public RoutingOutputPort HdmiOut { get; private set; }

		/// <summary>
		/// OutputPorts
		/// </summary>
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		#endregion

	}

}