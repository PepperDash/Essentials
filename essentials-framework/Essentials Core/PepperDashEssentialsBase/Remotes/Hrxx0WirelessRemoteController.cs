using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Gateways;
using Crestron.SimplSharpPro.Remotes;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharp.Reflection;
using Newtonsoft.Json;


using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core
{
    [Description("Wrapper class for all HR-Series remotes")]
    public class Hrxx0WirelessRemoteController : EssentialsBridgeableDevice, IHasFeedback
    {
        private Hr1x0WirelessRemoteBase _remote;

        public FeedbackCollection<Feedback> Feedbacks { get; set; }

        public CrestronCollection<Button> Buttons { get { return _remote.Button; } }

        public Hrxx0WirelessRemoteController(string key, Func<DeviceConfig, Hr1x0WirelessRemoteBase> preActivationFunc, 
            DeviceConfig config)
            : base(key, config.Name)
        {
            Feedbacks = new FeedbackCollection<Feedback>();

            AddPreActivationAction(() =>
            {
                _remote = preActivationFunc(config);

                _remote.ButtonStateChange += new ButtonEventHandler(_remote_ButtonStateChange);

                Feedbacks.Add(new BoolFeedback("BatteryCritical", () => _remote.BatteryCriticalFeedback.BoolValue));
                Feedbacks.Add(new BoolFeedback("BatteryLow", () => _remote.BatteryLowFeedback.BoolValue));
                Feedbacks.Add(new IntFeedback("BatteryVoltage", () => _remote.BatteryVoltageFeedback.UShortValue));

                _remote.BaseEvent += new BaseEventHandler(_remote_BaseEvent);
            });
        }

        void _remote_BaseEvent(GenericBase device, BaseEventArgs args)
        {
            if(args.EventId == Hr1x0EventIds.BatteryCriticalFeedbackEventId)
                Feedbacks["BatteryCritical"].FireUpdate();
            if(args.EventId == Hr1x0EventIds.BatteryLowFeedbackEventId)
                Feedbacks["BatteryLow"].FireUpdate();
            if(args.EventId == Hr1x0EventIds.BatteryVoltageFeedbackEventId)
                Feedbacks["BatteryVoltage"].FireUpdate();
        }

        void _remote_ButtonStateChange(GenericBase device, ButtonEventArgs args)
        {
            try
            {
                var handler = args.Button.UserObject;

                if (handler == null) return;

                Debug.Console(1, this, "Executing Action: {0}", handler.ToString());

                if (handler is Action<bool>)
                {
                    (handler as Action<bool>)(args.Button.State == eButtonState.Pressed ? true : false);
                }
            }
            catch (Exception e)
            {
                Debug.Console(2, this, "Error in ButtonStateChange handler: {0}", e);
            }
        }


        #region Preactivation

        private static Hr1x0WirelessRemoteBase GetHr1x0WirelessRemote(DeviceConfig config)
        {
            var props = JsonConvert.DeserializeObject<CrestronRemotePropertiesConfig>(config.Properties.ToString());

            var type = config.Type;
            var rfId = (uint)props.Control.InfinetIdInt;

            GatewayBase gateway;

            if (props.GatewayDeviceKey == "processor")
            {
                gateway = Global.ControlSystem.ControllerRFGatewayDevice;
            }
            else
            {
                var gatewayDev = DeviceManager.GetDeviceForKey(props.GatewayDeviceKey) as CenRfgwController;
                if (gatewayDev == null)
                {
                    Debug.Console(0, "GetHr1x0WirelessRemote: Device '{0}' is not a valid device", props.GatewayDeviceKey);
                    return null;
                }
                Debug.Console(0, "GetHr1x0WirelessRemote: Device '{0}' is a valid device", props.GatewayDeviceKey);
                gateway = gatewayDev.GateWay; 
            }

            if (gateway == null)
            {
                Debug.Console(0, "GetHr1x0WirelessRemote: Device '{0}' is not a valid gateway", props.GatewayDeviceKey);
                return null;
            }

            switch (type)
            {
                case ("hr100"):
                    return new Hr100(rfId, gateway);
                case ("hr150"):
                    return new Hr150(rfId, gateway);
                case ("hr310"):
                    return new Hr310(rfId, gateway);
                default:
                    return null;
            }
        }

        static void gateway_BaseEvent(GenericBase device, BaseEventArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Factory
        public class Hrxx0WirelessRemoteControllerFactory : EssentialsDeviceFactory<Hrxx0WirelessRemoteController>
        {
            public Hrxx0WirelessRemoteControllerFactory()
            {
                TypeNames = new List<string>() { "hr100", "hr150", "hr310" };
            }

            public override EssentialsDevice BuildDevice(DeviceConfig dc)
            {
                Debug.Console(1, "Factory Attempting to create new HR-x00 Remote Device");

                return new Hrxx0WirelessRemoteController(dc.Key, GetHr1x0WirelessRemote, dc);
            }
        }
        #endregion

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new Hrxxx0WirelessRemoteControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<Hrxxx0WirelessRemoteControllerJoinMap>(joinMapSerialized);

            bridge.AddJoinMap(Key, joinMap);

            //List<string> ExcludedKeys = new List<string>();
            foreach (var feedback in Feedbacks)
            {
                var myFeedback = feedback;

                var joinData =
                    joinMap.Joins.FirstOrDefault(
                        x => 
                            x.Key.Equals(myFeedback.Key, StringComparison.InvariantCultureIgnoreCase));

                if (string.IsNullOrEmpty((joinData.Key))) continue;

                var name = joinData.Key;
                var join = joinData.Value;

                if (join.Metadata.JoinType == eJoinType.Digital)
                {
                    Debug.Console(0, this, "Linking Bool Feedback '{0}' to join {1}", name, join.JoinNumber);
                    var someFeedback = myFeedback as BoolFeedback;
                    if(someFeedback == null) continue;
                    someFeedback.LinkInputSig(trilist.BooleanInput[join.JoinNumber]);
                }
                if (join.Metadata.JoinType == eJoinType.Analog)
                {
                    Debug.Console(0, this, "Linking Analog Feedback '{0}' to join {1}", name, join.JoinNumber);
                    var someFeedback = myFeedback as IntFeedback;
                    if (someFeedback == null) continue;
                    someFeedback.LinkInputSig(trilist.UShortInput[join.JoinNumber]);
                }
                if (join.Metadata.JoinType == eJoinType.Serial)
                {
                    Debug.Console(0, this, "Linking Serial Feedback '{0}' to join {1}", name, join.JoinNumber);
                    var someFeedback = myFeedback as StringFeedback;
                    if (someFeedback == null) continue;
                    someFeedback.LinkInputSig(trilist.StringInput[join.JoinNumber]);
                }
            }

            //var newJoinKeys = joinMap.Joins.Keys.Except(ExcludedKeys).ToList();

            //var newJoinMap = newJoinKeys.Where(k => joinMap.Joins.ContainsKey(k)).Select(k => joinMap.Joins[k]);


            Debug.Console(2, this, "There are {0} remote buttons", _remote.Button.Count);
            for (uint i = 1; i <= _remote.Button.Count; i++)
            {
                Debug.Console(2, this, "Attempting to link join index {0}", i);
                var index = i;
                var joinData =
                    joinMap.Joins.FirstOrDefault(
                        o =>
                            o.Key.Equals(_remote.Button[index].Name.ToString(),
                                StringComparison.InvariantCultureIgnoreCase));

                if (string.IsNullOrEmpty((joinData.Key))) continue;

                var join = joinData.Value;
                var name = joinData.Key;

                Debug.Console(2, this, "Setting User Object for '{0}'", name);
                if (join.Metadata.JoinType == eJoinType.Digital)
                {
                    _remote.Button[i].SetButtonAction((b) => trilist.BooleanInput[join.JoinNumber].BoolValue = b);
                }
            }

            trilist.OnlineStatusChange += (d, args) =>
            {
                if (!args.DeviceOnLine) return;

                foreach (var feedback in Feedbacks)
                {
                    feedback.FireUpdate();
                }
                
            };
        }
        public void SetTrilistBool(BasicTriList trilist, uint join, bool b)
        {
            trilist.BooleanInput[join].BoolValue = b;
        }
    }
}