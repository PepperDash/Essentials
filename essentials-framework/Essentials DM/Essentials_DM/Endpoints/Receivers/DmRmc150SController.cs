using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.DM
{
    /// <summary>
    /// Builds a controller for basic DM-RMCs with Com and IR ports and no control functions
    /// 
    /// </summary>
    public class DmRmc150SController : DmRmcControllerBase, IRoutingInputsOutputs,
        IIROutputPorts, IComPorts, ICec
    {
        public DmRmc150S Rmc { get; private set; }

        public RoutingInputPort DmIn { get; private set; }
        public RoutingOutputPort HdmiOut { get; private set; }

        public RoutingPortCollection<RoutingInputPort> InputPorts
        {
            get { return new RoutingPortCollection<RoutingInputPort> { DmIn }; }
        }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts
        {
            get { return new RoutingPortCollection<RoutingOutputPort> { HdmiOut }; }
        }

        /// <summary>
        ///  Make a Crestron RMC and put it in here
        /// </summary>
        public DmRmc150SController(string key, string name, DmRmc150S rmc)
            : base(key, name, rmc)
        {
            Rmc = rmc;
            DmIn = new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DmCat, 0, this);
            HdmiOut = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, null, this);

            EdidManufacturerFeedback = new StringFeedback(() => Rmc.HdmiOutput.ConnectedDevice.Manufacturer.StringValue);
            EdidNameFeedback = new StringFeedback(() => Rmc.HdmiOutput.ConnectedDevice.Name.StringValue);
            EdidPreferredTimingFeedback = new StringFeedback(() => Rmc.HdmiOutput.ConnectedDevice.PreferredTiming.StringValue);
            EdidSerialNumberFeedback = new StringFeedback(() => Rmc.HdmiOutput.ConnectedDevice.SerialNumber.StringValue);

            //VideoOutputResolutionFeedback = new StringFeedback(() => Rmc.HdmiOutput.GetVideoResolutionString());

            //Rmc.HdmiOutput.OutputStreamChange += HdmiOutput_OutputStreamChange;
            Rmc.HdmiOutput.ConnectedDevice.DeviceInformationChange += ConnectedDevice_DeviceInformationChange;

            // Set Ports for CEC
            HdmiOut.Port = Rmc.HdmiOutput;
        }

        //void HdmiOutput_OutputStreamChange(EndpointOutputStream outputStream, EndpointOutputStreamEventArgs args)
        //{
        //    if (args.EventId == EndpointOutputStreamEventIds.HorizontalResolutionFeedbackEventId || args.EventId == EndpointOutputStreamEventIds.VerticalResolutionFeedbackEventId ||
        //        args.EventId == EndpointOutputStreamEventIds.FramesPerSecondFeedbackEventId)
        //    {
        //        VideoOutputResolutionFeedback.FireUpdate();
        //    }
        //}

        void ConnectedDevice_DeviceInformationChange(ConnectedDeviceInformation connectedDevice, ConnectedDeviceEventArgs args)
        {
            if (args.EventId == ConnectedDeviceEventIds.ManufacturerEventId)
            {
                EdidManufacturerFeedback.FireUpdate();
            }
            else if (args.EventId == ConnectedDeviceEventIds.NameEventId)
            {
                EdidNameFeedback.FireUpdate();
            }
            else if (args.EventId == ConnectedDeviceEventIds.PreferredTimingEventId)
            {
                EdidPreferredTimingFeedback.FireUpdate();
            }
            else if (args.EventId == ConnectedDeviceEventIds.SerialNumberEventId)
            {
                EdidSerialNumberFeedback.FireUpdate();
            }
        }

        public override bool CustomActivate()
        {
            // Base does register and sets up comm monitoring.
            return base.CustomActivate();
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkDmRmcToApi(this, trilist, joinStart, joinMapKey, bridge);
        }

        #region IIROutputPorts Members
        public CrestronCollection<IROutputPort> IROutputPorts { get { return Rmc.IROutputPorts; } }
        public int NumberOfIROutputPorts { get { return Rmc.NumberOfIROutputPorts; } }
        #endregion

        #region IComPorts Members
        public CrestronCollection<ComPort> ComPorts { get { return Rmc.ComPorts; } }
        public int NumberOfComPorts { get { return Rmc.NumberOfComPorts; } }
        #endregion

        #region ICec Members
        public Cec StreamCec { get { return Rmc.HdmiOutput.StreamCec; } }
        #endregion
    }
}