using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.DM
{
    public class DmRmc4kZ100CController : DmRmcX100CController
    {
        private readonly DmRmc4kz100C _rmc;

        public DmRmc4kZ100CController(string key, string name, DmRmc4kz100C rmc)
			: base(key, name, rmc)
        {
            _rmc = rmc;

            EdidManufacturerFeedback = new StringFeedback(() => _rmc.HdmiOutput.ConnectedDevice.Manufacturer.StringValue);
            EdidNameFeedback = new StringFeedback(() => _rmc.HdmiOutput.ConnectedDevice.Name.StringValue);
            EdidPreferredTimingFeedback = new StringFeedback(() => _rmc.HdmiOutput.ConnectedDevice.PreferredTiming.StringValue);
            EdidSerialNumberFeedback = new StringFeedback(() => _rmc.HdmiOutput.ConnectedDevice.SerialNumber.StringValue);

            _rmc.HdmiOutput.OutputStreamChange += HdmiOutput_OutputStreamChange;
            _rmc.HdmiOutput.ConnectedDevice.DeviceInformationChange += ConnectedDevice_DeviceInformationChange;
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
            switch (args.EventId)
            {
                case ConnectedDeviceEventIds.ManufacturerEventId:
                    EdidManufacturerFeedback.FireUpdate();
                    break;
                case ConnectedDeviceEventIds.NameEventId:
                    EdidNameFeedback.FireUpdate();
                    break;
                case ConnectedDeviceEventIds.PreferredTimingEventId:
                    EdidPreferredTimingFeedback.FireUpdate();
                    break;
                case ConnectedDeviceEventIds.SerialNumberEventId:
                    EdidSerialNumberFeedback.FireUpdate();
                    break;
            }
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkDmRmcToApi(this, trilist, joinStart, joinMapKey, bridge);
        }
    }
}