using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.DeviceSupport.Support;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.AirMedia;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.DM.AirMedia
{
    [Description("Wrapper class for an AM-200 or AM-300")]
    public class AirMediaController : CrestronGenericBridgeableBaseDevice, IRoutingNumeric, IIROutputPorts, IComPorts
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
            : base(key, name, device)
        {
            AirMedia = device;

            DeviceConfig = dc;

            PropertiesConfig = props;

            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

            InputPorts.Add(new RoutingInputPort(DmPortName.Osd, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.None, new Action(SelectPinPointUxLandingPage), this));

            InputPorts.Add(new RoutingInputPort(DmPortName.AirMediaIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Streaming, new Action(SelectAirMedia), this));

            InputPorts.Add(new RoutingInputPort(DmPortName.HdmiIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, new Action(SelectHdmiIn), this));

            InputPorts.Add(new RoutingInputPort(DmPortName.AirBoardIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.None, new Action(SelectAirboardIn), this));

            if (AirMedia is Am300)
            {
                InputPorts.Add(new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.AudioVideo,
                    eRoutingPortConnectionType.DmCat, new Action(SelectDmIn), this));
            }

            OutputPorts.Add(new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, null, this));

            AirMedia.AirMedia.AirMediaChange += new Crestron.SimplSharpPro.DeviceSupport.GenericEventHandler(AirMedia_AirMediaChange);

            IsInSessionFeedback = new BoolFeedback(new Func<bool>(() => AirMedia.AirMedia.StatusFeedback.UShortValue == 0));
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

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new AirMediaControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<AirMediaControllerJoinMap>(joinMapSerialized);

            bridge.AddJoinMap(Key, joinMap);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to Airmedia: {0}", Name);

            trilist.StringInput[joinMap.Name.JoinNumber].StringValue = Name;

            var commMonitor = this as ICommunicationMonitor;

            commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);

            IsInSessionFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsInSession.JoinNumber]);
            HdmiVideoSyncDetectedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.HdmiVideoSync.JoinNumber]);

            trilist.SetSigTrueAction(joinMap.AutomaticInputRoutingEnabled.JoinNumber, AirMedia.DisplayControl.EnableAutomaticRouting);
            trilist.SetSigFalseAction(joinMap.AutomaticInputRoutingEnabled.JoinNumber, AirMedia.DisplayControl.DisableAutomaticRouting);
            AutomaticInputRoutingEnabledFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AutomaticInputRoutingEnabled.JoinNumber]);

            trilist.SetUShortSigAction(joinMap.VideoOut.JoinNumber, (u) => SelectVideoOut(u));

            VideoOutFeedback.LinkInputSig(trilist.UShortInput[joinMap.VideoOut.JoinNumber]);
            ErrorFeedback.LinkInputSig(trilist.UShortInput[joinMap.ErrorFB.JoinNumber]);
            NumberOfUsersConnectedFeedback.LinkInputSig(trilist.UShortInput[joinMap.NumberOfUsersConnectedFB.JoinNumber]);

            trilist.SetUShortSigAction(joinMap.LoginCode.JoinNumber, (u) => AirMedia.AirMedia.LoginCode.UShortValue = u);
            LoginCodeFeedback.LinkInputSig(trilist.UShortInput[joinMap.LoginCode.JoinNumber]);

            ConnectionAddressFeedback.LinkInputSig(trilist.StringInput[joinMap.ConnectionAddressFB.JoinNumber]);
            HostnameFeedback.LinkInputSig(trilist.StringInput[joinMap.HostnameFB.JoinNumber]);
            SerialNumberFeedback.LinkInputSig(trilist.StringInput[joinMap.SerialNumberFeedback.JoinNumber]);
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
            AirMedia.DisplayControl.VideoOut = AmX00DisplayControl.eAirMediaX00VideoSource.DM;
        }

        /// <summary>
        /// Selects the HDMI INput
        /// </summary>
        public void SelectHdmiIn()
        {
            AirMedia.DisplayControl.VideoOut = AmX00DisplayControl.eAirMediaX00VideoSource.HDMI;
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



        #region IRoutingNumeric Members

        public void ExecuteNumericSwitch(ushort input, ushort output, eRoutingSignalType signalType)
        {
            if ((signalType & eRoutingSignalType.Video) != eRoutingSignalType.Video) return;
            if (!Enum.IsDefined(typeof (AmX00DisplayControl.eAirMediaX00VideoSource), input))
            {
                Debug.Console(2, this, "Invalid Video Source Index : {0}", input);
                return;
            }
            AirMedia.DisplayControl.VideoOut = (AmX00DisplayControl.eAirMediaX00VideoSource) input;
        }

        #endregion

        #region IRouting Members

        public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
        {
            Debug.Console(2, this, "Input Selector = {0}", inputSelector.ToString());
            var handler = inputSelector as Action;
            if (handler == null) return;
            handler();
        }

        #endregion
    }

    public class AirMediaControllerFactory : EssentialsDeviceFactory<AirMediaController>
    {
        public AirMediaControllerFactory()
        {
            TypeNames = new List<string>() { "am200", "am300" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var type = dc.Type.ToLower();

            Debug.Console(1, "Factory Attempting to create new AirMedia Device");

            var props = JsonConvert.DeserializeObject<AirMediaPropertiesConfig>(dc.Properties.ToString());
            AmX00 amDevice = null;
            if (type == "am200")
                amDevice = new Crestron.SimplSharpPro.DM.AirMedia.Am200(props.Control.IpIdInt, Global.ControlSystem);
            else if (type == "am300")
                amDevice = new Crestron.SimplSharpPro.DM.AirMedia.Am300(props.Control.IpIdInt, Global.ControlSystem);

            return new AirMediaController(dc.Key, dc.Name, amDevice, dc, props);

        }
    }
}