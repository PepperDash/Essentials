using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;
using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials.Devices.Common.SoftCodec
{
    public class GenericSoftCodec : EssentialsDevice, IRoutingSource, IRoutingOutputs, IRoutingSinkWithSwitching
    {
        private RoutingInputPort _currentInputPort;
        public RoutingInputPort CurrentInputPort {
            get => _currentInputPort;
            set
            {
                _currentInputPort = value;

                InputChanged?.Invoke(this, _currentInputPort);
            }
        }

        public GenericSoftCodec(string key, string name, GenericSoftCodecProperties props) : base(key, name)
        {
            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

            for(var i = 1; i <= props.OutputCount; i++)
            {
                var outputPort = new RoutingOutputPort($"{Key}-output{i}", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, null, this);

                OutputPorts.Add(outputPort);
            }

            for(var i = 1; i<= props.ContentInputCount; i++)
            {
                var inputPort = new RoutingInputPort($"{Key}-contentInput{i}", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, $"contentInput{i}", this);

                InputPorts.Add(inputPort);
            }

            if (!props.HasCameraInputs)
            {
                return;
            }

            for(var i = 1; i <=props.CameraInputCount; i++)
            {
                var cameraPort = new RoutingInputPort($"{Key}-cameraInput{i}", eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, $"cameraInput{i}", this);

                InputPorts.Add(cameraPort);
            }
        }

        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }
        public string CurrentSourceInfoKey { get ; set; }
        public SourceListItem CurrentSourceInfo
        {
            get
            {
                return _CurrentSourceInfo;
            }
            set
            {
                if (value == _CurrentSourceInfo) return;

                var handler = CurrentSourceChange;

                if (handler != null)
                    handler(_CurrentSourceInfo, ChangeType.WillChange);

                _CurrentSourceInfo = value;

                if (handler != null)
                    handler(_CurrentSourceInfo, ChangeType.DidChange);
            }
        }

        SourceListItem _CurrentSourceInfo;

        public event SourceInfoChangeHandler CurrentSourceChange;
        public event InputChangedEventHandler InputChanged;

        public void ExecuteSwitch(object inputSelector)
        {
            var inputPort = InputPorts.FirstOrDefault(p => p.Selector == inputSelector);

            if(inputPort == null)
            {
                Debug.LogMessage(LogEventLevel.Warning, "No input port found for selector {inputSelector}", inputSelector);
                return;
            }

            CurrentInputPort = inputPort;
        }
    }

    public class GenericSoftCodecProperties
    {
        [JsonProperty("hasCameraInputs")]
        public bool HasCameraInputs { get; set; }

        [JsonProperty("cameraInputCount")]
        public int CameraInputCount { get; set; }

        [JsonProperty("contentInputCount")]
        public int ContentInputCount { get; set; }

        [JsonProperty("contentOutputCount")]
        public int OutputCount { get; set; }
    }

    public class GenericSoftCodecFactory: EssentialsDeviceFactory<GenericSoftCodec>
    {
        public GenericSoftCodecFactory()
        {
            TypeNames = new List<string> { "genericsoftcodec" };
        }

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Attempting to create new Generic SoftCodec Device");

            var props = dc.Properties.ToObject<GenericSoftCodecProperties>();

            return new GenericSoftCodec(dc.Key, dc.Name, props);
        }
    }
}
