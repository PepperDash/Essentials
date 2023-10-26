using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;


namespace PepperDash.Essentials.Core
{
    //*******************************************************************************************
	// Interfaces


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