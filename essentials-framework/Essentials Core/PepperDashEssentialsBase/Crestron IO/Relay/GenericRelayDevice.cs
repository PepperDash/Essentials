using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core.CrestronIO
{
    /// <summary>
    /// Represents a generic device controlled by relays
    /// </summary>
    public class GenericRelayDevice : EssentialsBridgeableDevice, ISwitchedOutput
    {
        public Relay RelayOutput { get; private set; }

        public BoolFeedback OutputIsOnFeedback { get; private set; }

        public GenericRelayDevice(string key, Relay relay):
            base(key)
        {
            OutputIsOnFeedback = new BoolFeedback(new Func<bool>(() => RelayOutput.State));

            RelayOutput = relay;
            RelayOutput.Register();

            RelayOutput.StateChange += new RelayEventHandler(RelayOutput_StateChange);
        }

        void RelayOutput_StateChange(Relay relay, RelayEventArgs args)
        {
            OutputIsOnFeedback.FireUpdate();
        }

        public void OpenRelay()
        {
            RelayOutput.State = false;
        }

        public void CloseRelay()
        {
            RelayOutput.State = true;
        }

        public void ToggleRelayState()
        {
            if (RelayOutput.State == true)
                OpenRelay();
            else
                CloseRelay();
        }

        #region ISwitchedOutput Members

        void ISwitchedOutput.On()
        {
            CloseRelay();
        }

        void ISwitchedOutput.Off()
        {
            OpenRelay();
        }

        #endregion

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApi bridge)
        {
            var joinMap = new GenericRelayControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<GenericRelayControllerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            if (RelayOutput == null)
            {
                Debug.Console(1, this, "Unable to link device '{0}'.  Relay is null", Key);
                return;
            }

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            trilist.SetBoolSigAction(joinMap.Relay, b =>
            {
                if (b)
                    CloseRelay();
                else
                    OpenRelay();
            });

            // feedback for relay state

            OutputIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Relay]);
        }
    }
}