using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.DM.Config;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.DM.Chassis
{
	[Description("Wrapper class for all HdMdNxM4E switchers")]
	public class HdMdNxM4kEBridgeableController : CrestronGenericBridgeableBaseDevice, IRoutingNumericWithFeedback, IHasFeedback
	{
		private HdMdNxM _Chassis;
		private HdMd4x14kE _Chassis4x1;

		//IroutingNumericEvent
		public event EventHandler<RoutingNumericEventArgs> NumericSwitchChange;

		public Dictionary<uint, string> InputNames { get; set; }
		public Dictionary<uint, string> OutputNames { get; set; }

		public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }
		public RoutingPortCollection<RoutingOutputPort> OutputPorts { get; private set; }

		public FeedbackCollection<BoolFeedback> VideoInputSyncFeedbacks { get; private set; }
		public FeedbackCollection<IntFeedback> VideoOutputRouteFeedbacks { get; private set; }
		public FeedbackCollection<StringFeedback> InputNameFeedbacks { get; private set; }
		public FeedbackCollection<StringFeedback> OutputNameFeedbacks { get; private set; }
		public FeedbackCollection<StringFeedback> OutputRouteNameFeedbacks { get; private set; }
		public FeedbackCollection<BoolFeedback> InputHdcpEnableFeedback { get; private set; }
		public FeedbackCollection<StringFeedback> DeviceNameFeedback { get; private set; }
		public FeedbackCollection<BoolFeedback> AutoRouteFeedback { get; private set; }

		#region Constructor

		public HdMdNxM4kEBridgeableController(string key, string name, HdMdNxM chassis,
			HdMdNxM4kEBridgeablePropertiesConfig props)
			: base(key, name, chassis)
		{
			_Chassis = chassis;

			if (props == null)
			{
				Debug.Console(1, this, "HdMdNx4keBridgeableController properties are null, failed to build the device");
				return;
			}


			if (props.Inputs != null)
			{
				foreach (var kvp in props.Inputs)
				{
					Debug.Console(0, this, "props.Inputs: {0}-{1}", kvp.Key, kvp.Value);
				}
				InputNames = props.Inputs;
			}
			if (props.Outputs != null)
			{
				foreach (var kvp in props.Outputs)
				{
					Debug.Console(0, this, "props.Outputs: {0}-{1}", kvp.Key, kvp.Value);
				}
				OutputNames = props.Outputs;
			}

			VideoInputSyncFeedbacks = new FeedbackCollection<BoolFeedback>();
			VideoOutputRouteFeedbacks = new FeedbackCollection<IntFeedback>();
			InputNameFeedbacks = new FeedbackCollection<StringFeedback>();
			OutputNameFeedbacks = new FeedbackCollection<StringFeedback>();
			OutputRouteNameFeedbacks = new FeedbackCollection<StringFeedback>();
			InputHdcpEnableFeedback = new FeedbackCollection<BoolFeedback>();
			DeviceNameFeedback = new FeedbackCollection<StringFeedback>();
			AutoRouteFeedback = new FeedbackCollection<BoolFeedback>();

			InputPorts = new RoutingPortCollection<RoutingInputPort>();
			OutputPorts = new RoutingPortCollection<RoutingOutputPort>();

			var deviceName = string.IsNullOrEmpty(this.Name) ? name : this.Name;
			DeviceNameFeedback.Add(new StringFeedback(deviceName, () => deviceName));				

			if (_Chassis.NumberOfInputs == 1)
			{
				_Chassis4x1 = _Chassis as HdMd4x14kE;
				AutoRouteFeedback.Add(new BoolFeedback(deviceName + "-" + InputNames[1], () => _Chassis4x1.AutoModeOnFeedback.BoolValue));
			}

			for (uint i = 1; i <= _Chassis.NumberOfInputs; i++)
			{
				var index = i;
				var inputName = InputNames[index];
				_Chassis.Inputs[index].Name.StringValue = inputName;

				InputPorts.Add(new RoutingInputPort(inputName, eRoutingSignalType.AudioVideo,
					eRoutingPortConnectionType.Hdmi, index, this)
				{
					FeedbackMatchObject = _Chassis.HdmiInputs[index]
				});
				VideoInputSyncFeedbacks.Add(new BoolFeedback(inputName, () => _Chassis.Inputs[index].VideoDetectedFeedback.BoolValue));
				InputNameFeedbacks.Add(new StringFeedback(inputName, () => _Chassis.Inputs[index].Name.StringValue));
				InputHdcpEnableFeedback.Add(new BoolFeedback(inputName, () => _Chassis.HdmiInputs[index].HdmiInputPort.HdcpSupportOnFeedback.BoolValue));
			}

			for (uint i = 1; i <= _Chassis.NumberOfOutputs; i++)
			{
				var index = i;
				var outputName = OutputNames[index];
				//_Chassis.Outputs[i].Name.StringValue = outputName;

				OutputPorts.Add(new RoutingOutputPort(outputName, eRoutingSignalType.AudioVideo,
					eRoutingPortConnectionType.Hdmi, index, this)
				{
					FeedbackMatchObject = _Chassis.HdmiOutputs[index]
				});
				VideoOutputRouteFeedbacks.Add(new IntFeedback(outputName, () => (int)_Chassis.Outputs[index].VideoOutFeedback.Number));
				OutputNameFeedbacks.Add(new StringFeedback(outputName, () => OutputNames[index]));
				OutputRouteNameFeedbacks.Add(new StringFeedback(outputName, () => _Chassis.Outputs[index].VideoOutFeedback.NameFeedback.StringValue));
			}

			_Chassis.DMInputChange += Chassis_DMInputChange;
			_Chassis.DMOutputChange += Chassis_DMOutputChange;

			AddPostActivationAction(AddFeedbackCollections);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Raise an event when the status of a switch object changes.
		/// </summary>
		/// <param name="e">Arguments defined as IKeyName sender, output, input, and eRoutingSignalType</param>
		private void OnSwitchChange(RoutingNumericEventArgs e)
		{
			var newEvent = NumericSwitchChange;
			if (newEvent != null) newEvent(this, e);
		}

		public void EnableHdcp(uint port)
		{
			if (port > _Chassis.NumberOfInputs) return;
			if (port <= 0) return;

			_Chassis.HdmiInputs[port].HdmiInputPort.HdcpSupportOn();
			InputHdcpEnableFeedback[InputNames[port]].FireUpdate();
		}

		public void DisableHdcp(uint port)
		{
			if (port > _Chassis.NumberOfInputs) return;
			if (port <= 0) return;

			_Chassis.HdmiInputs[port].HdmiInputPort.HdcpSupportOff();
			InputHdcpEnableFeedback[InputNames[port]].FireUpdate();
		}

		public void EnableAutoRoute()
		{
			if (_Chassis.NumberOfInputs != 1) return;

			if (_Chassis4x1 == null) return;

			_Chassis4x1.AutoModeOn();
		}

		public void DisableAutoRoute()
		{
			if (_Chassis.NumberOfInputs != 1) return;

			if (_Chassis4x1 == null) return;

			_Chassis4x1.AutoModeOff();
		}

		#region PostActivate

		public void AddFeedbackCollections()
		{
			AddCollectionsToList(VideoInputSyncFeedbacks, InputHdcpEnableFeedback);
			AddCollectionsToList(VideoOutputRouteFeedbacks);
			AddCollectionsToList(InputNameFeedbacks, OutputNameFeedbacks, OutputRouteNameFeedbacks, DeviceNameFeedback);
		}

		#endregion

		#region FeedbackCollection Methods

		//Add arrays of collections
		public void AddCollectionsToList(params FeedbackCollection<BoolFeedback>[] newFbs)
		{
			foreach (FeedbackCollection<BoolFeedback> fbCollection in newFbs)
			{
				foreach (var item in newFbs)
				{
					AddCollectionToList(item);
				}
			}
		}
		public void AddCollectionsToList(params FeedbackCollection<IntFeedback>[] newFbs)
		{
			foreach (FeedbackCollection<IntFeedback> fbCollection in newFbs)
			{
				foreach (var item in newFbs)
				{
					AddCollectionToList(item);
				}
			}
		}

		public void AddCollectionsToList(params FeedbackCollection<StringFeedback>[] newFbs)
		{
			foreach (FeedbackCollection<StringFeedback> fbCollection in newFbs)
			{
				foreach (var item in newFbs)
				{
					AddCollectionToList(item);
				}
			}
		}

		//Add Collections
		public void AddCollectionToList(FeedbackCollection<BoolFeedback> newFbs)
		{
			foreach (var f in newFbs)
			{
				if (f == null) continue;

				AddFeedbackToList(f);
			}
		}

		public void AddCollectionToList(FeedbackCollection<IntFeedback> newFbs)
		{
			foreach (var f in newFbs)
			{
				if (f == null) continue;

				AddFeedbackToList(f);
			}
		}

		public void AddCollectionToList(FeedbackCollection<StringFeedback> newFbs)
		{
			foreach (var f in newFbs)
			{
				if (f == null) continue;

				AddFeedbackToList(f);
			}
		}

		//Add Individual Feedbacks
		public void AddFeedbackToList(PepperDash.Essentials.Core.Feedback newFb)
		{
			if (newFb == null) return;

			if (!Feedbacks.Contains(newFb))
			{
				Feedbacks.Add(newFb);
			}
		}

		#endregion

		#region IRouting Members

		public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
		{
			// Try to make switch only when necessary.  The unit appears to toggle when already selected.
			var current = _Chassis.HdmiOutputs[(uint)outputSelector].VideoOut;
			if (current != _Chassis.HdmiInputs[(uint)inputSelector])
				_Chassis.HdmiOutputs[(uint)outputSelector].VideoOut = _Chassis.HdmiInputs[(uint)inputSelector];
		}

		#endregion

		#region IRoutingNumeric Members

		public void ExecuteNumericSwitch(ushort inputSelector, ushort outputSelector, eRoutingSignalType signalType)
		{
			ExecuteSwitch(inputSelector, outputSelector, signalType);
		}

		#endregion

		#endregion

		#region Bridge Linking

		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			var joinMap = new HdMdNxM4kEControllerJoinMap(joinStart);

			var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

			if (!string.IsNullOrEmpty(joinMapSerialized))
				joinMap = JsonConvert.DeserializeObject<HdMdNxM4kEControllerJoinMap>(joinMapSerialized);

			if (bridge != null)
			{
				bridge.AddJoinMap(Key, joinMap);
			}
			else
			{
				Debug.Console(0, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
			}

			IsOnline.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
			DeviceNameFeedback[this.Name].LinkInputSig(trilist.StringInput[joinMap.Name.JoinNumber]);

			if (_Chassis4x1 != null)
			{
				trilist.SetSigTrueAction(joinMap.EnableAutoRoute.JoinNumber, () => _Chassis4x1.AutoModeOn());
				trilist.SetSigFalseAction(joinMap.EnableAutoRoute.JoinNumber, () => _Chassis4x1.AutoModeOff());
				AutoRouteFeedback[this.Name + "-" + InputNames[1]].LinkInputSig(trilist.BooleanInput[joinMap.EnableAutoRoute.JoinNumber]);
			}

			for (uint i = 1; i <= _Chassis.NumberOfInputs; i++)
			{
				var joinIndex = i - 1;
				//Digital
				VideoInputSyncFeedbacks[InputNames[i]].LinkInputSig(trilist.BooleanInput[joinMap.InputSync.JoinNumber + joinIndex]);
				InputHdcpEnableFeedback[InputNames[i]].LinkInputSig(trilist.BooleanInput[joinMap.EnableInputHdcp.JoinNumber + joinIndex]);
				InputHdcpEnableFeedback[InputNames[i]].LinkComplementInputSig(trilist.BooleanInput[joinMap.DisableInputHdcp.JoinNumber + joinIndex]);
				trilist.SetSigTrueAction(joinMap.EnableInputHdcp.JoinNumber + joinIndex, () => EnableHdcp(i));
				trilist.SetSigTrueAction(joinMap.DisableInputHdcp.JoinNumber + joinIndex, () => DisableHdcp(i));

				//Serial
				InputNameFeedbacks[InputNames[i]].LinkInputSig(trilist.StringInput[joinMap.InputName.JoinNumber + joinIndex]);
			}

			for (uint i = 1; i <= _Chassis.NumberOfOutputs; i++)
			{
				var joinIndex = i - 1;
				//Analog
				VideoOutputRouteFeedbacks[OutputNames[i]].LinkInputSig(trilist.UShortInput[joinMap.OutputRoute.JoinNumber + joinIndex]);
				trilist.SetUShortSigAction(joinMap.OutputRoute.JoinNumber + joinIndex, (a) => ExecuteSwitch(a, i, eRoutingSignalType.AudioVideo));

				//Serial
				OutputNameFeedbacks[OutputNames[i]].LinkInputSig(trilist.StringInput[joinMap.OutputName.JoinNumber + joinIndex]);
				OutputRouteNameFeedbacks[OutputNames[i]].LinkInputSig(trilist.StringInput[joinMap.OutputRoutedName.JoinNumber + joinIndex]);
			}

			_Chassis.OnlineStatusChange += Chassis_OnlineStatusChange;

			trilist.OnlineStatusChange += (d, args) =>
			{
			    if (args.DeviceOnLine)
			    {
			        foreach (var feedback in Feedbacks)
			        {
			            feedback.FireUpdate();
			        }
			    }
			};
		}


		#endregion

		#region Events

		void Chassis_OnlineStatusChange(Crestron.SimplSharpPro.GenericBase currentDevice, Crestron.SimplSharpPro.OnlineOfflineEventArgs args)
		{
			if (!args.DeviceOnLine) return;
			for (uint i = 1; i <= _Chassis.NumberOfInputs; i++)
			{
				_Chassis.Inputs[i].Name.StringValue = InputNames[i];
			}
			for (uint i = 1; i <= _Chassis.NumberOfOutputs; i++)
			{
				_Chassis.Outputs[i].Name.StringValue = OutputNames[i];
			}

			foreach (var feedback in Feedbacks)
			{
				feedback.FireUpdate();
			}
		}

		void Chassis_DMOutputChange(Switch device, DMOutputEventArgs args)
		{
			if (args.EventId != DMOutputEventIds.VideoOutEventId) return;

			for (var i = 0; i < VideoOutputRouteFeedbacks.Count; i++)
			{
				var index = i;
				var localInputPort = InputPorts.FirstOrDefault(p => (DMInput)p.FeedbackMatchObject == _Chassis.HdmiOutputs[(uint)index + 1].VideoOutFeedback);
				var localOutputPort =
					OutputPorts.FirstOrDefault(p => (DMOutput)p.FeedbackMatchObject == _Chassis.HdmiOutputs[(uint)index + 1]);


				VideoOutputRouteFeedbacks[i].FireUpdate();
				OnSwitchChange(new RoutingNumericEventArgs((ushort)i, VideoOutputRouteFeedbacks[i].UShortValue, localOutputPort, localInputPort, eRoutingSignalType.AudioVideo));
			}
		}

		void Chassis_DMInputChange(Switch device, DMInputEventArgs args)
		{
			if (args.EventId != DMInputEventIds.VideoDetectedEventId) return;
			foreach (var item in VideoInputSyncFeedbacks)
			{
				item.FireUpdate();
			}
		}

		#endregion

		#region Factory

		public class HdMdNxM4kEControllerFactory : EssentialsDeviceFactory<HdMdNxM4kEBridgeableController>
		{
			public HdMdNxM4kEControllerFactory()
			{
				TypeNames = new List<string>() { "hdmd4x14ke-bridgeable", "hdmd4x24ke", "hdmd6x24ke" };
			}

			public override EssentialsDevice BuildDevice(DeviceConfig dc)
			{
				Debug.Console(1, "Factory Attempting to create new HD-MD-NxM-4K-E Device");

				var props = JsonConvert.DeserializeObject<HdMdNxM4kEBridgeablePropertiesConfig>(dc.Properties.ToString());

				var type = dc.Type.ToLower();
				var control = props.Control;
				var ipid = control.IpIdInt;
				var address = control.TcpSshProperties.Address;

				switch (type)
				{
					case ("hdmd4x14ke-bridgeable"):
						return new HdMdNxM4kEBridgeableController(dc.Key, dc.Name, new HdMd4x14kE(ipid, address, Global.ControlSystem), props);
					case ("hdmd4x24ke"):
						return new HdMdNxM4kEBridgeableController(dc.Key, dc.Name, new HdMd4x24kE(ipid, address, Global.ControlSystem), props);
					case ("hdmd6x24ke"):
						return new HdMdNxM4kEBridgeableController(dc.Key, dc.Name, new HdMd6x24kE(ipid, address, Global.ControlSystem), props);
					default:
						return null;
				}
			}
		}

		#endregion



	}
}