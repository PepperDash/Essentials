using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Represents a connection between routing ports, linking a source output port to a destination input port.
    /// This class is used to define signal paths for routing algorithms, including signal type overrides and internal connections.
    /// </summary>
    public class TieLine
    {
        /// <summary>
        /// The source output port of the tie line.
        /// </summary>
        public RoutingOutputPort SourcePort { get; private set; }
        /// <summary>
        /// The destination input port of the tie line.
        /// </summary>
        public RoutingInputPort DestinationPort { get; private set; }
        //public int InUseCount { get { return DestinationUsingThis.Count; } }

        /// <summary>
        /// Gets the type of this tie line. Will either be the type of the destination port
        /// or the type of OverrideType when it is set.
        /// </summary>
        public eRoutingSignalType Type
        {
            get
            {
                if (OverrideType.HasValue) return OverrideType.Value;
                return DestinationPort.Type;
            }
        }

        /// <summary>
        /// Use this to override the Type property for the destination port. For example,
        /// when the tie line is type AudioVideo, and the signal flow should be limited to
        /// Audio-only or Video only, changing this type will alter the signal paths
        /// available to the routing algorithm without affecting the actual Type
        /// of the destination port.
        /// </summary>
        public eRoutingSignalType? OverrideType { get; set; }

        //List<IRoutingInputs> DestinationUsingThis = new List<IRoutingInputs>();

        /// <summary>
        /// Gets a value indicating whether this tie line represents an internal connection within a device (both source and destination ports are internal).
        /// </summary>
        public bool IsInternal { get { return SourcePort.IsInternal && DestinationPort.IsInternal; } }
        /// <summary>
        /// Gets a value indicating whether the signal types of the source and destination ports differ.
        /// </summary>
        public bool TypeMismatch { get { return SourcePort.Type != DestinationPort.Type; } }
        /// <summary>
        /// Gets a value indicating whether the connection types of the source and destination ports differ.
        /// </summary>
        public bool ConnectionTypeMismatch { get { return SourcePort.ConnectionType != DestinationPort.ConnectionType; } }
        /// <summary>
        /// A descriptive note about any type mismatch, if applicable.
        /// </summary>
        public string TypeMismatchNote { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TieLine"/> class.
        /// </summary>
        /// <param name="sourcePort">The source output port.</param>
        /// <param name="destinationPort">The destination input port.</param>
        public TieLine(RoutingOutputPort sourcePort, RoutingInputPort destinationPort)
        {
            if (sourcePort == null || destinationPort == null)
                throw new ArgumentNullException("source or destination port");
            SourcePort = sourcePort;
            DestinationPort = destinationPort;
        }

        /// <summary>
        /// Creates a tie line with an overriding Type. See help for OverrideType property for info.
        /// </summary>
        /// <param name="sourcePort">The source output port.</param>
        /// <param name="destinationPort">The destination input port.</param>
        /// <param name="overrideType">The signal type to limit the link to. Overrides DestinationPort.Type for routing calculations.</param>
        public TieLine(RoutingOutputPort sourcePort, RoutingInputPort destinationPort, eRoutingSignalType? overrideType) :
            this(sourcePort, destinationPort)
        {
            OverrideType = overrideType;
        }

        /// <summary>
        /// Creates a tie line with an overriding Type. See help for OverrideType property for info.
        /// </summary>
        /// <param name="sourcePort">The source output port.</param>
        /// <param name="destinationPort">The destination input port.</param>
        /// <param name="overrideType">The signal type to limit the link to. Overrides DestinationPort.Type for routing calculations.</param>
        public TieLine(RoutingOutputPort sourcePort, RoutingInputPort destinationPort, eRoutingSignalType overrideType) :
            this(sourcePort, destinationPort)
        {
            OverrideType = overrideType;
        }

        /// <summary>
        /// Will link up video status from supporting inputs to connected outputs.
        /// </summary>
        public void Activate()
        {
            // Now does nothing
        }

        /// <summary>
        /// Deactivates the tie line.
        /// </summary>
        public void Deactivate()
        {
            // Now does nothing
        }

        /// <summary>
        /// Returns a string representation of the tie line.
        /// </summary>
        /// <returns>A string describing the source, destination, and type of the tie line.</returns>
        public override string ToString()
        {
            return string.Format("Tie line: {0}:{1} --> {2}:{3} {4}", SourcePort.ParentDevice.Key, SourcePort.Key,
                DestinationPort.ParentDevice.Key, DestinationPort.Key, Type.ToString());
        }
    }

    //********************************************************************************

    /// <summary>
    /// Represents a collection of <see cref="TieLine"/> objects, which define signal paths for routing algorithms.
    /// This class provides functionality for managing tie lines and includes a singleton instance for global access.
    /// </summary>
    public class TieLineCollection : List<TieLine>
    {
        /// <summary>
        /// Gets the default singleton instance of the <see cref="TieLineCollection"/>.
        /// </summary>
        public static TieLineCollection Default
        {
            get
            {
                if (_Default == null)
                    _Default = new TieLineCollection();
                return _Default;
            }
        }

        /// <summary>
        /// Backing field for the singleton instance.
        /// </summary>
        [JsonIgnore]
        private static TieLineCollection _Default;
    }
}