﻿namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Represents a single switching step within a larger route, detailing the switching device, input port, and optionally the output port.
    /// </summary>
    public class RouteSwitchDescriptor
	{
		/// <summary>
		/// The device performing the switch (derived from the InputPort's parent).
		/// </summary>
		public IRoutingInputs SwitchingDevice { get { return InputPort?.ParentDevice; } }
		/// <summary>
		/// The output port being switched from (relevant for matrix switchers). Null for sink devices.
		/// </summary>
		public RoutingOutputPort OutputPort { get; set; }
		/// <summary>
		/// The input port being switched to.
		/// </summary>
		public RoutingInputPort InputPort { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RouteSwitchDescriptor"/> class for sink devices (no output port).
		/// </summary>
		/// <param name="inputPort">The input port being switched to.</param>
		public RouteSwitchDescriptor(RoutingInputPort inputPort)
		{
			InputPort = inputPort;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RouteSwitchDescriptor"/> class for matrix switchers.
		/// </summary>
		/// <param name="outputPort">The output port being switched from.</param>
		/// <param name="inputPort">The input port being switched to.</param>
		public RouteSwitchDescriptor(RoutingOutputPort outputPort, RoutingInputPort inputPort)
		{
			InputPort = inputPort;
			OutputPort = outputPort;
		}

		/// <summary>
		/// Returns a string representation of the route switch descriptor.
		/// </summary>
		/// <returns>A string describing the switch operation.</returns>
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