using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash_Essentials_Core.Bridges;
using PepperDash_Essentials_DM.Config;

namespace PepperDash_Essentials_DM.Chassis
{
	[Description("Wrapper class for all HdPsXxx switchers")]
	public class HdPsXxxController : CrestronGenericBridgeableBaseDevice, IRoutingNumericWithFeedback
	{
		private readonly HdPsXxx _chassis;

		public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		public Dictionary<uint, string> InputNames { get; set; }
		public Dictionary<uint, string> OutputNames { get; set; }

		public FeedbackCollection<StringFeedback> InputNameFeedbacks { get; private set; }
		public FeedbackCollection<BoolFeedback> InputHdcpEnableFeedback { get; private set; }

		public FeedbackCollection<StringFeedback> OutputNameFeedbacks { get; private set; }
		public FeedbackCollection<StringFeedback> OutputRouteNameFeedback { get; private set; }

		public FeedbackCollection<BoolFeedback> VideoInputSyncFeedbacks { get; private set; }
		public FeedbackCollection<IntFeedback> VideoOutputRouteFeedbacks { get; private set; }

		public StringFeedback DeviceNameFeedback { get; private set; }
		public BoolFeedback AutoRouteFeedback { get; private set; }

		public event EventHandler<RoutingNumericEventArgs> NumericSwitchChange;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="chassis">HdPs401 device instance</param>
		/// <param name="props"></param>
		public HdPsXxxController(string key, string name, HdPsXxx chassis, HdPsXxxPropertiesConfig props)
			: base(key, name, chassis)
		{
			_chassis = chassis;
			Name = name;

			if (props == null)
			{
				Debug.Console(1, this, "HdPsXxxController properties are null, failed to build device");
				return;
			}

			InputPorts = new RoutingPortCollection<RoutingInputPort>();
			InputNameFeedbacks = new FeedbackCollection<StringFeedback>();
			InputHdcpEnableFeedback = new FeedbackCollection<BoolFeedback>();
			InputNames = new Dictionary<uint, string>();

			OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
			OutputNameFeedbacks = new FeedbackCollection<StringFeedback>();
			OutputRouteNameFeedback = new FeedbackCollection<StringFeedback>();
			OutputNames = new Dictionary<uint, string>();

			VideoInputSyncFeedbacks = new FeedbackCollection<BoolFeedback>();
			VideoOutputRouteFeedbacks = new FeedbackCollection<IntFeedback>();

			if (_chassis.NumberOfOutputs == 1)
				AutoRouteFeedback = new BoolFeedback(() => _chassis.PriorityRouteOnFeedback.BoolValue);

			InputNames = props.Inputs;
			SetupInputs(InputNames);

			OutputNames = props.Outputs;
			SetupOutputs(OutputNames);
		}

		// input setup
		private void SetupInputs(Dictionary<uint, string> dict)
		{
			if (dict == null)
			{
				Debug.Console(1, this, "Failed to setup inputs, properties are null");
				return;
			}
			foreach (var kvp in dict)
			{
				Debug.Console(1, this, "props.Input[{0}]: {1}", kvp.Key, kvp.Value);
			}

			foreach (var item in _chassis.HdmiInputs)
			{
				var input = item;
				var index = item.Number;
				var key = string.Format("hdmiIn{0}", index);
				var name = string.IsNullOrEmpty(InputNames[index]) ? string.Format("HDMI Input {0}", index) : InputNames[index];

				InputNameFeedbacks.Add(new StringFeedback(name, () => InputNames[index]));

				var port = new RoutingInputPort(key, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, input, this)
				{
					FeedbackMatchObject = input
				};
				Debug.Console(1, this, "Adding Input port: {0}", port.Key);
				InputPorts.Add(port);

				InputHdcpEnableFeedback.Add(new BoolFeedback(name, () => input.InputPort.HdcpSupportOnFeedback.BoolValue));

				VideoInputSyncFeedbacks.Add(new BoolFeedback(name, () => input.VideoDetectedFeedback.BoolValue));
			}

			foreach (var item in _chassis.DmLiteInputs)
			{
				var input = item;
				var index = item.Number;
				var key = string.Format("dmLiteIn{0}", index);
				var name = string.IsNullOrEmpty(InputNames[index]) ? string.Format("DM Input {0}", index) : InputNames[index];
				input.Name.StringValue = name;

				InputNameFeedbacks.Add(new StringFeedback(name, () => InputNames[index]));

				var port = new RoutingInputPort(key, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, input, this)
				{
					FeedbackMatchObject = input
				};
				Debug.Console(0, this, "Adding Input port: {0}", port.Key);
				InputPorts.Add(port);

				InputHdcpEnableFeedback.Add(new BoolFeedback(name, () => input.InputPort.HdcpSupportOnFeedback.BoolValue));

				VideoInputSyncFeedbacks.Add(new BoolFeedback(name, () => input.VideoDetectedFeedback.BoolValue));
			}

			_chassis.DMInputChange += _chassis_InputChange;
		}

