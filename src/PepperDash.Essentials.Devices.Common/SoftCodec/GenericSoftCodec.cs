using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;
using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials.Devices.Common.SoftCodec
{
    /// <summary>
    /// Represents a GenericSoftCodec
    /// </summary>
    public class GenericSoftCodec : EssentialsDevice, IRoutingSource, IRoutingSinkWithSwitchingWithInputPort
    {
        private RoutingInputPort _currentInputPort;

        /// <summary>
        /// Gets or sets the CurrentInputPort
        /// </summary>
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
                var outputPort = new RoutingOutputPort($"output{i}", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, null, this);

                OutputPorts.Add(outputPort);
            }

            for(var i = 1; i<= props.ContentInputCount; i++)
            {
                var inputPort = new RoutingInputPort($"contentInput{i}", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, $"contentInput{i}", this);

                InputPorts.Add(inputPort);
            }

            if (!props.HasCameraInputs)
            {
                return;
            }

            for(var i = 1; i <=props.CameraInputCount; i++)
            {
                var cameraPort = new RoutingInputPort($"cameraInput{i}", eRoutingSignalType.Video, eRoutingPortConnectionType.Hdmi, $"cameraInput{i}", this);

                InputPorts.Add(cameraPort);
            }
        }

        /// <summary>
        /// Gets or sets the InputPorts
        /// </summary>
        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        /// <summary>
        /// Gets or sets the OutputPorts
        /// </summary>
        public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }
        /// <summary>
        /// Gets or sets the CurrentSourceInfoKey
        /// </summary>
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

        /// <summary>
        /// ExecuteSwitch method
        /// </summary>
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

    /// <summary>
    /// Represents a GenericSoftCodecProperties
    /// </summary>
    public class GenericSoftCodecProperties
    {
        [JsonProperty("hasCameraInputs")]
        /// <summary>
        /// Gets or sets the HasCameraInputs
        /// </summary>
        public bool HasCameraInputs { get; set; }

        [JsonProperty("cameraInputCount")]
        /// <summary>
        /// Gets or sets the CameraInputCount
        /// </summary>
        public int CameraInputCount { get; set; }

        [JsonProperty("contentInputCount")]
        /// <summary>
        /// Gets or sets the ContentInputCount
        /// </summary>
        public int ContentInputCount { get; set; }

        [JsonProperty("contentOutputCount")]
        /// <summary>
        /// Gets or sets the OutputCount
        /// </summary>
        public int OutputCount { get; set; }
    }

    /// <summary>
    /// Represents a GenericSoftCodecFactory
    /// </summary>
    public class GenericSoftCodecFactory: EssentialsDeviceFactory<GenericSoftCodec>
    {
        public GenericSoftCodecFactory()
        {
            TypeNames = new List<string> { "genericsoftcodec" };
        }

        /// <summary>
        /// BuildDevice method
        /// </summary>
        /// <inheritdoc />
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(LogEventLevel.Debug, "Attempting to create new Generic SoftCodec Device");

            var props = dc.Properties.ToObject<GenericSoftCodecProperties>();

            return new GenericSoftCodec(dc.Key, dc.Name, props);
        }
    }
}
