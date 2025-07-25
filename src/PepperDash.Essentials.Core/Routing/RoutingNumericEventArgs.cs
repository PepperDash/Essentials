using System;


namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a RoutingNumericEventArgs
    /// </summary>
    public class RoutingNumericEventArgs : EventArgs
    {
        /// <summary>
        /// The numeric representation of the output, if applicable.
        /// </summary>
        public uint? Output { get; set; }
        /// <summary>
        /// The numeric representation of the input, if applicable.
        /// </summary>
        public uint? Input { get; set; }

        /// <summary>
        /// The type of signal involved in the routing change.
        /// </summary>
        public eRoutingSignalType SigType { get; set; }
        /// <summary>
        /// The input port involved in the routing change, if applicable.
        /// </summary>
        public RoutingInputPort InputPort { get; set; }
        /// <summary>
        /// The output port involved in the routing change, if applicable.
        /// </summary>
        public RoutingOutputPort OutputPort { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingNumericEventArgs"/> class using numeric identifiers.
        /// </summary>
        /// <param name="output">The numeric output identifier.</param>
        /// <param name="input">The numeric input identifier.</param>
        /// <param name="sigType">The signal type.</param>
        public RoutingNumericEventArgs(uint output, uint input, eRoutingSignalType sigType) : this(output, input, null, null, sigType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingNumericEventArgs"/> class using port objects.
        /// </summary>
        /// <param name="outputPort">The output port object.</param>
        /// <param name="inputPort">The input port object.</param>
        /// <param name="sigType">The signal type.</param>
        public RoutingNumericEventArgs(RoutingOutputPort outputPort, RoutingInputPort inputPort,
            eRoutingSignalType sigType)
            : this(null, null, outputPort, inputPort, sigType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingNumericEventArgs"/> class with default values.
        /// </summary>
        public RoutingNumericEventArgs()
            : this(null, null, null, null, 0)
        {
 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingNumericEventArgs"/> class with potentially mixed identifiers.
        /// </summary>
        /// <param name="output">The numeric output identifier (optional).</param>
        /// <param name="input">The numeric input identifier (optional).</param>
        /// <param name="outputPort">The output port object (optional).</param>
        /// <param name="inputPort">The input port object (optional).</param>
        /// <param name="sigType">The signal type.</param>
        public RoutingNumericEventArgs(uint? output, uint? input, RoutingOutputPort outputPort,
            RoutingInputPort inputPort, eRoutingSignalType sigType)
        {
            OutputPort = outputPort;
            InputPort = inputPort;

            Output = output;
            Input = input;
            SigType = sigType;
        }
    }
}