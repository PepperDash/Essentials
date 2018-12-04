using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.EthernetCommunication;

using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Essentials.Devices.Common;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.DM;

namespace PepperDash.Essentials.Bridges
{
    /// <summary>
    /// Base class for all bridge class variants
    /// </summary>
    public class BridgeBase : Device
    {
        public BridgeApi Api { get; private set; }

        public BridgeBase(string key) :
            base(key)
        {

        }
            
    }

    /// <summary>
    /// Base class for bridge API variants
    /// </summary>
    public abstract class BridgeApi : Device
    {
        public BridgeApi(string key) :
            base(key)
        {

        }

    }

    /// <summary>
    /// Bridge API using EISC
    /// </summary>
    public class EiscApi : BridgeApi
    {
        public EiscApiPropertiesConfig PropertiesConfig { get; private set; }

        public ThreeSeriesTcpIpEthernetIntersystemCommunications Eisc { get; private set; }


        public EiscApi(DeviceConfig dc) :
            base(dc.Key)
        {
            PropertiesConfig = JsonConvert.DeserializeObject<EiscApiPropertiesConfig>(dc.Properties.ToString());

            Eisc = new ThreeSeriesTcpIpEthernetIntersystemCommunications(PropertiesConfig.Control.IpIdInt, PropertiesConfig.Control.TcpSshProperties.Address, Global.ControlSystem);

            Eisc.SigChange += new Crestron.SimplSharpPro.DeviceSupport.SigEventHandler(Eisc_SigChange);

            Eisc.Register();

            AddPostActivationAction( () =>
            {
                Debug.Console(1, this, "Linking Devices...");

                foreach (var d in PropertiesConfig.Devices)
                {
                    var device = DeviceManager.GetDeviceForKey(d.DeviceKey);

                    if (device != null)
                    {
                        if (device is GenericComm)
                        {
                            (device as GenericComm).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                            continue;
                        }
						else if (device is DigitalLogger)
						{
							(device as DigitalLogger).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
							continue;
						}
						else if (device is DmChassisController)
						{
							// (device as DmChassisController).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
							continue;
						}
						else if (device is DmTxControllerBase)
						{
							// (device as DmTxControllerBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
							continue;
						}
						else if (device is DmRmcControllerBase)
						{
							// (device as DmRmcControllerBase).LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
							continue;
						}
                    }
                }

                Debug.Console(1, this, "Devices Linked.");
            });
        }

        /// <summary>
        /// Handles incoming sig changes
        /// </summary>
        /// <param name="currentDevice"></param>
        /// <param name="args"></param>
        void Eisc_SigChange(object currentDevice, Crestron.SimplSharpPro.SigEventArgs args)
        {
            if (Debug.Level >= 1)
                Debug.Console(1, this, "EiscApi change: {0} {1}={2}", args.Sig.Type, args.Sig.Number, args.Sig.StringValue);
            var uo = args.Sig.UserObject;
            if (uo is Action<bool>)
                (uo as Action<bool>)(args.Sig.BoolValue);
            else if (uo is Action<ushort>)
                (uo as Action<ushort>)(args.Sig.UShortValue);
            else if (uo is Action<string>)
                (uo as Action<string>)(args.Sig.StringValue);
        }
    }

    public class EiscApiPropertiesConfig
    {
        [JsonProperty("control")]
        public EssentialsControlPropertiesConfig Control { get; set; }

        [JsonProperty("devices")]
        public List<ApiDevice> Devices { get; set; }

        public class ApiDevice
        {
            [JsonProperty("deviceKey")]
            public string DeviceKey { get; set; }

            [JsonProperty("joinStart")]
            public uint JoinStart { get; set; }

            [JsonProperty("joinMapKey")]
            public string JoinMapKey { get; set; }
        }

    }


    ///// <summary>
    ///// API class for IBasicCommunication devices
    ///// </summary>
    //public class IBasicCommunicationApi : DeviceApiBase
    //{
    //    public IBasicCommunication Device { get; set; }

    //    SerialFeedback TextReceivedFeedback;

    //    public IBasicCommunicationApi(IBasicCommunication dev)
    //    {
    //        TextReceivedFeedback = new SerialFeedback();

    //        Device = dev;

    //        SetupFeedbacks();

    //        ActionApi = new Dictionary<string, Object>
    //        {
    //            { "connect", new Action(Device.Connect) },
    //            { "disconnect", new Action(Device.Disconnect) },
    //            { "connectstate", new Action<bool>( b => ConnectByState(b) ) },
    //            { "sendtext", new Action<string>( s => Device.SendText(s) ) }

    //        };

    //        FeedbackApi = new Dictionary<string, Feedback>
    //        {
    //            { "isconnected", new BoolFeedback( () => Device.IsConnected ) },
    //            { "textrecieved", TextReceivedFeedback }
    //        };
    //    }

    //    /// <summary>
    //    /// Controls connection based on state of input
    //    /// </summary>
    //    /// <param name="state"></param>
    //    void ConnectByState(bool state)
    //    {
    //        if (state)
    //            Device.Connect();
    //        else
    //            Device.Disconnect();                             
    //    }

    //    void SetupFeedbacks()
    //    {
    //        Device.TextReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(Device_TextReceived);

    //        if(Device is ISocketStatus)
    //            (Device as ISocketStatus).ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(IBasicCommunicationApi_ConnectionChange);
    //    }

    //    void IBasicCommunicationApi_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e)
    //    {
    //        FeedbackApi["isconnected"].FireUpdate();
    //    }

    //    void Device_TextReceived(object sender, GenericCommMethodReceiveTextArgs e)
    //    {
    //        TextReceivedFeedback.FireUpdate(e.Text);
    //    }
    //}

 

    ///// <summary>
    ///// Each flavor of API is a static class with static properties and a static constructor that
    ///// links up the things to do.
    ///// </summary>
    //public class DmChassisControllerApi : DeviceApiBase
    //{
    //    IntFeedback Output1Feedback;
    //    IntFeedback Output2Feedback;

    //    public DmChassisControllerApi(DmChassisController dev)
    //    {
    //        Output1Feedback = new IntFeedback( new Func<int>(() => 1));
    //        Output2Feedback = new IntFeedback( new Func<int>(() => 2));

    //        ActionApi = new Dictionary<string, Object>
    //        {
                
    //        };

    //        FeedbackApi = new Dictionary<string, Feedback>
    //        {
    //            { "Output-1/fb", Output1Feedback },
    //            { "Output-2/fb", Output2Feedback }
    //        };
    //    }

    //    /// <summary>
    //    /// Factory method
    //    /// </summary>
    //    /// <param name="dev"></param>
    //    /// <returns></returns>
    //    public static DmChassisControllerApi GetActionApiForDevice(DmChassisController dev)
    //    {
    //        return new DmChassisControllerApi(dev);
    //    }
    //}


}