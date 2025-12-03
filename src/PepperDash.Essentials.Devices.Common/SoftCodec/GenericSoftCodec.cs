using System.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using Serilog.Events;

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
        public RoutingInputPort CurrentInputPort
        {
            get => _currentInputPort;
            set
            {
                _currentInputPort = value;

                InputChanged?.Invoke(this, _currentInputPort);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSoftCodec"/> class
        /// </summary>
        /// <param name="key">The device key</param>
        /// <param name="name">The device name</param>
        /// <param name="props">The device properties</param>
        public GenericSoftCodec(string key, string name, GenericSoftCodecProperties props) : base(key, name)
        {
            InputPorts = new RoutingPortCollection<RoutingInputPort>();
            OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

            for (var i = 1; i <= props.OutputCount; i++)
            {
                var outputPort = new RoutingOutputPort($"output{i}", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, null, this);

                OutputPorts.Add(outputPort);
            }

            for (var i = 1; i <= props.ContentInputCount; i++)
            {
                var inputPort = new RoutingInputPort($"contentInput{i}", eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, $"contentInput{i}", this);

                InputPorts.Add(inputPort);
            }

            if (!props.HasCameraInputs)
            {
                return;
            }

            for (var i = 1; i <= props.CameraInputCount; i++)
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
        public string CurrentSourceInfoKey { get; set; }

        /// <summary>
        /// Gets or sets the CurrentSourceInfo
        /// </summary>
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

        /// <summary>
        /// Event fired when the current source changes
        /// </summary>
        public event SourceInfoChangeHandler CurrentSourceChange;

        /// <summary>
        /// Event fired when the input changes
        /// </summary>
        public event InputChangedEventHandler InputChanged;

        /// <summary>
        /// ExecuteSwitch method
        /// </summary>
        public void ExecuteSwitch(object inputSelector)
        {
            var inputPort = InputPorts.FirstOrDefault(p => p.Selector == inputSelector);

            if (inputPort == null)
            {
                Debug.LogMessage(LogEventLevel.Warning, "No input port found for selector {inputSelector}", inputSelector);
                return;
            }

            CurrentInputPort = inputPort;
        }
    }
}
