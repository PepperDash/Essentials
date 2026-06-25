using System;
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Routing;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Generic;

/// <summary>
/// Represents a GenericSink
/// </summary>
public class GenericSink : EssentialsDevice, IRoutingSinkWithFeedback
{
	/// <inheritdoc/> 
	public Dictionary<eRoutingSignalType, IRoutingSource> CurrentSources { get; private set; }

	/// <inheritdoc/>
	public Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; private set; }

	/// <inheritdoc />
	public event EventHandler<CurrentSourcesChangedEventArgs> CurrentSourcesChanged;

    /// <summary>
    /// Initializes a new instance of the GenericSink class
    /// </summary>
    /// <param name="key">The device key</param>
    /// <param name="name">The device name</param>
    public GenericSink(string key, string name) : base(key, name)
    {
        InputPorts = new RoutingPortCollection<RoutingInputPort>();

        var inputPort = new RoutingInputPort(RoutingPortNames.AnyVideoIn, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, null, this);

        InputPorts.Add(inputPort);

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
    }

	/// <inheritdoc />
	public virtual void SetCurrentSource(eRoutingSignalType signalType, IRoutingSource sourceDevice)
	{
		foreach (eRoutingSignalType type in Enum.GetValues(typeof(eRoutingSignalType)))
		{
			var flagValue = Convert.ToInt32(type);
			// Skip if flagValue is 0 or not a power of two (i.e., not a single-bit flag).
			// (flagValue & (flagValue - 1)) != 0 checks if more than one bit is set.
			if (flagValue == 0 || (flagValue & (flagValue - 1)) != 0)
			{
				this.LogDebug("Skipping {type}", type);
				continue;
			}

			if (!signalType.HasFlag(type))
			{
				this.LogDebug("Skipping {type}", type);
				continue;
			}

			this.LogDebug("setting {type}", type);

			CurrentSources.TryGetValue(type, out var previousSource);

			UpdateCurrentSources(type, previousSource, sourceDevice);
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
			CurrentSourceKeys[signalType] = sourceDevice?.Key;
		}
		else
		{
			CurrentSourceKeys.Add(signalType, sourceDevice?.Key);
		}

		// Raise the CurrentSourcesChanged event
		CurrentSourcesChanged?.Invoke(this, new CurrentSourcesChangedEventArgs(signalType, previousSource, sourceDevice));
	}

    /// <summary>
    /// Gets or sets the InputPorts
    /// </summary>
    public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

    private SourceListItem _currentSource;

    /// <summary>
    /// Gets the current input port
    /// </summary>
    public RoutingInputPort CurrentInputPort => InputPorts[0];

    /// <inheritdoc />
    public event InputChangedEventHandler InputChanged;

    /// <inheritdoc />
    public void ExecuteSwitch(object inputSelector)
    {
        this.LogDebug("GenericSink Executing Switch to: {inputSelector}", inputSelector);
    }
}

/// <summary>
/// Represents a GenericSinkFactory
/// </summary>
public class GenericSinkFactory : EssentialsDeviceFactory<GenericSink>
{
    /// <summary>
    /// Initializes a new instance of the GenericSinkFactory class
    /// </summary>
    public GenericSinkFactory()
    {
        TypeNames = new List<string>() { "genericsink", "genericdestination" };
    }

    /// <summary>
    /// BuildDevice method
    /// </summary>
    /// <inheritdoc />
    public override EssentialsDevice BuildDevice(DeviceConfig dc)
    {
        Debug.LogMessage(LogEventLevel.Debug, "Factory Attempting to create new Generic Sink Device");
        return new GenericSink(dc.Key, dc.Name);
    }
}



