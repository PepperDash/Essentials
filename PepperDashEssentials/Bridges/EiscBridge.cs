using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.EthernetCommunication;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Bridges
{
    [Obsolete("Please use 'eiscapiadvanced' in configurations going forward")]
    public class EiscApi : BridgeApi
    {
        public EiscApiPropertiesConfig PropertiesConfig { get; private set; }

        public ThreeSeriesTcpIpEthernetIntersystemCommunications Eisc { get; private set; }

        public EiscApi(DeviceConfig dc) :
            base(dc.Key)
        {
            PropertiesConfig = dc.Properties.ToObject<EiscApiPropertiesConfig>();
            //PropertiesConfig = JsonConvert.DeserializeObject<EiscApiPropertiesConfig>(dc.Properties.ToString());

            Eisc = new ThreeSeriesTcpIpEthernetIntersystemCommunications(PropertiesConfig.Control.IpIdInt, PropertiesConfig.Control.TcpSshProperties.Address, Global.ControlSystem);

            Eisc.SigChange += Eisc_SigChange;

            Eisc.Register();

            AddPostActivationAction(() =>
            {
                Debug.Console(1, this, "Linking Devices...");

                foreach (var d in PropertiesConfig.Devices)
                {
                    var device = DeviceManager.GetDeviceForKey(d.DeviceKey);

                    if (device == null) continue;

                    Debug.Console(1, this, "Linking Device: '{0}'", device.Key);
                    if (device is IBridge)      // Check for this first to allow bridges in plugins to override existing bridges that apply to the same type.
                    {
                        Debug.Console(2, this, "'{0}' is IBridge", device.Key);

                        var dev = device as IBridge;

                        dev.LinkToApi(Eisc, d.JoinStart, d.JoinMapKey);
                    }
                    if (!(device is IBridgeAdvanced)) continue;
                    Debug.Console(2, this, "'{0}' is IBridgeAdvanced", device.Key);

                    var advDev = device as IBridgeAdvanced;

                    try
                    {
                        advDev.LinkToApi(Eisc, d.JoinStart, d.JoinMapKey, null);
                    }
                    catch (NullReferenceException)
                    {
                        Debug.ConsoleWithLog(0, this,
                            "Please update the bridge config to use EiscBridgeAdvanced with this device: {0}", device.Key);
                    }

                }
                Debug.Console(1, this, "Devices Linked.");

            });
        }

        /// <summary>
        /// Used for debugging to trigger an action based on a join number and type
        /// </summary>
        /// <param name="join"></param>
        /// <param name="type"></param>
        /// <param name="state"></param>
        public void ExecuteJoinAction(uint join, string type, object state)
        {
            try
            {
                switch (type.ToLower())
                {
                    case "digital":
                        {
                            var uo = Eisc.BooleanOutput[join].UserObject as Action<bool>;
                            if (uo != null)
                            {
                                Debug.Console(1, this, "Executing Action: {0}", uo.ToString());
                                uo(Convert.ToBoolean(state));
                            }
                            else
                                Debug.Console(1, this, "User Action is null.  Nothing to Execute");
                            break;
                        }
                    case "analog":
                        {
                            var uo = Eisc.BooleanOutput[join].UserObject as Action<ushort>;
                            if (uo != null)
                            {
                                Debug.Console(1, this, "Executing Action: {0}", uo.ToString());
                                uo(Convert.ToUInt16(state));
                            }
                            else
                                Debug.Console(1, this, "User Action is null.  Nothing to Execute"); break;
                        }
                    case "serial":
                        {
                            var uo = Eisc.BooleanOutput[join].UserObject as Action<string>;
                            if (uo != null)
                            {
                                Debug.Console(1, this, "Executing Action: {0}", uo.ToString());
                                uo(Convert.ToString(state));
                            }
                            else
                                Debug.Console(1, this, "User Action is null.  Nothing to Execute");
                            break;
                        }
                    default:
                        {
                            Debug.Console(1, "Unknown join type.  Use digital/serial/analog");
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error: {0}", e);
            }

        }

        /// <summary>
        /// Handles incoming sig changes
        /// </summary>
        /// <param name="currentDevice"></param>
        /// <param name="args"></param>
        void Eisc_SigChange(object currentDevice, SigEventArgs args)
        {
            try
            {
                if (Debug.Level >= 1)
                    Debug.Console(1, this, "EiscApi change: {0} {1}={2}", args.Sig.Type, args.Sig.Number, args.Sig.StringValue);
                var uo = args.Sig.UserObject;

                if (uo == null) return;

                Debug.Console(1, this, "Executing Action: {0}", uo.ToString());
                if (uo is Action<bool>)
                    (uo as Action<bool>)(args.Sig.BoolValue);
                else if (uo is Action<ushort>)
                    (uo as Action<ushort>)(args.Sig.UShortValue);
                else if (uo is Action<string>)
                    (uo as Action<string>)(args.Sig.StringValue);
            }
            catch (Exception e)
            {
                Debug.Console(2, this, "Error in Eisc_SigChange handler: {0}", e);
            }
        }
    }

    public class EiscApiFactory : EssentialsDeviceFactory<EiscApiAdvanced>
    {
        public EiscApiFactory()
        {
            TypeNames = new List<string>() { "eiscapi" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "Factory Attempting to create new EiscApi Device");

            return new EiscApi(dc);
        }
    }

}