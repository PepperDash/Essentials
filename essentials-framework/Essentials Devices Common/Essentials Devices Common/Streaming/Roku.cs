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
using PepperDash.Essentials.Core.Routing;

namespace PepperDash.Essentials.Devices.Common
{
    [Description("Wrapper class for an IR-Controlled Roku")]
	public class Roku2 : EssentialsDevice, IDPad, ITransport, IUiDisplayInfo, IRoutingOutputs
	{
		[Api]
		public IrOutputPortController IrPort { get; private set; }
		public const string StandardDriverName = "Roku XD_S.ir";
		[Api]
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
		public void Up(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_UP_ARROW, pressRelease);
		}

		[Api]
		public void Down(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_DN_ARROW, pressRelease);
		}

		[Api]
		public void Left(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_LEFT_ARROW, pressRelease);
		}

		[Api]
		public void Right(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_RIGHT_ARROW, pressRelease);
		}

		[Api]
		public void Select(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_ENTER, pressRelease);
		}

		[Api]
		public void Menu(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_MENU, pressRelease);
		}

		[Api]
		public void Exit(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_EXIT, pressRelease);
		}

		#endregion

		#region ITransport Members

		[Api]
		public void Play(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_PLAY, pressRelease);
		}

		[Api]
		public void Pause(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_PAUSE, pressRelease);
		}

		[Api]
		public void Rewind(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_RSCAN, pressRelease);
		}

		[Api]
		public void FFwd(bool pressRelease)
		{
			IrPort.PressRelease(IROutputStandardCommands.IROut_FSCAN, pressRelease);
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
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		#endregion

	}

    public class Roku2Factory : EssentialsDeviceFactory<Roku2>
    {
        public Roku2Factory()
        {
            TypeNames = new List<string>() { "roku" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new Roku Device");
            var irCont = IRPortHelper.GetIrOutputPortController(dc);
            return new Roku2(dc.Key, dc.Name, irCont);

        }
    }

}