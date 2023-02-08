extern alias Full;

using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core;
using PepperDash.Core;
using Full.Newtonsoft.Json;

namespace PepperDash.Essentials.DM
{
    public class HDBaseTRxController : DmHdBaseTControllerBase, IRoutingInputsOutputs,
        IComPorts
    {
        public RoutingInputPort DmIn { get; private set; }
        public RoutingOutputPort HDBaseTSink { get; private set; }

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

        public HDBaseTRxController(string key, string name, HDRx3CB rmc)
            : base(key, name, rmc)
        {
            DmIn = new RoutingInputPort(DmPortName.DmIn, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.DmCat, 0, this);
            HDBaseTSink = new RoutingOutputPort(DmPortName.HdmiOut, eRoutingSignalType.AudioVideo,
                eRoutingPortConnectionType.Hdmi, null, this) {Port = Rmc};

            InputPorts = new RoutingPortCollection<RoutingInputPort> {DmIn};
            OutputPorts = new RoutingPortCollection<RoutingOutputPort> {HDBaseTSink};
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DmRmcControllerJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DmRmcControllerJoinMap>(joinMapSerialized);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }
            else
            {
                Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
            }

            Debug.Console(1, this, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
            trilist.StringInput[joinMap.Name.JoinNumber].StringValue = this.Name;
        }

        #region IComPorts Members
        public CrestronCollection<ComPort> ComPorts { get { return Rmc.ComPorts; } }
        public int NumberOfComPorts { get { return Rmc.NumberOfComPorts; } }
        #endregion
    }
}