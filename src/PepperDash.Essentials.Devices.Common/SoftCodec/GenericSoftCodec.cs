using System.Collections.Generic;
using System.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Routing;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.SoftCodec;


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

    /// <inheritdoc/> 
    public Dictionary<eRoutingSignalType, IRoutingSource> CurrentSources { get; private set; }

    /// <inheritdoc/>
    public Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; private set; }

    /// <inheritdoc />
    public event System.EventHandler<CurrentSourcesChangedEventArgs> CurrentSourcesChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSoftCodec"/> class
    /// </summary>
    /// <param name="key">The device key</param>
    /// <param name="name">The device name</param>
    /// <param name="props">The device properties</param>
    public GenericSoftCodec(string key, string name, GenericSoftCodecProperties props) : base(key, name)
    {
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

        CurrentSources = new Dictionary<eRoutingSignalType, IRoutingSource>
            {
                { eRoutingSignalType.Audio, null },
                { eRoutingSignalType.Video, null },
            };

        CurrentSourceKeys = new Dictionary<eRoutingSignalType, string>
            {
                { eRoutingSignalType.Audio, string.Empty },
                { eRoutingSignalType.Video, string.Empty },
            };

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

    	/// <inheritdoc />
	public virtual void SetCurrentSource(eRoutingSignalType signalType, IRoutingSource sourceDevice)
	{
		foreach (eRoutingSignalType type in System.Enum.GetValues(typeof(eRoutingSignalType)))
		{
			var flagValue = System.Convert.ToInt32(type);
			// Skip if flagValue is 0 or not a power of two (i.e., not a single-bit flag).
			// (flagValue & (flagValue - 1)) != 0 checks if more than one bit is set.
			if (flagValue == 0 || (flagValue & (flagValue - 1)) != 0)
			{
				this.LogDebug("Skipping {type}", type);
				continue;
			}

			this.LogDebug("setting {type}", type);

			var previousSource = CurrentSources[type];

			if (signalType.HasFlag(type))
			{
				UpdateCurrentSources(type, previousSource, sourceDevice);
			}
		}
	}

	private void UpdateCurrentSources(eRoutingSignalType signalType, IRoutingSource previousSource, IRoutingSource sourceDevice)
	{
		if (CurrentSources.ContainsKey(signalType))
		{
			CurrentSources[signalType] = sourceDevice;
		}
		else
		{
			CurrentSources.Add(signalType, sourceDevice);
		}

		// Update the current source key for the specified signal type
		if (CurrentSourceKeys.ContainsKey(signalType))
		{
			CurrentSourceKeys[signalType] = sourceDevice.Key;
		}
		else
		{
			CurrentSourceKeys.Add(signalType, sourceDevice.Key);
		}

		// Raise the CurrentSourcesChanged event
		CurrentSourcesChanged?.Invoke(this, new CurrentSourcesChangedEventArgs(signalType, previousSource, sourceDevice));
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

