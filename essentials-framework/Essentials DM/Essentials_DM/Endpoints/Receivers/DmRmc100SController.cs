using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.DM.Config;

namespace PepperDash.Essentials.DM
{
    /// <summary>
    /// Builds a controller for basic DM-RMCs with Com and IR ports and no control functions
    /// 
    /// </summary>
    public class DmRmc100SController : DmRmcControllerBase, IRoutingInputsOutputs,
        IIROutputPorts, IComPorts, ICec
    {
        private DmRmc100S _rmc;

        public RoutingInputPort DmIn { get; private set; }
        public RoutingOutputPort HdmiOut { get; private set; }

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        /// <summary>
        ///  Make a Crestron RMC and put it in here
        /// </summary>
        public DmRmc100SController(string key, string name, DmRmc100S rmc)
            : base(key, name, rmc)
        {
            _rmc = rmc;

            InitializeRouting();
        }

        /// <summary>
        /// Defers creation of the actual hardware until the Preactivation phase
        /// </summary>
        /// <param name="key">Device Key</param>
        /// <param name="name">Device Name</param>
        /// <param name="props">Config from file</param>
        /// <param name="preActivationFunc">Function to run to get the DM Chassis</param>
        public DmRmc100SController(string key, string name, DmRmcPropertiesConfig props, Func<string, string, IDmSwitch> preActivationFunc ):base(key, name)
        {
            AddPreActivationAction(() =>
            {
                var dmSwitch = preActivationFunc(Key, props.ParentDeviceKey);

                var chassis = dmSwitch.Chassis;

                var dmOut = DmRmcHelper.AddRmcAndGetDmOutput(Key, props.ParentOutputNumber, dmSwitch);

                if (chassis is DmMd8x8Cpu3 || chassis is DmMd16x16Cpu3 ||
                    chassis is DmMd32x32Cpu3 || chassis is DmMd8x8Cpu3rps ||
                    chassis is DmMd16x16Cpu3rps || chassis is DmMd32x32Cpu3rps ||
                    chassis is DmMd128x128 || chassis is DmMd64x64)
                {

                    _rmc = new DmRmc100S(dmOut);
                }
                else
                {
                    _rmc = new DmRmc100S(props.Control.IpIdInt, dmOut);    
                }

                SetBaseClassRmcs();

                InitializeRouting();
            });
        }

        private void SetBaseClassRmcs()
        {
            //Set Rmc in DmRmcControllerBase Class
            SetRmc(_rmc);

            //Set Rmc In CrestronGenericBaseDevice
            SetHardwareAndRegisterEvents(_rmc);
        }

        private void InitializeRouting()
        {
            DmIn = new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.DmCat, 0, this);
            HdmiOut = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, null, this) { Port = _rmc };

            InputPorts = new RoutingPortCollection<RoutingInputPort> { DmIn };
            OutputPorts = new RoutingPortCollection<RoutingOutputPort> { HdmiOut };
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkDmRmcToApi(this, trilist, joinStart, joinMapKey, bridge);
        }

        #region IIROutputPorts Members
        public CrestronCollection<IROutputPort> IROutputPorts { get { return _rmc.IROutputPorts; } }
        public int NumberOfIROutputPorts { get { return _rmc.NumberOfIROutputPorts; } }
        #endregion

        #region IComPorts Members
        public CrestronCollection<ComPort> ComPorts { get { return _rmc.ComPorts; } }
        public int NumberOfComPorts { get { return _rmc.NumberOfComPorts; } }
        #endregion

        #region ICec Members
        public Cec StreamCec { get { return _rmc.HdmiOutput.StreamCec; } }
        #endregion
    }
}