		// output setup
		private void SetupOutputs(Dictionary<uint, string> dict)
		{
			if (dict == null)
			{
				Debug.Console(1, this, "Failed to setup outputs, properties are null");
				return;
			}
			foreach (var kvp in dict)
			{
				Debug.Console(1, this, "props.Output[{0}]: {1}", kvp.Key, kvp.Value);
			}

			foreach (var item in _chassis.HdmiDmLiteOutputs)
			{
				var output = item;
				var index = item.Number;
				var key = string.Format("hdmiDmLiteOut{0}", index);
				var name = string.IsNullOrEmpty(OutputNames[index]) ? string.Format("Output {0}", index) : OutputNames[index];
				output.Name.StringValue = name;

				var port = new RoutingOutputPort(key, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, output, this)
				{
					FeedbackMatchObject = output,
					// set port for CEC
					Port = output
				};
				Debug.Console(0, this, "Adding Output port: {0}", port.Key);
				OutputPorts.Add(port);

				OutputRouteNameFeedback.Add(new StringFeedback(name, () => output.VideoOutFeedback.NameFeedback.StringValue));

				VideoOutputRouteFeedbacks.Add(new IntFeedback(name, () => output.VideoOutFeedback == null ? 0 : (int)output.VideoOutFeedback.Number));

				// TODO [ ] Investigate setting input priorities per output
				// {{in1-priority-level}, {in2-priority-level}, .... {in6-priority-level}}
				// default priority level input 1-4 ascending
				output.OutputPort.InputPriorities(new byte[] { 1, 2, 3, 4 });

				if (port.Port == null) continue;

				var hdmiOutputStreamCec = output.HdmiOutput.HdmiOutputPort.StreamCec;
				if (hdmiOutputStreamCec != null)
				{
					var streamCec = new StreamCecWrapper(string.Format("{0}-hdmiOut{1}-streamCec", Key, index), hdmiOutputStreamCec);
					DeviceManager.AddDevice(streamCec);
				}

				var dmLiteOutputStreamCec = output.DmLiteOutput.DmLiteOutputPort.StreamCec;
				if (dmLiteOutputStreamCec != null)
				{
					var streamCec = new StreamCecWrapper(string.Format("{0}-dmLiteOut{1}-streamCec", Key, index), dmLiteOutputStreamCec);
					DeviceManager.AddDevice(streamCec);
				}
			}

			_chassis.DMOutputChange += _chassis_OutputChange;
		}


		#region BridgeLinking

		/// <summary>
		/// Link device to API
		/// </summary>
		/// <param name="trilist"></param>
		/// <param name="joinStart"></param>
		/// <param name="joinMapKey"></param>
		/// <param name="bridge"></param>
		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			var joinMap = new HdPsXxxControllerJoinMap(joinStart);

			if (bridge != null)
			{
				bridge.AddJoinMap(Key, joinMap);
			}
			else
			{
				Debug.Console(0, this, "Please update config to use 'eiscApiAdvanced' to get all join map features for this device");
			}

			IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
			DeviceNameFeedback.LinkInputSig(trilist.StringInput[joinMap.Name.JoinNumber]);

			_chassis.OnlineStatusChange += _chassis_OnlineStatusChange;

			LinkChassisInputsToApi(trilist, joinMap);
			LinkChassisOutputsToApi(trilist, joinMap);

			trilist.OnlineStatusChange += (sender, args) =>
			{
				if (!args.DeviceOnLine) return;
			};
		}


		// links inputs to API
		private void LinkChassisInputsToApi(BasicTriList trilist, HdPsXxxControllerJoinMap joinMap)
		{
			for (uint i = 1; i <= _chassis.NumberOfInputs; i++)
			{
				var input = i;
				var inputName = InputNames[input];
				var indexWithOffset = input - 1;

				trilist.SetSigTrueAction(joinMap.EnableInputHdcp.JoinNumber + indexWithOffset, () => EnableHdcp(input));
				trilist.SetSigTrueAction(joinMap.DisableInputHdcp.JoinNumber + indexWithOffset, () => DisableHdcp(input));

				InputHdcpEnableFeedback[inputName].LinkInputSig(trilist.BooleanInput[joinMap.EnableInputHdcp.JoinNumber + indexWithOffset]);
				InputHdcpEnableFeedback[inputName].LinkComplementInputSig(trilist.BooleanInput[joinMap.EnableInputHdcp.JoinNumber + indexWithOffset]);

				VideoInputSyncFeedbacks[inputName].LinkInputSig(trilist.BooleanInput[joinMap.InputSync.JoinNumber + indexWithOffset]);

				InputNameFeedbacks[inputName].LinkInputSig(trilist.StringInput[joinMap.InputName.JoinNumber + indexWithOffset]);
			}
		}


