using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.DM
{
    [Description("Wrapper Class for DM-RMC-4K-Z-100-C")]
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

            //removed to prevent NullReferenceException
            //_rmc.HdmiOutput.ConnectedDevice.DeviceInformationChange += ConnectedDevice_DeviceInformationChange;
		}

        void HdmiOutput_OutputStreamChange(EndpointOutputStream outputStream, EndpointOutputStreamEventArgs args)
        {
            switch (args.EventId)
            {
                case EndpointOutputStreamEventIds.FramesPerSecondFeedbackEventId:
                case EndpointOutputStreamEventIds.VerticalResolutionFeedbackEventId:
                case EndpointOutputStreamEventIds.HorizontalResolutionFeedbackEventId:
                    VideoOutputResolutionFeedback.FireUpdate();
                    break;
                case EndpointOutputStreamEventIds.HotplugDetectedEventId:
                    if (_rmc.HdmiOutput.ConnectedDevice == null) return;
                    EdidManufacturerFeedback.FireUpdate();
                    EdidNameFeedback.FireUpdate();
                    EdidPreferredTimingFeedback.FireUpdate();
                    EdidSerialNumberFeedback.FireUpdate();
                    break;
            }
        }

        /*
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
        }*/

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            LinkDmRmcToApi(this, trilist, joinStart, joinMapKey, bridge);
        }
    }
}