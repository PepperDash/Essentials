//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;
//using Crestron.SimplSharpPro.DM;
//using Crestron.SimplSharpPro.DM.Cards;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.DM;

//namespace PepperDash.Essentials.DM.Cards
//{
//    public class DmInputCardControllerBase : IRoutingInputsOutputs
//    {
//        public string Key { get; private set; }
//        public uint Slot { get; private set; }
//        public abstract eDmInputCardType Type { get; }

//        //public RoutingOutputPort BackplaneVideoOut { get; private set; }
//        //public RoutingOutputPort BackplaneAudioOut { get; private set; }

//        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

//        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

//        public DmInputCardControllerBase(string key, uint slot)
//        {
//            Key = key;
//            Slot = slot;
//            //BackplaneAudioOut = new RoutingOutputPort("backplaneAudioOut", eRoutingSignalType.Audio,
//            //    eRoutingPortConnectionType.BackplaneOnly, slot, this);
//            //BackplaneVideoOut = new RoutingOutputPort("backplaneVideoOut", eRoutingSignalType.Video,
//            //    eRoutingPortConnectionType.BackplaneOnly, slot, this);
//            //InputPorts = new RoutingPortCollection<RoutingInputPort>();
//            //OutputPorts = new RoutingPortCollection<RoutingOutputPort> { BackplaneAudioOut, BackplaneVideoOut };
//        }

//        ///// <summary>
//        ///// Gets a physical port by name.  Returns null if doesn't exist
//        ///// </summary>
//        //public RoutingInputPort GetInputPort(string key)
//        //{
//        //    return InputPorts.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
//        //}

//        ///// <summary>
//        ///// Gets a physical port by name.  Returns null if doesn't exist
//        ///// </summary>
//        //public RoutingOutputPort GetOutputPort(string key)
//        //{
//        //    return OutputPorts.FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
//        //}
//    }

//    public enum eDmInputCardType
//    {
//        None, DmcHd, DmcHdDsp, Dmc4kHd, Dmc4kHdDsp, Dmc4kC, Dmc4kCDsp
//    }
//}