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
    public class GenericDigitalInputDevice : EssentialsBridgeableDevice, IDigitalInput
    {
        public DigitalInput InputPort { get; private set; }

        public BoolFeedback InputStateFeedback { get; private set; }

        Func<bool> InputStateFeedbackFunc
        {
            get
            {
                return () => InputPort.State;
            }
        }

        public GenericDigitalInputDevice(string key, DigitalInput inputPort):
            base(key)
        {
            InputStateFeedback = new BoolFeedback(InputStateFeedbackFunc);

            InputPort = inputPort;

            InputPort.StateChange += InputPort_StateChange;

        }

        void InputPort_StateChange(DigitalInput digitalInput, DigitalInputEventArgs args)
        {
            InputStateFeedback.FireUpdate();
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new IDigitalInputJoinMap();

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<IDigitalInputJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            try
            {
                Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

                // Link feedback for input state
                InputStateFeedback.LinkInputSig(trilist.BooleanInput[joinMap.InputState]);
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Unable to link device '{0}'.  Input is null", Key);
                Debug.Console(1, this, "Error: {0}", e);
            }
        }
    }
}