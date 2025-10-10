﻿using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Generic
{
    /// <summary>
    /// Represents a GenericSink
    /// </summary>
    public class GenericSink : EssentialsDevice, IRoutingSinkWithInputPort
    {
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
        }

        /// <summary>
        /// Gets or sets the InputPorts
        /// </summary>
        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

        /// <summary>
        /// Gets or sets the CurrentSourceInfoKey
        /// </summary>
        public string CurrentSourceInfoKey { get; set; }

        private SourceListItem _currentSource;
        /// <summary>
        /// Gets or sets the CurrentSourceInfo
        /// </summary>
        public SourceListItem CurrentSourceInfo
        {
            get => _currentSource;
            set
            {
                if (value == _currentSource)
                {
                    return;
                }

                CurrentSourceChange?.Invoke(_currentSource, ChangeType.WillChange);

                _currentSource = value;

                CurrentSourceChange?.Invoke(_currentSource, ChangeType.DidChange);
            }
        }

        /// <summary>
        /// Gets the current input port
        /// </summary>
        public RoutingInputPort CurrentInputPort => InputPorts[0];

        /// <summary>
        /// Event fired when the current source changes
        /// </summary>
        public event SourceInfoChangeHandler CurrentSourceChange;
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
}
