using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
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
	public class HdPsXxxController : CrestronGenericBridgeableBaseDevice, IRoutingNumericWithFeedback, IHasFeedback
	{

		private readonly HdPsXxx _chassis;
		private readonly HdPs401 _chassis401;
		private readonly HdPs621 _chassis621;

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
			: base(key, name)
		{
			_chassis = chassis;
			Name = name;

			if (props == null)
			{
				Debug.Console(1, this, "HdPsXxxController properties are null, failed to build device");
				return;
			}

			InputPorts = new RoutingPortCollection<RoutingInputPort>();
			InputNames = new Dictionary<uint, string>();
			InputNameFeedbacks = new FeedbackCollection<StringFeedback>();
			InputHdcpEnableFeedback = new FeedbackCollection<BoolFeedback>();

			OutputPorts = new RoutingPortCollection<RoutingOutputPort>();
			OutputNames = new Dictionary<uint, string>();
			OutputNameFeedbacks = new FeedbackCollection<StringFeedback>();
			OutputRouteNameFeedback = new FeedbackCollection<StringFeedback>();

			VideoInputSyncFeedbacks = new FeedbackCollection<BoolFeedback>();
			VideoOutputRouteFeedbacks = new FeedbackCollection<IntFeedback>();

			if (_chassis.NumberOfOutputs == 1)
			{
				if (_chassis is HdPs401)
					_chassis401 = _chassis as HdPs401;
				if (_chassis is HdPs621)
					_chassis621 = _chassis as HdPs621;

				AutoRouteFeedback = new BoolFeedback(() => _chassis401.PriorityRouteOnFeedback.BoolValue);
			}

			SetupInputs(props.Inputs);
			SetupOutputs(props.Outputs);

			AddPostActivationAction(AddFeedbackCollecitons);
		}

		// input setup
		private void SetupInputs(Dictionary<uint, string> dict)
		{
			InputNames = dict;

			for (uint i = 1; i <= _chassis.NumberOfInputs; i++)
			{
				var index = i;
				var name = string.IsNullOrEmpty(InputNames[index]) ? string.Format("Input {0}", index) : InputNames[index];
				var input = _chassis.Inputs[index];
				var hdmiInput = _chassis.HdmiInputs[index];
				var dmLiteInput = _chassis.DmLiteInputs[index];

				InputNameFeedbacks.Add(new StringFeedback(name, () => InputNames[index]));

				// TODO [ ] verify which input type is needed
				input.Name.StringValue = name;
				hdmiInput.Name.StringValue = name;
				dmLiteInput.Name.StringValue = name;

				var port = new RoutingInputPort(name, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, input, this)
				{
					FeedbackMatchObject = input
				};
				InputPorts.Add(port);

				InputHdcpEnableFeedback.Add(new BoolFeedback(name, () => hdmiInput.InputPort.HdcpSupportOnFeedback.BoolValue));

				VideoInputSyncFeedbacks.Add(new BoolFeedback(name, () => input.VideoDetectedFeedback.BoolValue));
			}

			_chassis.DMInputChange += _chassis_InputChange;			
		}

		// output setup
		private void SetupOutputs(Dictionary<uint, string> dict)
		{
			OutputNames = dict;

			for (uint i = 1; i <= _chassis.NumberOfOutputs; i++)
			{
				var index = i;
				var name = string.IsNullOrEmpty(OutputNames[index]) ? string.Format("Output {0}", index) : OutputNames[index];
				var output = _chassis.Outputs[index];
				var hdmiDmLiteOutput = _chassis.HdmiDmLiteOutputs[index];

				OutputNameFeedbacks.Add(new StringFeedback(name, () => OutputNames[index]));

				// TODO [ ] verify which output type is needed
				output.Name.StringValue = name;
				hdmiDmLiteOutput.Name.StringValue = name;

				var port = new RoutingOutputPort(name, eRoutingSignalType.AudioVideo, eRoutingPortConnectionType.Hdmi, output, this)
				{
					FeedbackMatchObject = output
				};
				OutputPorts.Add(port);

				OutputRouteNameFeedback.Add(new StringFeedback(name, () => output.VideoOutFeedback.NameFeedback.StringValue));

				VideoOutputRouteFeedbacks.Add(new IntFeedback(name, () => output.VideoOutFeedback == null ? 0 : (int)output.VideoOutFeedback.Number));
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

			if (_chassis401 != null) LinkChassis401ToApi(trilist, joinMap);

			if (_chassis621 != null) LinkChassis621ToApi(trilist, joinMap);

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
					ExecuteNumericSwitch(a, (ushort) output, eRoutingSignalType.AudioVideo));

				OutputNameFeedbacks[outputName].LinkInputSig(trilist.StringInput[joinMap.OutputName.JoinNumber + indexWithOffset]);
				OutputRouteNameFeedback[outputName].LinkInputSig(trilist.StringInput[joinMap.OutputRoutedName.JoinNumber + indexWithOffset]);

				VideoOutputRouteFeedbacks[outputName].LinkInputSig(trilist.UShortInput[joinMap.OutputRoute.JoinNumber + indexWithOffset]);
			}
		}


		// links HdPs401 chassis to API
		private void LinkChassis401ToApi(BasicTriList trilist, HdPsXxxControllerJoinMap joinMap)
		{
			trilist.SetSigTrueAction(joinMap.EnableAutoRoute.JoinNumber, () => _chassis401.AutoRouteOn());
			trilist.SetSigFalseAction(joinMap.EnableAutoRoute.JoinNumber, () => _chassis401.AutoRouteOff());

			AutoRouteFeedback.LinkInputSig(trilist.BooleanInput[joinMap.EnableAutoRoute.JoinNumber]);
		}


		// links HdPs621 chassis to API
		private void LinkChassis621ToApi(BasicTriList trilist, HdPsXxxControllerJoinMap joinMap)
		{
			trilist.SetSigTrueAction(joinMap.EnableAutoRoute.JoinNumber, () => _chassis621.AutoRouteOn());
			trilist.SetSigFalseAction(joinMap.EnableAutoRoute.JoinNumber, () => _chassis621.AutoRouteOff());

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
			if (_chassis.NumberOfInputs != 1) return;

			if (_chassis401 != null)
			{
				_chassis401.AutoRouteOn();
			}

			if (_chassis621 != null)
			{
				_chassis621.AutoRouteOn();
			}
		}


		/// <summary>
		/// Disables switcher auto route
		/// </summary>
		public void DisableAutoRoute()
		{
			if (_chassis.NumberOfInputs != 1) return;

			if (_chassis401 != null)
			{
				_chassis401.AutoRouteOff();
			}

			if (_chassis621 != null)
			{
				_chassis621.AutoRouteOff();
			}
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


		#region FeedbacksAndFeedbackCollections


		/// <summary>
		/// Add feedback colleciton arrays to feedback collections 
		/// </summary>
		/// <param name="feedbackCollections">BoolFeedback[] arrays</param>
		public void AddCollectionsToList(params FeedbackCollection<BoolFeedback>[] feedbackCollections)
		{
			foreach (var item in feedbackCollections.SelectMany(feedbackCollection => feedbackCollections))
			{
				AddCollectionsToList(item);
			}
		}


		/// <summary>
		/// Add feedback colleciton arrays to feedback collections 
		/// </summary>
		/// <param name="feedbackCollections">IntFeedback[] arrays</param>
		public void AddCollectionsToList(params FeedbackCollection<IntFeedback>[] feedbackCollections)
		{
			foreach (var item in feedbackCollections.SelectMany(feedbackCollection => feedbackCollections))
			{
				AddCollectionsToList(item);
			}
		}


		/// <summary>
		/// Add feedback colleciton arrays to feedback collections 
		/// </summary>
		/// <param name="feedbackCollections">StringFeedback[] arrays</param>
		public void AddCollectionsToList(params FeedbackCollection<StringFeedback>[] feedbackCollections)
		{
			foreach (var item in feedbackCollections.SelectMany(feedbackCollection => feedbackCollections))
			{
				AddCollectionsToList(item);
			}
		}


		/// <summary>
		/// Adds feedback colleciton to feedback collections
		/// </summary>
		/// <param name="feedbackCollection">BoolFeedback</param>
		public void AddCollectionToList(FeedbackCollection<BoolFeedback> feedbackCollection)
		{
			foreach (var item in feedbackCollection.Where(item => item != null))
			{
				AddFeedbackToList(item);
			}
		}


		/// <summary>
		/// Adds feedback colleciton to feedback collections
		/// </summary>
		/// <param name="feedbackCollection">IntFeedback</param>
		public void AddCollectionToList(FeedbackCollection<IntFeedback> feedbackCollection)
		{
			foreach (var item in feedbackCollection.Where(item => item != null))
			{
				AddFeedbackToList(item);
			}
		}


		/// <summary>
		/// Adds feedback colleciton to feedback collections
		/// </summary>
		/// <param name="feedbackCollection">StringFeedback</param>
		public void AddCollectionToList(FeedbackCollection<StringFeedback> feedbackCollection)
		{
			foreach (var item in feedbackCollection.Where(item => item != null))
			{
				AddFeedbackToList(item);
			}
		}


		/// <summary>
		/// Adds individual feedbacks to feedback collection
		/// </summary>
		/// <param name="fb">Feedback</param>
		public void AddFeedbackToList(PepperDash.Essentials.Core.Feedback fb)
		{
			if (fb == null || Feedbacks.Contains(fb)) return;

			Feedbacks.Add(fb);
		}


		/// <summary>
		/// Adds provided feedbacks to feedback collection list
		/// </summary>
		public void AddFeedbackCollecitons()
		{
			AddFeedbackToList(DeviceNameFeedback);
			AddCollectionsToList(VideoInputSyncFeedbacks, InputHdcpEnableFeedback);
			AddCollectionsToList(VideoOutputRouteFeedbacks);
			AddCollectionsToList(InputNameFeedbacks, OutputNameFeedbacks, OutputRouteNameFeedback);
		}


		#endregion


		#region Factory


		public class HdSp401ControllerFactory : EssentialsDeviceFactory<HdPsXxxController>
		{
			public HdSp401ControllerFactory()
			{
				TypeNames = new List<string>() { "hdsp401", "hdsp402", "hdsp621", "hdsp622" };
			}
			public override EssentialsDevice BuildDevice(DeviceConfig dc)
			{
				Debug.Console(1, "Factory Attempting to create new HD-PSXxx device");

				var props = JsonConvert.DeserializeObject<HdPsXxxPropertiesConfig>(dc.Properties.ToString());
				if (props == null)
				{
					Debug.Console(1, "Factory failed to create new HD-PSXxx device, properties config was null");
					return null;
				}

				var key = dc.Key;
				var name = dc.Name;
				var type = dc.Type.ToLower();
				var control = props.Control;
				var ipid = control.IpIdInt;
				var address = control.TcpSshProperties.Address;

				switch (type)
				{
					case ("hdps401"):
						{
							return new HdPsXxxController(key, name, new HdPs401(ipid, Global.ControlSystem), props);
						}
					case ("hdsp402"):
						{
							return new HdPsXxxController(key, name, new HdPs402(ipid, Global.ControlSystem), props);
						}
					case ("hdsp621"):
						{
							return new HdPsXxxController(key, name, new HdPs621(ipid, Global.ControlSystem), props);
						}
					case ("hdsp622"):
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
}