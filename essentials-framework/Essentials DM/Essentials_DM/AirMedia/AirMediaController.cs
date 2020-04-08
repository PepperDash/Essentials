using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.DeviceSupport.Support;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.AirMedia;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.DM.AirMedia
{
    public class AirMediaController : CrestronGenericBaseDevice, IRoutingInputsOutputs, IIROutputPorts, IComPorts
    {
        public AmX00 AirMedia { get; private set; }

        public DeviceConfig DeviceConfig { get; private set; }

        AirMediaPropertiesConfig PropertiesConfig;

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        public BoolFeedback IsInSessionFeedback { get; private set; }
        public IntFeedback ErrorFeedback { get; private set; }
        public IntFeedback NumberOfUsersConnectedFeedback { get; set; }
        public IntFeedback LoginCodeFeedback { get; set; }
        public StringFeedback ConnectionAddressFeedback { get; set; }
        public StringFeedback HostnameFeedback { get; set; }
        public IntFeedback VideoOutFeedback { get; private set; }
        public BoolFeedback HdmiVideoSyncDetectedFeedback { get; private set; }
        public StringFeedback SerialNumberFeedback { get; private set; }
        public BoolFeedback AutomaticInputRoutingEnabledFeedback { get; private set; }

        public AirMediaController(string key, string name, AmX00 device, DeviceConfig dc, AirMediaPropertiesConfig props)
            :base(key, name, device)
        {
            AirMedia = device;

            DeviceConfig = dc;

            PropertiesConfig = props;

            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

            InputPorts.Add(new RoutingInputPort(DmPortName.Osd, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.None, new Action(SelectPinPointUxLandingPage), this));

            InputPorts.Add(new RoutingInputPort(DmPortName.AirMediaIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Streaming, new Action(SelectAirMedia), this));

            InputPorts.Add(new RoutingInputPort(DmPortName.HdmiIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                eRoutingPortConnectionType.Hdmi, new Action(SelectHdmiIn), this));

            InputPorts.Add(new RoutingInputPort(DmPortName.AirBoardIn, eRoutingSignalType.Video,
                eRoutingPortConnectionType.None, new Action(SelectAirboardIn), this));

            if (AirMedia is Am300)
            {
                InputPorts.Add(new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.Audio | eRoutingSignalType.Video,
                    eRoutingPortConnectionType.DmCat, new Action(SelectDmIn), this));
            }

            AirMedia.AirMedia.AirMediaChange += new Crestron.SimplSharpPro.DeviceSupport.GenericEventHandler(AirMedia_AirMediaChange);

            IsInSessionFeedback = new BoolFeedback( new Func<bool>(() => AirMedia.AirMedia.StatusFeedback.UShortValue == 0 ));
            ErrorFeedback = new IntFeedback(new Func<int>(() => AirMedia.AirMedia.ErrorFeedback.UShortValue));
            NumberOfUsersConnectedFeedback = new IntFeedback(new Func<int>(() => AirMedia.AirMedia.NumberOfUsersConnectedFeedback.UShortValue));
            LoginCodeFeedback = new IntFeedback(new Func<int>(() => AirMedia.AirMedia.LoginCodeFeedback.UShortValue));
            ConnectionAddressFeedback = new StringFeedback(new Func<string>(() => AirMedia.AirMedia.ConnectionAddressFeedback.StringValue));
            HostnameFeedback = new StringFeedback(new Func<string>(() => AirMedia.AirMedia.HostNameFeedback.StringValue));

            // TODO: Figure out if we can actually get the TSID/Serial
            SerialNumberFeedback = new StringFeedback(new Func<string>(() => "unknown"));

            AirMedia.DisplayControl.DisplayControlChange += new Crestron.SimplSharpPro.DeviceSupport.GenericEventHandler(DisplayControl_DisplayControlChange);

            VideoOutFeedback = new IntFeedback(new Func<int>(() => Convert.ToInt16(AirMedia.DisplayControl.VideoOutFeedback)));
            AutomaticInputRoutingEnabledFeedback = new BoolFeedback(new Func<bool>(() => AirMedia.DisplayControl.EnableAutomaticRoutingFeedback.BoolValue));

            AirMedia.HdmiIn.StreamChange += new Crestron.SimplSharpPro.DeviceSupport.StreamEventHandler(HdmiIn_StreamChange);

            HdmiVideoSyncDetectedFeedback = new BoolFeedback(new Func<bool>(() => AirMedia.HdmiIn.SyncDetectedFeedback.BoolValue));
        }

        public override bool CustomActivate()
        {
            if (PropertiesConfig.AutoSwitchingEnabled)
                AirMedia.DisplayControl.EnableAutomaticRouting();
            else
                AirMedia.DisplayControl.DisableAutomaticRouting();

            return base.CustomActivate();
        }

        void AirMedia_AirMediaChange(object sender, Crestron.SimplSharpPro.DeviceSupport.GenericEventArgs args)
        {
            if (args.EventId == AirMediaInputSlot.AirMediaStatusFeedbackEventId)
                IsInSessionFeedback.FireUpdate();
            else if (args.EventId == AirMediaInputSlot.AirMediaErrorFeedbackEventId)
                ErrorFeedback.FireUpdate();
            else if (args.EventId == AirMediaInputSlot.AirMediaNumberOfUserConnectedEventId)
                NumberOfUsersConnectedFeedback.FireUpdate();
            else if (args.EventId == AirMediaInputSlot.AirMediaLoginCodeEventId)
                LoginCodeFeedback.FireUpdate();
            else if (args.EventId == AirMediaInputSlot.AirMediaConnectionAddressFeedbackEventId)
                ConnectionAddressFeedback.FireUpdate();
            else if (args.EventId == AirMediaInputSlot.AirMediaHostNameFeedbackEventId)
                HostnameFeedback.FireUpdate();
        }

        void DisplayControl_DisplayControlChange(object sender, Crestron.SimplSharpPro.DeviceSupport.GenericEventArgs args)
        {
            if (args.EventId == AmX00.VideoOutFeedbackEventId)
                VideoOutFeedback.FireUpdate();
            else if (args.EventId == AmX00.EnableAutomaticRoutingFeedbackEventId)
                AutomaticInputRoutingEnabledFeedback.FireUpdate();
        }

        void HdmiIn_StreamChange(Crestron.SimplSharpPro.DeviceSupport.Stream stream, Crestron.SimplSharpPro.DeviceSupport.StreamEventArgs args)
        {
            if (args.EventId == DMInputEventIds.SourceSyncEventId)
                HdmiVideoSyncDetectedFeedback.FireUpdate();
        }

        /// <summary>
        /// Sets the VideoOut source ( 0 = PinpointUX, 1 = AirMedia, 2 = HDMI, 3 = DM, 4 = Airboard )
        /// </summary>
        /// <param name="source">source number</param>
        public void SelectVideoOut(uint source)
        {
            AirMedia.DisplayControl.VideoOut = (AmX00DisplayControl.eAirMediaX00VideoSource)source;
        }

        /// <summary>
        /// Selects the PinPointUXLandingPage input
        /// </summary>
        public void SelectPinPointUxLandingPage()
        {
            AirMedia.DisplayControl.VideoOut = AmX00DisplayControl.eAirMediaX00VideoSource.PinPointUxLandingPage;
        }

        /// <summary>
        /// Selects the AirMedia input
        /// </summary>
        public void SelectAirMedia()
        {
            AirMedia.DisplayControl.VideoOut = AmX00DisplayControl.eAirMediaX00VideoSource.AirMedia;
        }

        /// <summary>
        /// Selects the DM input
        /// </summary>
        public void SelectDmIn()
        {
            AirMedia.DisplayControl.VideoOut = AmX00DisplayControl.eAirMediaX00VideoSource.HDMI;
        }

        /// <summary>
        /// Selects the HDMI INput
        /// </summary>
        public void SelectHdmiIn()
        {
            AirMedia.DisplayControl.VideoOut = AmX00DisplayControl.eAirMediaX00VideoSource.DM;
        }

        public void SelectAirboardIn()
        {
            AirMedia.DisplayControl.VideoOut = AmX00DisplayControl.eAirMediaX00VideoSource.AirBoard;
        }

        /// <summary>
        /// Reboots the device
        /// </summary>
        public void RebootDevice()
        {
            AirMedia.AirMedia.DeviceReboot();
        }

        #region IIROutputPorts Members

        public CrestronCollection<IROutputPort> IROutputPorts
        {
            get { return AirMedia.IROutputPorts; }
        }

        public int NumberOfIROutputPorts
        {
            get { return AirMedia.NumberOfIROutputPorts; }
        }



        #endregion



        #region IComPorts Members

        public CrestronCollection<ComPort> ComPorts
        {
            get { return AirMedia.ComPorts; }
        }

        public int NumberOfComPorts
        {
            get { return AirMedia.NumberOfComPorts; }
        }

        #endregion


    }
}