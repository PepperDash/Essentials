using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common
{
    [Description("Wrapper class for an IR-Controlled Roku")]
 /// <summary>
 /// Represents a Roku2
 /// </summary>
	public class Roku2 : EssentialsDevice, IDPad, ITransport, IUiDisplayInfo, IRoutingSource, IRoutingOutputs
	{
		[Api]
  /// <summary>
  /// Gets or sets the IrPort
  /// </summary>
		public IrOutputPortController IrPort { get; private set; }
		public const string StandardDriverName = "Roku XD_S.ir";
		[Api]
  /// <summary>
  /// Gets or sets the DisplayUiType
  /// </summary>
		public uint DisplayUiType { get { return DisplayUiConstants.TypeRoku; } }

		public Roku2(string key, string name, IrOutputPortController portCont)
			: base(key, name)
		{
			IrPort = portCont;
			DeviceManager.AddDevice(portCont);;

			HdmiOut = new RoutingOutputPort(RoutingPortNames.HdmiOut, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.Hdmi, null, this);
			OutputPorts = new RoutingPortCollection<RoutingOutputPort> { HdmiOut };
		}

		#region IDPad Members

		[Api]
  /// <summary>
  /// Up method
  /// </summary>
		public void Up(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_UP_ARROW, pressRelease);
		}

		[Api]
  /// <summary>
  /// Down method
  /// </summary>
		public void Down(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_DN_ARROW, pressRelease);
		}

		[Api]
  /// <summary>
  /// Left method
  /// </summary>
		public void Left(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_LEFT_ARROW, pressRelease);
		}

		[Api]
  /// <summary>
  /// Right method
  /// </summary>
		public void Right(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_RIGHT_ARROW, pressRelease);
		}

		[Api]
  /// <summary>
  /// Select method
  /// </summary>
		public void Select(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_ENTER, pressRelease);
		}

		[Api]
  /// <summary>
  /// Menu method
  /// </summary>
		public void Menu(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_MENU, pressRelease);
		}

		[Api]
  /// <summary>
  /// Exit method
  /// </summary>
		public void Exit(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_EXIT, pressRelease);
		}

		#endregion

		#region ITransport Members

		[Api]
  /// <summary>
  /// Play method
  /// </summary>
		public void Play(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_PLAY, pressRelease);
		}

		[Api]
  /// <summary>
  /// Pause method
  /// </summary>
		public void Pause(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_PAUSE, pressRelease);
		}

		[Api]
  /// <summary>
  /// Rewind method
  /// </summary>
		public void Rewind(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_RSCAN, pressRelease);
		}

		[Api]
  /// <summary>
  /// FFwd method
  /// </summary>
		public void FFwd(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_FSCAN, pressRelease);
		}

		/// <summary>
		/// Not implemented
		/// </summary>
		/// <param name="pressRelease"></param>
  /// <summary>
  /// ChapMinus method
  /// </summary>
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
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		#endregion

	}

    /// <summary>
    /// Represents a Roku2Factory
    /// </summary>
    public class Roku2Factory : EssentialsDeviceFactory<Roku2>
    {
        public Roku2Factory()
        {
            TypeNames = new List<string>() { "roku" };
        }

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Roku Device");
            var irCont = IRPortHelper.GetIrOutputPortController(dc);
            return new Roku2(dc.Key, dc.Name, irCont);

        }
    }

}