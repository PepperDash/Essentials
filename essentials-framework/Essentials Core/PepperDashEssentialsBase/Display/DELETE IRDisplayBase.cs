//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharpPro;

//namespace PepperDash.Essentials.Core
//{
//    public abstract class IRDisplayBase : DisplayBase, IHasCueActionList
//    {
//        public IrOutputPortController IrPort { get; private set; }
//        /// <summary>
//        /// Default to 200ms
//        /// </summary>
//        public ushort IrPulseTime { get; set; }
//        bool _PowerIsOn;
//        bool _IsWarmingUp;
//        bool _IsCoolingDown;

//        /// <summary>
//        /// FunctionList is pre-defined to have power commands.
//        /// </summary>
//        public IRDisplayBase(string key, string name, IROutputPort port, string irDriverFilepath)
//            : base(key, name)
//        {
//            IrPort = new IrOutputPortController("ir-" + key, port, irDriverFilepath);
//            IrPulseTime = 200;
//            WarmupTime = 7000;
//            CooldownTime = 10000;

//            CueActionList = new List<CueActionPair>
//            {
//                new BoolCueActionPair(CommonBoolCue.Power, b=> PowerToggle()),
//                new BoolCueActionPair(CommonBoolCue.PowerOn, b=> PowerOn()),
//                new BoolCueActionPair(CommonBoolCue.PowerOff, b=> PowerOff()),
//            };
//        }

//        public override void PowerOn()
//        {
//            if (!PowerIsOnFeedback.BoolValue && !_IsWarmingUp && !_IsCoolingDown)
//            {
//                _IsWarmingUp = true;
//                IsWarmingUpFeedback.FireUpdate();
//                // Fake power-up cycle
//                WarmupTimer = new CTimer(o =>
//                {
//                    _IsWarmingUp = false;
//                    _PowerIsOn = true;
//                    IsWarmingUpFeedback.FireUpdate();
//                    PowerIsOnFeedback.FireUpdate();
//                }, WarmupTime);
//            }
//            IrPort.Pulse(IROutputStandardCommands.IROut_POWER_ON, IrPulseTime);
//        }

//        public override void PowerOff()
//        {
//            // If a display has unreliable-power off feedback, just override this and
//            // remove this check.
//            if (PowerIsOnFeedback.BoolValue && !_IsWarmingUp && !_IsCoolingDown)
//            {
//                _IsCoolingDown = true;
//                _PowerIsOn = false;
//                PowerIsOnFeedback.FireUpdate();
//                IsCoolingDownFeedback.FireUpdate();
//                // Fake cool-down cycle
//                CooldownTimer = new CTimer(o =>
//                {
//                    _IsCoolingDown = false;
//                    IsCoolingDownFeedback.FireUpdate();
//                }, CooldownTime);
//            }
//            IrPort.Pulse(IROutputStandardCommands.IROut_POWER_OFF, IrPulseTime);
//        }

//        public override void PowerToggle()
//        {
//            // Not sure how to handle the feedback, but we should default to power off fb.
//            // Does this need to trigger feedback??
//            _PowerIsOn = false;
//            IrPort.Pulse(IROutputStandardCommands.IROut_POWER, IrPulseTime);
//        }

//        #region IFunctionList Members

//        public List<CueActionPair> CueActionList
//        {
//            get;
//            private set;
//        }

//        #endregion

//        protected override Func<bool> PowerIsOnOutputFunc { get { return () => _PowerIsOn; } }
//        protected override Func<bool> IsCoolingDownOutputFunc { get { return () => _IsCoolingDown; } }
//        protected override Func<bool> IsWarmingUpOutputFunc { get { return () => _IsWarmingUp; } }

//        public override void ExecuteSwitch(object selector)
//        {
//            IrPort.Pulse((string)selector, IrPulseTime);
//        }
//    }
//}