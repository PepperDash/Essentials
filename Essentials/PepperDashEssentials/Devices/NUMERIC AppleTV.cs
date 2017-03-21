//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;
//using Crestron.SimplSharpPro.DeviceSupport;

//using PepperDash.Essentials.Core;

//namespace PepperDash.Essentials.Devices
//{
//    public class AppleTV : Device, IHasCueActionList
//    {
//        public IrOutputPortController IrPort { get; private set; }

//        public AppleTV(string key, string name, IROutputPort port, string irDriverFilepath)
//            : base(key, name) 
//        {
//            IrPort = new IrOutputPortController("ir" + key, port, irDriverFilepath);
//        }

//        #region IFunctionList Members
//        public List<CueActionPair> CueActionList
//        {
//            get 
//            {
//                var numToIr = new Dictionary<Cue, string>
//                {
//                    { CommonBoolCue.Menu, IROutputStandardCommands.IROut_MENU },
//                    { CommonBoolCue.Up, IROutputStandardCommands.IROut_UP_ARROW },
//                    { CommonBoolCue.Down, IROutputStandardCommands.IROut_DN_ARROW },
//                    { CommonBoolCue.Left, IROutputStandardCommands.IROut_LEFT_ARROW },
//                    { CommonBoolCue.Right, IROutputStandardCommands.IROut_RIGHT_ARROW },
//                    { CommonBoolCue.Select, IROutputStandardCommands.IROut_ENTER }
//                };
//                var funcs = new List<CueActionPair>(numToIr.Count);

//                foreach (var kvp in numToIr)
//                    funcs.Add(new BoolCueActionPair(kvp.Key, b => IrPort.PressRelease(kvp.Value, b)));
//                return funcs;
//            }
//        }
//        #endregion
//    }
//}