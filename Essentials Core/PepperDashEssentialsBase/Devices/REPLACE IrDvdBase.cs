//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Crestron.SimplSharpPro;

//using PepperDash.Essentials.Core;

//using PepperDash.Core;


//namespace PepperDash.Essentials.Core
//{
//    /// <summary>
//    /// This DVD class should cover most IR, one-way DVD and Bluray fuctions
//    /// </summary>
//    public class IrDvdBase : Device, IHasCueActionList,
//        IPresentationSource, IAttachVideoStatus, IHasFeedback, IRoutingOutputs
//    {
//        public PresentationSourceType Type { get; protected set; }
//        public string IconName { get; set; }

//        public BoolFeedback HasPowerOnFeedback { get; private set; }
//        public IrOutputPortController IrPort { get; private set; }

//        public RoutingOutputPort HdmiOut { get; private set; }

//        #region IRoutingOutputs Members

//        /// <summary>
//        /// Options: hdmi
//        /// </summary>
//        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

//        #endregion

//        public IrDvdBase(string key, string name, IROutputPort port, IrDriverInfo driverInfo)
//            : base(key, name)
//        {
//            IrPort = new IrOutputPortController("ir-" + key, port, driverInfo.FileName);
//            Type = PresentationSourceType.Dvd;
//            IconName = "Bluray";
//            HasPowerOnFeedback = new BoolFeedback(CommonBoolCue.HasPowerFeedback, () => false);

//            HdmiOut = new RoutingOutputPort("HDMI", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 0, this);
//            OutputPorts = new RoutingPortCollection<RoutingOutputPort>() 
//            {  
//                HdmiOut 
//            };
//            CueActionList = IrPort.GetUOsForIrCommands(driverInfo.IrMap);
//        }

//        public IrDvdBase(string key, string name, IROutputPort port, string irDriverFilepath)
//            : base(key, name)
//        {
//            IrPort = new IrOutputPortController("ir-" + key, port, irDriverFilepath);
//            Type = PresentationSourceType.Dvd;
//            IconName = "Bluray";
//            HasPowerOnFeedback = new BoolFeedback(CommonBoolCue.HasPowerFeedback, () => false);

//            HdmiOut = new RoutingOutputPort("HDMI", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 0, this);
//            OutputPorts = new RoutingPortCollection<RoutingOutputPort>() 
//            {  
//                HdmiOut 
//            };
//            var numToIr = new Dictionary<Cue, string>
//            {
//                { CommonBoolCue.Power, IROutputStandardCommands.IROut_POWER },
//                { CommonBoolCue.PowerOff, IROutputStandardCommands.IROut_POWER_OFF },
//                { CommonBoolCue.PowerOn, IROutputStandardCommands.IROut_POWER_ON },
//                { CommonBoolCue.Replay, IROutputStandardCommands.IROut_REPLAY },
//                { CommonBoolCue.Play, IROutputStandardCommands.IROut_PLAY },
//                { CommonBoolCue.Pause, IROutputStandardCommands.IROut_PAUSE },
//                { CommonBoolCue.Stop, IROutputStandardCommands.IROut_STOP },
//                { CommonBoolCue.ChapPrevious, IROutputStandardCommands.IROut_TRACK_MINUS },
//                { CommonBoolCue.ChapNext, IROutputStandardCommands.IROut_TRACK_PLUS },
//                { CommonBoolCue.Rewind, IROutputStandardCommands.IROut_RSCAN },
//                { CommonBoolCue.Ffwd, IROutputStandardCommands.IROut_FSCAN },
//                { CommonBoolCue.RStep, IROutputStandardCommands.IROut_R_STEP },
//                { CommonBoolCue.FStep, IROutputStandardCommands.IROut_F_STEP },
//                { CommonBoolCue.Exit, IROutputStandardCommands.IROut_EXIT },
//                { CommonBoolCue.Home, IROutputStandardCommands.IROut_HOME },
//                { CommonBoolCue.Menu, IROutputStandardCommands.IROut_MENU },
//                { CommonBoolCue.PopUp, IROutputStandardCommands.IROut_POPUPMENU },
//                { CommonBoolCue.Up, IROutputStandardCommands.IROut_UP_ARROW },
//                { CommonBoolCue.Down, IROutputStandardCommands.IROut_DN_ARROW },
//                { CommonBoolCue.Left, IROutputStandardCommands.IROut_LEFT_ARROW },
//                { CommonBoolCue.Right, IROutputStandardCommands.IROut_RIGHT_ARROW },
//                { CommonBoolCue.Select, IROutputStandardCommands.IROut_ENTER },
//                { CommonBoolCue.Info, IROutputStandardCommands.IROut_INFO },
//                { CommonBoolCue.Red, IROutputStandardCommands.IROut_RED },
//                { CommonBoolCue.Green, IROutputStandardCommands.IROut_GREEN },
//                { CommonBoolCue.Yellow, IROutputStandardCommands.IROut_YELLOW },
//                { CommonBoolCue.Blue, IROutputStandardCommands.IROut_BLUE },
//                { CommonBoolCue.Digit0, IROutputStandardCommands.IROut_0 },
//                { CommonBoolCue.Digit1, IROutputStandardCommands.IROut_1 },
//                { CommonBoolCue.Digit2, IROutputStandardCommands.IROut_2 },
//                { CommonBoolCue.Digit3, IROutputStandardCommands.IROut_3 },
//                { CommonBoolCue.Digit4, IROutputStandardCommands.IROut_4 },
//                { CommonBoolCue.Digit5, IROutputStandardCommands.IROut_5 },
//                { CommonBoolCue.Digit6, IROutputStandardCommands.IROut_6 },
//                { CommonBoolCue.Digit7, IROutputStandardCommands.IROut_7 },
//                { CommonBoolCue.Digit8, IROutputStandardCommands.IROut_8 },
//                { CommonBoolCue.Digit9, IROutputStandardCommands.IROut_9 },
//                { CommonBoolCue.Audio, "AUDIO" },
//                { CommonBoolCue.Subtitle, "SUBTITLE" },
//                { CommonBoolCue.Setup, "SETUP" },
//            };
//            CueActionList = IrPort.GetUOsForIrCommands(numToIr);
//        }


//        public List<Feedback> Feedbacks
//        {
//            get { return this.GetVideoStatuses().ToList(); }
//        }


//        #region IFunctionList Members

//        public List<CueActionPair> CueActionList { get; set; }

//        #endregion
//    }

//    public class IrDriverInfo
//    {
//        public Dictionary<Cue, string> IrMap { get; set; }
//        public string FileName { get; set; }
//    }
//}