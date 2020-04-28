using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Core;

namespace PepperDash.Essentials.DM
{
    public class DmRmc4kZScalerCController : DmRmcControllerBase, IRmcRouting,
        IIRInputPort, IComPorts, ICec
    {
        DmRmc4kzScalerC Rmc { get; private set; }

        public RoutingInputPort DmIn { get; private set; }
        public RoutingInputPort HdmiIn { get; private set; }
        public RoutingOutputPort HdmiOut { get; private set; }

        /// <summary>
        /// The value of the current video source for the HDMI output on the receiver
        /// </summary>
        public IntFeedback AudioVideoSourceNumericFeedback { get; private set; }

        public RoutingPortCollection<RoutingInputPort> InputPorts
        {
            get { return new RoutingPortCollection<RoutingInputPort> { DmIn, HdmiIn }; }
        }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts
        {
            get { return new RoutingPortCollection<RoutingOutputPort> { HdmiOut }; }
        }

        public DmRmc4kZScalerCController(string key, string name, DmRmc4kzScalerC rmc)
            : base(key, name, rmc)
        {
            Rmc = rmc;
            DmIn = new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.DmCat, 0, this);
            HdmiIn = new RoutingInputPort(DmPortName.HdmiIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, 0, this);
            HdmiOut = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, null, this);

            EdidManufacturerFeedback = new StringFeedback(() => Rmc.HdmiOutput.ConnectedDevice.Manufacturer.StringValue);
            EdidNameFeedback = new StringFeedback(() => Rmc.HdmiOutput.ConnectedDevice.Name.StringValue);
            EdidPreferredTimingFeedback = new StringFeedback(() => Rmc.HdmiOutput.ConnectedDevice.PreferredTiming.StringValue);
            EdidSerialNumberFeedback = new StringFeedback(() => Rmc.HdmiOutput.ConnectedDevice.SerialNumber.StringValue);

            VideoOutputResolutionFeedback = new StringFeedback(() => Rmc.HdmiOutput.GetVideoResolutionString());

            Rmc.HdmiOutput.OutputStreamChange += HdmiOutput_OutputStreamChange;
            Rmc.HdmiOutput.ConnectedDevice.DeviceInformationChange += ConnectedDevice_DeviceInformationChange;

            // Set Ports for CEC
            HdmiOut.Port = Rmc.HdmiOutput;

            AudioVideoSourceNumericFeedback = new IntFeedback(() => (ushort)(Rmc.SelectedSourceFeedback));
        }

        void HdmiOutput_OutputStreamChange(EndpointOutputStream outputStream, EndpointOutputStreamEventArgs args)
        {
            if (args.EventId == EndpointOutputStreamEventIds.HorizontalResolutionFeedbackEventId || args.EventId == EndpointOutputStreamEventIds.VerticalResolutionFeedbackEventId ||
                args.EventId == EndpointOutputStreamEventIds.FramesPerSecondFeedbackEventId)
            {
                VideoOutputResolutionFeedback.FireUpdate();
            }

            if (args.EventId == EndpointOutputStreamEventIds.SelectedSourceFeedbackEventId)
            {
                AudioVideoSourceNumericFeedback.FireUpdate();
            }
        }

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
        /// <summary>
        /// Gets the CEC stream directly from the HDMI port.
        /// </summary>
        public Cec StreamCec { get { return Rmc.HdmiOutput.StreamCec; } }
        #endregion


        #region IRmcRouting Members
        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            Debug.Console(2, this, "Attempting a route from input {0} to HDMI Output", inputSelector);

            var number = Convert.ToUInt16(inputSelector);

            Rmc.AudioVideoSource = (DmRmc4kzScalerC.eAudioVideoSource)number;
        }

        public void ExecuteNumericSwitch(uint inputSelector, uint outputSelector, eRoutingSignalType signalType)
        {
            Debug.Console(2, this, "Attempting a route from input {0} to HDMI Output", inputSelector);

            Rmc.AudioVideoSource = (DmRmc4kzScalerC.eAudioVideoSource)inputSelector;
        }
        #endregion

    }
}