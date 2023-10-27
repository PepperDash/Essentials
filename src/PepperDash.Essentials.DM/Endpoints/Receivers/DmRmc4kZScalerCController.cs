using System;
using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Core;
using PepperDash_Essentials_DM;

namespace PepperDash.Essentials.DM
{
    [Description("Wrapper Class for DM-RMC-4K-Z-SCALER-C")]
    public class DmRmc4kZScalerCController : DmRmcControllerBase, IRmcRoutingWithFeedback,
        IIROutputPorts, IComPorts, ICec, IRelayPorts, IHasDmInHdcp, IHasHdmiInHdcp
    {
        private readonly DmRmc4kzScalerC _rmc;

        public RoutingInputPort DmIn { get; private set; }
        public RoutingInputPort HdmiIn { get; private set; }
        public RoutingOutputPort HdmiOut { get; private set; }

        public IntFeedback DmInHdcpStateFeedback { get; private set; }
        public IntFeedback HdmiInHdcpStateFeedback { get; private set; }

        public BoolFeedback HdmiVideoSyncFeedback { get; private set; }



        /// <summary>
        /// The value of the current video source for the HDMI output on the receiver
        /// </summary>
        public IntFeedback AudioVideoSourceNumericFeedback { get; private set; }

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        //IroutingNumericEvent
        public event EventHandler<RoutingNumericEventArgs> NumericSwitchChange;

        /// <summary>
        /// Raise an event when the status of a switch object changes.
        /// </summary>
        /// <param name="e">Arguments defined as IKeyName sender, output, input, and eRoutingSignalType</param>
        private void OnSwitchChange(RoutingNumericEventArgs e)
        {
            var newEvent = NumericSwitchChange;
            if (newEvent != null) newEvent(this, e);
        }

        public DmRmc4kZScalerCController(string key, string name, DmRmc4kzScalerC rmc)
            : base(key, name, rmc)
        {
            _rmc = rmc;

            DmIn = new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.DmCat, 0, this)
            {
                FeedbackMatchObject = 1
            };
            HdmiIn = new RoutingInputPort(DmPortName.HdmiIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, 0, this)
            {
                FeedbackMatchObject = 2
            };
            HdmiOut = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, null, this);

            HdmiInHdcpStateFeedback = new IntFeedback("HdmiInHdcpCapability",
                () => (int)_rmc.HdmiIn.HdcpCapabilityFeedback);
            DmInHdcpStateFeedback = new IntFeedback("DmInHdcpCapability",
                () => (int)_rmc.DmInput.HdcpCapabilityFeedback);
            HdmiVideoSyncFeedback = new BoolFeedback("HdmiInVideoSync",
                () => _rmc.HdmiIn.SyncDetectedFeedback.BoolValue);

            AddToFeedbackList(HdmiInHdcpStateFeedback, DmInHdcpStateFeedback, HdmiVideoSyncFeedback);


            EdidManufacturerFeedback = new StringFeedback(() => _rmc.HdmiOutput.ConnectedDevice.Manufacturer.StringValue);
            EdidNameFeedback = new StringFeedback(() => _rmc.HdmiOutput.ConnectedDevice.Name.StringValue);
            EdidPreferredTimingFeedback = new StringFeedback(() => _rmc.HdmiOutput.ConnectedDevice.PreferredTiming.StringValue);
            EdidSerialNumberFeedback = new StringFeedback(() => _rmc.HdmiOutput.ConnectedDevice.SerialNumber.StringValue);

            VideoOutputResolutionFeedback = new StringFeedback(() => _rmc.HdmiOutput.GetVideoResolutionString());

            InputPorts = new RoutingPortCollection<RoutingInputPort> { DmIn, HdmiIn };
            OutputPorts = new RoutingPortCollection<RoutingOutputPort> { HdmiOut };

            _rmc.HdmiOutput.OutputStreamChange += HdmiOutput_OutputStreamChange;
            _rmc.HdmiOutput.ConnectedDevice.DeviceInformationChange += ConnectedDevice_DeviceInformationChange;
            _rmc.HdmiIn.InputStreamChange += InputStreamChangeEvent;
            _rmc.DmInput.InputStreamChange += InputStreamChangeEvent;

