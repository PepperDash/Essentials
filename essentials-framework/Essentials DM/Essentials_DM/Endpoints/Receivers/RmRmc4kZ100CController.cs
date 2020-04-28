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

namespace PepperDash.Essentials.DM
{
    public class RmRmc4kZ100CController : DmRmcX100CController
    {
        public new DmRmc4kz100C Rmc { get; private set; }

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

        public RmRmc4kZ100CController(string key, string name, DmRmc4kz100C rmc)
			: base(key, name, rmc)
		{
			Rmc = rmc;
			DmIn = new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.DmCat, 0, this);
			HdmiOut = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.Audio | eRoutingSignalType.Video, 
				eRoutingPortConnectionType.Hdmi, null, this);

            // Set Ports for CEC
            HdmiOut.Port = Rmc; // Unique case, this class has no HdmiOutput port and ICec is implemented on the receiver class itself

            EdidManufacturerFeedback = new StringFeedback(() => Rmc.HdmiOutput.ConnectedDevice.Manufacturer.StringValue);
            EdidNameFeedback = new StringFeedback(() => Rmc.HdmiOutput.ConnectedDevice.Name.StringValue);
            EdidPreferredTimingFeedback = new StringFeedback(() => Rmc.HdmiOutput.ConnectedDevice.PreferredTiming.StringValue);
            EdidSerialNumberFeedback = new StringFeedback(() => Rmc.HdmiOutput.ConnectedDevice.SerialNumber.StringValue);

            Rmc.HdmiOutput.OutputStreamChange += HdmiOutput_OutputStreamChange;
            Rmc.HdmiOutput.ConnectedDevice.DeviceInformationChange += ConnectedDevice_DeviceInformationChange;
		}

        void HdmiOutput_OutputStreamChange(EndpointOutputStream outputStream, EndpointOutputStreamEventArgs args)
        {
            if (args.EventId == EndpointOutputStreamEventIds.HorizontalResolutionFeedbackEventId || args.EventId == EndpointOutputStreamEventIds.VerticalResolutionFeedbackEventId ||
                args.EventId == EndpointOutputStreamEventIds.FramesPerSecondFeedbackEventId)
            {
                VideoOutputResolutionFeedback.FireUpdate();
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
    }
}