		// links outputs to API
		private void LinkChassisOutputsToApi(BasicTriList trilist, HdPsXxxControllerJoinMap joinMap)
		{
			for (uint i = 1; i <= _chassis.NumberOfOutputs; i++)
			{
				var output = i;
				var outputName = OutputNames[output];
				var indexWithOffset = output - 1;

				trilist.SetUShortSigAction(joinMap.OutputRoute.JoinNumber + indexWithOffset, (a) =>
					ExecuteNumericSwitch(a, (ushort)output, eRoutingSignalType.AudioVideo));

				OutputNameFeedbacks[outputName].LinkInputSig(trilist.StringInput[joinMap.OutputName.JoinNumber + indexWithOffset]);
				OutputRouteNameFeedback[outputName].LinkInputSig(trilist.StringInput[joinMap.OutputRoutedName.JoinNumber + indexWithOffset]);

				VideoOutputRouteFeedbacks[outputName].LinkInputSig(trilist.UShortInput[joinMap.OutputRoute.JoinNumber + indexWithOffset]);
			}

			AutoRouteFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableAutoRoute.JoinNumber]);
		}

		#endregion


		/// <summary>
		/// Executes a device switch using objects
		/// </summary>
		/// <param name="inputSelector"></param>
		/// <param name="outputSelector"></param>
		/// <param name="signalType"></param>
		public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
		{
			var input = inputSelector as HdPsXxxHdmiInput;
			var output = outputSelector as HdPsXxxHdmiOutput;

			Debug.Console(2, this, "ExecuteSwitch: input={0}, output={1}", input, output);

			if (output == null)
			{
				Debug.Console(0, this, "Unable to make switch, output selector is not HdPsXxxHdmiOutput");
				return;
			}

			// TODO [ ] Validate if sending the same input toggles the switch
			var current = output.VideoOut;
			if (current != input)
				output.VideoOut = input;
		}


		/// <summary>
		/// Executes a device switch using numeric values
		/// </summary>
		/// <param name="inputSelector"></param>
		/// <param name="outputSelector"></param>
		/// <param name="signalType"></param>
		public void ExecuteNumericSwitch(ushort inputSelector, ushort outputSelector, eRoutingSignalType signalType)
		{
			var input = inputSelector == 0 ? null : _chassis.Inputs[inputSelector];
			var output = _chassis.Outputs[outputSelector];

			Debug.Console(2, this, "ExecuteNumericSwitch: input={0}, output={1}", input, output);

			ExecuteSwitch(input, output, signalType);
		}


		/// <summary>
		/// Enables Hdcp on the provided port
		/// </summary>
		/// <param name="port"></param>
		public void EnableHdcp(uint port)
		{
			if (port <= 0 || port > _chassis.NumberOfInputs) return;

			_chassis.HdmiInputs[port].InputPort.HdcpSupportOn();
			InputHdcpEnableFeedback[InputNames[port]].FireUpdate();
		}


		/// <summary>
		/// Disables Hdcp on the provided port
		/// </summary>
		/// <param name="port"></param>
		public void DisableHdcp(uint port)
		{
			if (port <= 0 || port > _chassis.NumberOfInputs) return;

			_chassis.HdmiInputs[port].InputPort.HdcpSupportOff();
			InputHdcpEnableFeedback[InputNames[port]].FireUpdate();
		}


		/// <summary>
		/// Enables switcher auto route
		/// </summary>
		public void EnableAutoRoute()
		{
			if (_chassis.NumberOfInputs == 1) return;

			_chassis.AutoRouteOn();
		}


		/// <summary>
		/// Disables switcher auto route
		/// </summary>
		public void DisableAutoRoute()
		{
			if (_chassis.NumberOfInputs == 1) return;

			_chassis.AutoRouteOff();
		}

		#region Events


		// _chassis online/offline event
		private void _chassis_OnlineStatusChange(GenericBase currentDevice,
			OnlineOfflineEventArgs args)
		{
			IsOnline.FireUpdate();

			if (!args.DeviceOnLine) return;

			foreach (var feedback in Feedbacks)
			{
				feedback.FireUpdate();
			}
		}


		// _chassis input change event
		private void _chassis_InputChange(Switch device, DMInputEventArgs args)
		{
			var eventId = args.EventId;

			switch (eventId)
			{
				case DMInputEventIds.VideoDetectedEventId:
					{
						Debug.Console(1, this, "Event ID {0}: Updating VideoInputSyncFeedbacks", eventId);
						foreach (var item in VideoInputSyncFeedbacks)
						{
							item.FireUpdate();
						}
						break;
					}
				case DMInputEventIds.InputNameFeedbackEventId:
				case DMInputEventIds.InputNameEventId:
				case DMInputEventIds.NameFeedbackEventId:
					{
						Debug.Console(1, this, "Event ID {0}: Updating name feedbacks", eventId);

						var input = args.Number;
						var name = _chassis.HdmiInputs[input].NameFeedback.StringValue;

						Debug.Console(1, this, "Input {0} Name {1}", input, name);
						break;
					}
				default:
					{
						Debug.Console(1, this, "Uhandled DM Input Event ID {0}", eventId);
						break;
					}
			}
		}


		// _chassis output change event
		private void _chassis_OutputChange(Switch device, DMOutputEventArgs args)
		{
			if (args.EventId != DMOutputEventIds.VideoOutEventId) return;

			var output = args.Number;

			var input = _chassis.HdmiDmLiteOutputs[output].VideoOutFeedback == null
				? 0
				: _chassis.HdmiDmLiteOutputs[output].VideoOutFeedback.Number;

			var outputName = OutputNames[output];

			var feedback = VideoOutputRouteFeedbacks[outputName];
			if (feedback == null) return;

			var inputPort = InputPorts.FirstOrDefault(
				p => p.FeedbackMatchObject == _chassis.HdmiDmLiteOutputs[output].VideoOutFeedback);

			var outputPort = OutputPorts.FirstOrDefault(
				p => p.FeedbackMatchObject == _chassis.HdmiDmLiteOutputs[output]);

			feedback.FireUpdate();

			OnSwitchChange(new RoutingNumericEventArgs(
				output, input, outputPort, inputPort, eRoutingSignalType.AudioVideo));
		}


		/// <summary>
		/// Raise an event when the status of a switch object changes.
		/// </summary>
		/// <param name="args">Argumetns defined as IKeyName sender, output, input, & eRoutingSignalType</param>
		private void OnSwitchChange(RoutingNumericEventArgs args)
		{
			var newEvent = NumericSwitchChange;
			if (newEvent != null) newEvent(this, args);
		}


		#endregion


		#region Factory


		public class HdSp401ControllerFactory : EssentialsDeviceFactory<HdPsXxxController>
		{
			public HdSp401ControllerFactory()
			{
				TypeNames = new List<string>() { "hdps401", "hdps402", "hdps621", "hdps622" };
			}
			public override EssentialsDevice BuildDevice(DeviceConfig dc)
			{
				var key = dc.Key;
				var name = dc.Name;
				var type = dc.Type.ToLower();

				Debug.Console(1, "Factory Attempting to create new {0} device", type);

				var props = JsonConvert.DeserializeObject<HdPsXxxPropertiesConfig>(dc.Properties.ToString());
				if (props == null)
				{
					Debug.Console(1, "Factory failed to create new HD-PSXxx device, properties config was null");
					return null;
				}

				var ipid = props.Control.IpIdInt;

				switch (type)
				{
					case ("hdps401"):
						{
							return new HdPsXxxController(key, name, new HdPs401(ipid, Global.ControlSystem), props);
						}
					case ("hdps402"):
						{
							return new HdPsXxxController(key, name, new HdPs402(ipid, Global.ControlSystem), props);
						}
					case ("hdps621"):
						{
							return new HdPsXxxController(key, name, new HdPs621(ipid, Global.ControlSystem), props);
						}
					case ("hdps622"):
						{
							return new HdPsXxxController(key, name, new HdPs622(ipid, Global.ControlSystem), props);
						}
					default:
						{
							Debug.Console(1, "Factory failed to create new {0} device", type);
							return null;
						}
				}
			}
		}


		#endregion
	}


	public class StreamCecWrapper : IKeyed, ICec
	{
		public string Key { get; private set; }
		public Cec StreamCec { get; private set; }

		public StreamCecWrapper(string key, Cec streamCec)
		{
			Key = key;
			StreamCec = streamCec;
		}
	}
}