            _rmc.OnlineStatusChange += _rmc_OnlineStatusChange;

            // Set Ports for CEC
            HdmiOut.Port = _rmc.HdmiOutput;

            AudioVideoSourceNumericFeedback = new IntFeedback(() => (ushort)(_rmc.SelectedSourceFeedback));
        }

        void InputStreamChangeEvent(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
        {
            switch (args.EventId)
            {
                case EndpointInputStreamEventIds.HdcpCapabilityFeedbackEventId:
                    if (inputStream == _rmc.HdmiIn) HdmiInHdcpStateFeedback.FireUpdate();
                    if (inputStream == _rmc.DmInput) DmInHdcpStateFeedback.FireUpdate();
                    break;
                case EndpointInputStreamEventIds.SyncDetectedFeedbackEventId:
                    if (inputStream == _rmc.HdmiIn) HdmiVideoSyncFeedback.FireUpdate();
                    break;
            }
        }

        private void _rmc_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            AudioVideoSourceNumericFeedback.FireUpdate();
            OnSwitchChange(new RoutingNumericEventArgs(1, AudioVideoSourceNumericFeedback.UShortValue, eRoutingSignalType.AudioVideo));
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
                var localInputPort =
                    InputPorts.FirstOrDefault(p => (int)p.FeedbackMatchObject == AudioVideoSourceNumericFeedback.UShortValue);


                AudioVideoSourceNumericFeedback.FireUpdate();
                OnSwitchChange(new RoutingNumericEventArgs(1, AudioVideoSourceNumericFeedback.UShortValue, OutputPorts.First(), localInputPort, eRoutingSignalType.AudioVideo));
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

        #region IIROutputPorts Members
        public CrestronCollection<IROutputPort> IROutputPorts { get { return _rmc.IROutputPorts; } }
        public int NumberOfIROutputPorts { get { return _rmc.NumberOfIROutputPorts; } }
        #endregion

        #region IComPorts Members
        public CrestronCollection<ComPort> ComPorts { get { return _rmc.ComPorts; } }
        public int NumberOfComPorts { get { return _rmc.NumberOfComPorts; } }
        #endregion

        #region ICec Members
        /// <summary>
        /// Gets the CEC stream directly from the HDMI port.
        /// </summary>
        public Cec StreamCec { get { return _rmc.HdmiOutput.StreamCec; } }
        #endregion


        #region IRmcRouting Members
        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            Debug.Console(2, this, "Attempting a route from input {0} to HDMI Output", inputSelector);

            var number = Convert.ToUInt16(inputSelector);

            _rmc.AudioVideoSource = (DmRmc4kzScalerC.eAudioVideoSource)number;
        }

        public void ExecuteNumericSwitch(ushort inputSelector, ushort outputSelector, eRoutingSignalType signalType)
        {
            Debug.Console(2, this, "Attempting a route from input {0} to HDMI Output", inputSelector);

            _rmc.AudioVideoSource = (DmRmc4kzScalerC.eAudioVideoSource)inputSelector;
        }
        #endregion

        #region Implementation of IRelayPorts

        public CrestronCollection<Relay> RelayPorts
        {
            get { return _rmc.RelayPorts; }
        }

        public int NumberOfRelayPorts
        {
            get { return _rmc.NumberOfRelayPorts; }
        }

        #endregion


        public eHdcpCapabilityType DmInHdcpCapability
        {
            get { return eHdcpCapabilityType.Hdcp2_2Support; }
        }

        public void SetDmInHdcpState(eHdcpCapabilityType hdcpState)
        {

            if (_rmc == null) return;
            _rmc.DmInput.HdcpCapability = hdcpState;
        }


        public eHdcpCapabilityType HdmiInHdcpCapability
        {
            get { return eHdcpCapabilityType.Hdcp2_2Support; }
        }

        public void SetHdmiInHdcpState(eHdcpCapabilityType hdcpState)
        {
            if (_rmc == null) return;
            _rmc.HdmiIn.HdcpCapability = hdcpState;
        }

    }
}