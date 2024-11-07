namespace PepperDash.Essentials.Core.Routing
{
    /// <summary>
    /// Represents an individual link for a route
    /// </summary>
    public class RouteSwitchDescriptor
	{
		public IRoutingInputs SwitchingDevice { get { return InputPort?.ParentDevice; } }
		public RoutingOutputPort OutputPort { get; set; }
		public RoutingInputPort InputPort { get; set; }

		public RouteSwitchDescriptor(RoutingInputPort inputPort)
		{
			InputPort = inputPort;
		}

		public RouteSwitchDescriptor(RoutingOutputPort outputPort, RoutingInputPort inputPort)
		{
			InputPort = inputPort;
			OutputPort = outputPort;
		}

		public override string ToString()
		{
            if (SwitchingDevice is IRouting)
                return $"{(SwitchingDevice != null ? SwitchingDevice.Key : "No Device")} switches output {(OutputPort != null ? OutputPort.Key : "No output port")} to input {(InputPort != null ? InputPort.Key : "No input port")}";
            else
                return $"{(SwitchingDevice != null ? SwitchingDevice.Key : "No Device")} switches to input {(InputPort != null ? InputPort.Key : "No input port")}";
		}
	}

    /*/// <summary>
    /// Represents an individual link for a route
    /// </summary>
    public class RouteSwitchDescriptor<TInputSelector, TOutputSelector>
    {
        public IRoutingInputs<TInputSelector> SwitchingDevice { get { return InputPort.ParentDevice; } }
        public RoutingOutputPort<TOutputSelector> OutputPort { get; set; }
        public RoutingInputPort<TInputSelector> InputPort { get; set; }

        public RouteSwitchDescriptor(RoutingInputPort<TInputSelector> inputPort)
        {
            InputPort = inputPort;
        }

        public RouteSwitchDescriptor(RoutingOutputPort<TOutputSelector> outputPort, RoutingInputPort<TInputSelector> inputPort)
        {
            InputPort = inputPort;
            OutputPort = outputPort;
        }

        public override string ToString()
        {
            if (SwitchingDevice is IRouting)
                return string.Format("{0} switches output '{1}' to input '{2}'", SwitchingDevice.Key, OutputPort.Selector, InputPort.Selector);
            else
                return string.Format("{0} switches to input '{1}'", SwitchingDevice.Key, InputPort.Selector);
        }
    }*/
}