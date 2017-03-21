//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Crestron.SimplSharpPro;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Core.Presets;

//using PepperDash.Core;


//namespace PepperDash.Essentials.Core
//{
//    /// <summary>
//    /// This DVD class should cover most IR, one-way DVD and Bluray fuctions
//    /// </summary>
//    public class IrSetTopBoxBase : Device, IHasCueActionList,
//        IPresentationSource, IAttachVideoStatus, IHasFeedback, IRoutingOutputs, IHasSetTopBoxProperties
//    {
//        public PresentationSourceType Type { get; protected set; }
//        public string IconName { get; set; }

//        public BoolFeedback HasPowerOnFeedback { get; private set; }
//        public IrOutputPortController IrPort { get; private set; }

//        public DevicePresetsModel PresetsModel { get; private set; }

//        #region IRoutingOutputs Members

//        /// <summary>
//        /// Options: hdmi
//        /// </summary>
//        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

//        #endregion

//        public IrSetTopBoxBase(string key, string name, IROutputPort port, string irDriverFilepath)
//            : base(key, name)
//        {
//            IrPort = new IrOutputPortController("ir-" + key, port, irDriverFilepath);
//            Type = PresentationSourceType.SetTopBox;
//            IconName = "TV";
//            HasPowerOnFeedback = new BoolFeedback(CommonBoolCue.HasPowerFeedback, () => false);
//            OutputPorts = new RoutingPortCollection<RoutingOutputPort>()
//            {
//                 new RoutingOutputPort("HDMI", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, 0, this)
//            };
//        }

//        public void LoadPresets(string filePath)
//        {
//            //PresetsModel = new DevicePresetsModel(Key + "-presets", this, filePath);
//            //DeviceManager.AddDevice(PresetsModel);
//        }

//        #region IDeviceWithOutputs Members

//        public List<Feedback> Feedbacks
//        {
//            get { return this.GetVideoStatuses().ToList(); }
//        }

//        #endregion

//        #region IFunctionList Members

//        public List<CueActionPair> CueActionList
//        {
//            get 
//            {
//                // This might be the best way to get the words back into functions
//                new BoolCueActionPair(CommonBoolCue.Power, b => IrPort.PressRelease(IROutputStandardCommands.IROut_POWER, b));
//                new BoolCueActionPair(CommonBoolCue.PowerOn, b => IrPort.PressRelease(IROutputStandardCommands.IROut_POWER_ON, b));
//                new BoolCueActionPair(CommonBoolCue.PowerOff, b => IrPort.PressRelease(IROutputStandardCommands.IROut_POWER_OFF, b));
//                new BoolCueActionPair(CommonBoolCue.ChannelUp, b => IrPort.PressRelease(IROutputStandardCommands.IROut_CH_PLUS, b));

//                var numToIr = new Dictionary<Cue, string>
//                {
//                    { CommonBoolCue.Power, IROutputStandardCommands.IROut_POWER },
//                    { CommonBoolCue.PowerOn, IROutputStandardCommands.IROut_POWER_ON },
//                    { CommonBoolCue.PowerOff, IROutputStandardCommands.IROut_POWER_OFF },
//                    { CommonBoolCue.ChannelUp, IROutputStandardCommands.IROut_CH_PLUS },
//                    { CommonBoolCue.ChannelDown, IROutputStandardCommands.IROut_CH_MINUS },
//                    { CommonBoolCue.Last, IROutputStandardCommands.IROut_LAST },
//                    { CommonBoolCue.Play, IROutputStandardCommands.IROut_PLAY },
//                    { CommonBoolCue.Pause, IROutputStandardCommands.IROut_PAUSE },
//                    { CommonBoolCue.Stop, IROutputStandardCommands.IROut_STOP },
//                    { CommonBoolCue.ChapPrevious, IROutputStandardCommands.IROut_TRACK_MINUS },
//                    { CommonBoolCue.ChapNext, IROutputStandardCommands.IROut_TRACK_PLUS },
//                    { CommonBoolCue.Rewind, IROutputStandardCommands.IROut_RSCAN },
//                    { CommonBoolCue.Ffwd, IROutputStandardCommands.IROut_FSCAN },
//                    { CommonBoolCue.Replay, IROutputStandardCommands.IROut_REPLAY },
//                    { CommonBoolCue.Advance, "ADVANCE" },
//                    { CommonBoolCue.Record, IROutputStandardCommands.IROut_RECORD },
//                    { CommonBoolCue.Exit, IROutputStandardCommands.IROut_EXIT },
//                    { CommonBoolCue.Menu, IROutputStandardCommands.IROut_MENU },
//                    { CommonBoolCue.List, IROutputStandardCommands.IROut_DVR },
//                    { CommonBoolCue.Dvr, IROutputStandardCommands.IROut_DVR },
//                    { CommonBoolCue.Back, IROutputStandardCommands.IROut_BACK },
//                    { CommonBoolCue.Up, IROutputStandardCommands.IROut_UP_ARROW },
//                    { CommonBoolCue.Down, IROutputStandardCommands.IROut_DN_ARROW },
//                    { CommonBoolCue.Left, IROutputStandardCommands.IROut_LEFT_ARROW },
//                    { CommonBoolCue.Right, IROutputStandardCommands.IROut_RIGHT_ARROW },
//                    { CommonBoolCue.Select, IROutputStandardCommands.IROut_ENTER },
//                    { CommonBoolCue.Guide, IROutputStandardCommands.IROut_GUIDE },
//                    { CommonBoolCue.PageUp, IROutputStandardCommands.IROut_PAGE_UP },
//                    { CommonBoolCue.PageDown, IROutputStandardCommands.IROut_PAGE_DOWN },
//                    { CommonBoolCue.Info, IROutputStandardCommands.IROut_INFO },
//                    { CommonBoolCue.Red, IROutputStandardCommands.IROut_RED },
//                    { CommonBoolCue.Green, IROutputStandardCommands.IROut_GREEN },
//                    { CommonBoolCue.Yellow, IROutputStandardCommands.IROut_YELLOW },
//                    { CommonBoolCue.Blue, IROutputStandardCommands.IROut_BLUE },
//                    { CommonBoolCue.Digit0, IROutputStandardCommands.IROut_0 },
//                    { CommonBoolCue.Digit1, IROutputStandardCommands.IROut_1 },
//                    { CommonBoolCue.Digit2, IROutputStandardCommands.IROut_2 },
//                    { CommonBoolCue.Digit3, IROutputStandardCommands.IROut_3 },
//                    { CommonBoolCue.Digit4, IROutputStandardCommands.IROut_4 },
//                    { CommonBoolCue.Digit5, IROutputStandardCommands.IROut_5 },
//                    { CommonBoolCue.Digit6, IROutputStandardCommands.IROut_6 },
//                    { CommonBoolCue.Digit7, IROutputStandardCommands.IROut_7 },
//                    { CommonBoolCue.Digit8, IROutputStandardCommands.IROut_8 },
//                    { CommonBoolCue.Digit9, IROutputStandardCommands.IROut_9 },
//                    { CommonBoolCue.Dash, "DASH" },
//                };
//                return IrPort.GetUOsForIrCommands(numToIr);
//            }
//        }

//        #endregion

//        #region IHasSetTopBoxProperties Members

//        public bool HasDpad { get; set; }

//        public bool HasPreset { get; set; }

//        public bool HasDvr { get; set; }

//        public bool HasNumbers { get; set; }

//        #endregion
//    }
//}