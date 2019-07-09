using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM;

namespace PepperDash.Essentials.DM.Cards
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class DmSingleOutputCardControllerBase// : IRoutingInputsOutputs
	{
		public string Key { get; private set; }
		public uint Slot { get; private set; }
		public abstract eDmOutputCardType Type { get; }

        //public RoutingInputPort BackplaneAudioIn1 { get; private set; }
        //public RoutingInputPort BackplaneVideoIn1 { get; private set; }
        //public RoutingInputPort BackplaneAudioIn2 { get; private set; }
        //public RoutingInputPort BackplaneVideoIn2 { get; private set; }

        //public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }
        //public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		public DmSingleOutputCardControllerBase(string key, uint cardSlot)
		{
			Key = key;
			Slot = cardSlot;
            //BackplaneAudioIn1 = new RoutingInputPort("backplaneAudioIn1", eRoutingSignalType.Audio,
            //    eRoutingPortConnectionType.BackplaneOnly, cardSlot, this);
            //BackplaneVideoIn1 = new RoutingInputPort("backplaneVideoIn1", eRoutingSignalType.Video,
            //    eRoutingPortConnectionType.BackplaneOnly, cardSlot, this);
            //BackplaneAudioIn2 = new RoutingInputPort("backplaneAudioIn2", eRoutingSignalType.Audio,
            //    eRoutingPortConnectionType.BackplaneOnly, cardSlot + 1, this);
            //BackplaneVideoIn2 = new RoutingInputPort("backplaneVideoIn2", eRoutingSignalType.Video,
            //    eRoutingPortConnectionType.BackplaneOnly, cardSlot + 1, this);
            //InputPorts = new RoutingPortCollection<RoutingInputPort>
            //{
            //    BackplaneAudioIn1,
            //    BackplaneAudioIn2,
            //    BackplaneVideoIn1,
            //    BackplaneVideoIn2
            //};
            //OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
		}

		///// <summary>
		///// Gets a physical port by name.  Returns null if doesn't exist
		///// </summary>
		//public RoutingInputPort GetInputPort(string key)
		//{
		//    return InputPorts.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
		//}

        ///// <summary>
        ///// Gets a physical port by name.  Returns null if doesn't exist
        ///// </summary>
        //public RoutingOutputPort GetOutputPort(string key)
        //{
        //    return OutputPorts.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        //}
	}

	/// <summary>
	/// 
	/// </summary>
	public enum eDmOutputCardType
	{
        None, Dmc4kCoHd, Dmc4kHdo, DmcCoHd, DmcSoHd
	}
}