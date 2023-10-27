extern alias Full;

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
using Full.Newtonsoft.Json;
using Full.Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.DM.AirMedia
{
    [Description("Wrapper class for an AM-200 or AM-300")]
    public class AirMediaController : CrestronGenericBridgeableBaseDevice, IRoutingNumericWithFeedback, IIROutputPorts, IComPorts
    {
        public Am3x00 AirMedia { get; private set; }

        public DeviceConfig DeviceConfig { get; private set; }

        AirMediaPropertiesConfig PropertiesConfig;

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }


        //IroutingNumericEvent
        public event EventHandler<RoutingNumericEventArgs> NumericSwitchChange;

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

        public AirMediaController(string key, string name, Am3x00 device, DeviceConfig dc, AirMediaPropertiesConfig props)
            : base(key, name, device)
        {

            AirMedia = device;

            DeviceConfig = dc;

            PropertiesConfig = props;

            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

            InputPorts.Add(new RoutingInputPort(DmPortName.Osd, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.None, new Action(SelectPinPointUxLandingPage), this)
            {
                FeedbackMatchObject = 0
            });

            InputPorts.Add(new RoutingInputPort(DmPortName.AirMediaIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Streaming, new Action(SelectAirMedia), this)
            {
                FeedbackMatchObject = 1
            });

            InputPorts.Add(new RoutingInputPort(DmPortName.HdmiIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, new Action(SelectHdmiIn), this)
                {
                    FeedbackMatchObject = 2
                });

            InputPorts.Add(new RoutingInputPort(DmPortName.AirBoardIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.None, new Action(SelectAirboardIn), this)
                {
                    FeedbackMatchObject = 4
                });

            if (AirMedia is Am300)
            {
                InputPorts.Add(new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.AudioVideo,
                    eRoutingPortConnectionType.DmCat, new Action(SelectDmIn), this)
                    {
                        FeedbackMatchObject = 3
                    });
            }

            OutputPorts.Add(new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, null, this));

            AirMedia.AirMedia.AirMediaChange += new Crestron.SimplSharpPro.DeviceSupport.GenericEventHandler(AirMedia_AirMediaChange);

            IsInSessionFeedback = new BoolFeedback(() => AirMedia.AirMedia.StatusFeedback.UShortValue == 0);
            ErrorFeedback = new IntFeedback(() => AirMedia.AirMedia.ErrorFeedback.UShortValue);
            NumberOfUsersConnectedFeedback = new IntFeedback(() => AirMedia.AirMedia.NumberOfUsersConnectedFeedback.UShortValue);
            LoginCodeFeedback = new IntFeedback(() => AirMedia.AirMedia.LoginCodeFeedback.UShortValue);
            ConnectionAddressFeedback = new StringFeedback(() => AirMedia.AirMedia.ConnectionAddressFeedback.StringValue);
            HostnameFeedback = new StringFeedback(() => AirMedia.AirMedia.HostNameFeedback.StringValue);

            // TODO: Figure out if we can actually get the TSID/Serial
            SerialNumberFeedback = new StringFeedback(() => "unknown");

            AirMedia.DisplayControl.DisplayControlChange += DisplayControl_DisplayControlChange;

            VideoOutFeedback = new IntFeedback(() => Convert.ToInt16(AirMedia.DisplayControl.VideoOutFeedback));
            AutomaticInputRoutingEnabledFeedback = new BoolFeedback(() => AirMedia.DisplayControl.EnableAutomaticRoutingFeedback.BoolValue);

            // Not all AirMedia versions support HDMI In like the 3200
            if (AirMedia.HdmiIn != null)
            {
                AirMedia.HdmiIn.StreamChange += HdmiIn_StreamChange;
                HdmiVideoSyncDetectedFeedback = new BoolFeedback(() => AirMedia.HdmiIn.SyncDetectedFeedback.BoolValue);
                return;
            }

            // Return false if the AirMedia device doesn't support HDMI Input
            HdmiVideoSyncDetectedFeedback = new BoolFeedback(() => false);
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

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

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

        /// <summary>
        /// Raise an event when the status of a switch object changes.
        /// </summary>
        /// <param name="e">Arguments defined as IKeyName sender, output, input, and eRoutingSignalType</param>
        private void OnSwitchChange(RoutingNumericEventArgs e)
        {
            var handler = NumericSwitchChange;

            if (handler == null) return;
                
            handler(this, e);
        }


        void AirMedia_AirMediaChange(object sender, Crestron.SimplSharpPro.DeviceSupport.GenericEventArgs args)
        {
            switch (args.EventId)
            {
                case AirMediaInputSlot.AirMediaStatusFeedbackEventId:
                    {
                        IsInSessionFeedback.FireUpdate();
                        break;
                    }
                case AirMediaInputSlot.AirMediaErrorFeedbackEventId:
                    {
                        ErrorFeedback.FireUpdate();
                        break;
                    }
                case AirMediaInputSlot.AirMediaNumberOfUserConnectedEventId:
                    {
                        NumberOfUsersConnectedFeedback.FireUpdate();
                        break;
                    }
                case AirMediaInputSlot.AirMediaLoginCodeEventId:
                    {
                        LoginCodeFeedback.FireUpdate();
                        break;
                    }
                case AirMediaInputSlot.AirMediaConnectionAddressFeedbackEventId:
                    {
                        ConnectionAddressFeedback.FireUpdate();
                        break;
                    }
                case AirMediaInputSlot.AirMediaHostNameFeedbackEventId:
                    {
                        HostnameFeedback.FireUpdate();
                        break;
                    }
            }
        }

        void DisplayControl_DisplayControlChange(object sender, Crestron.SimplSharpPro.DeviceSupport.GenericEventArgs args)
        {
                VideoOutFeedback.FireUpdate();

                var localInputPort =
                    InputPorts.FirstOrDefault(p => (int) p.FeedbackMatchObject == VideoOutFeedback.UShortValue);

                OnSwitchChange(new RoutingNumericEventArgs(1, VideoOutFeedback.UShortValue, OutputPorts.First(),
                    localInputPort, eRoutingSignalType.AudioVideo));
            
                AutomaticInputRoutingEnabledFeedback.FireUpdate();
        }

        void HdmiIn_StreamChange(Stream stream, Crestron.SimplSharpPro.DeviceSupport.StreamEventArgs args)
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
            TypeNames = new List<string>() { "am200", "am300", "am3200" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var type = dc.Type.ToLower();

            Debug.Console(1, "Factory Attempting to create new AirMedia Device");

            var props = dc.Properties.ToObject<AirMediaPropertiesConfig>();
            Am3x00 amDevice = null;
            switch (type)
            {
                case "am200" :
                {
                    amDevice = new Am200(props.Control.IpIdInt, Global.ControlSystem);
                    break;
                }
                case "am300" :
                {
                    amDevice = new Am300(props.Control.IpIdInt, Global.ControlSystem);
                    break;
                }
                case "am3200" :
                {
                    amDevice = new Am3200(props.Control.IpIdInt, Global.ControlSystem);
                    break;
                }
            }

            return new AirMediaController(dc.Key, dc.Name, amDevice, dc, props);

        }
    }
}