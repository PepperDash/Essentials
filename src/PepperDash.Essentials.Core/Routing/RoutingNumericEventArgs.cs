using System;

namespace PepperDash.Essentials.Core.Routing
{
    public class RoutingNumericEventArgs : EventArgs
    {

        public uint? Output { get; set; }
        public uint? Input { get; set; }

        public eRoutingSignalType SigType { get; set; }
        public RoutingInputPort InputPort { get; set; }
        public RoutingOutputPort OutputPort { get; set; }

        public RoutingNumericEventArgs(uint output, uint input, eRoutingSignalType sigType) : this(output, input, null, null, sigType)
        {
        }

        public RoutingNumericEventArgs(RoutingOutputPort outputPort, RoutingInputPort inputPort,
            eRoutingSignalType sigType)
            : this(null, null, outputPort, inputPort, sigType)
        {
        }

        public RoutingNumericEventArgs()
            : this(null, null, null, null, 0)
        {
 
        }

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