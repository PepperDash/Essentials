using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Routing;
using Serilog.Events;
using Feedback = PepperDash.Essentials.Core.Feedback;

namespace PepperDash.Essentials.Devices.Common.Displays
{
	/// <summary>
	/// Abstract base class for display devices that provides common display functionality
	/// including power control, input switching, and routing capabilities.
	/// </summary>
	public abstract class DisplayBase : EssentialsDevice, IDisplay, ICurrentSources, IHasFeedback
	{
		private RoutingInputPort _currentInputPort;

		/// <summary>
		/// Gets or sets the current input port that is selected on the display.
		/// </summary>
		public RoutingInputPort CurrentInputPort
		{
			get
			{
				return _currentInputPort;
			}

			protected set
			{
				if (_currentInputPort == value) return;

				_currentInputPort = value;

				InputChanged?.Invoke(this, _currentInputPort);
			}
		}

		/// <summary>
		/// Event that is raised when the input changes on the display.
		/// </summary>
		public event InputChangedEventHandler InputChanged;

		/// <summary>
		/// Event that is raised when the current source information changes.
		/// </summary>
		public event SourceInfoChangeHandler CurrentSourceChange;

		/// <summary>
		/// Gets or sets the CurrentSourceInfoKey
		/// </summary>
		public string CurrentSourceInfoKey { get; set; }

		/// <summary>
		/// Gets or sets the current source information for the display.
		/// </summary>
		public SourceListItem CurrentSourceInfo
		{
			get
			{
				return _CurrentSourceInfo;
			}
			set
			{
				if (value == _CurrentSourceInfo) return;

				var handler = CurrentSourceChange;

				handler?.Invoke(_CurrentSourceInfo, ChangeType.WillChange);

				_CurrentSourceInfo = value;

				handler?.Invoke(_CurrentSourceInfo, ChangeType.DidChange);
			}
		}
		SourceListItem _CurrentSourceInfo;

		/// <inheritdoc/> 
		public Dictionary<eRoutingSignalType, SourceListItem> CurrentSources { get; private set; }

		/// <inheritdoc/>
		public Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; private set; }

		/// <inheritdoc />
		public event EventHandler CurrentSourcesChanged;

		/// <summary>
		/// Gets feedback indicating whether the display is currently cooling down after being powered off.
		/// </summary>
		public BoolFeedback IsCoolingDownFeedback { get; protected set; }

		/// <summary>
		/// Gets or sets the IsWarmingUpFeedback
		/// </summary>
		public BoolFeedback IsWarmingUpFeedback { get; private set; }

		/// <summary>
		/// Gets or sets the UsageTracker
		/// </summary>
		public UsageTracking UsageTracker { get; set; }

		/// <summary>
		/// Gets or sets the WarmupTime
		/// </summary>
		public uint WarmupTime { get; set; }

		/// <summary>
		/// Gets or sets the CooldownTime
		/// </summary>
		public uint CooldownTime { get; set; }

		/// <summary>
		/// Abstract function that must be implemented by derived classes to provide the cooling down feedback value.
		/// Must be implemented by concrete sub-classes.
		/// </summary>
		abstract protected Func<bool> IsCoolingDownFeedbackFunc { get; }

		/// <summary>
		/// Abstract function that must be implemented by derived classes to provide the warming up feedback value.
		/// Must be implemented by concrete sub-classes.
		/// </summary>
		abstract protected Func<bool> IsWarmingUpFeedbackFunc { get; }

		/// <summary>
		/// Timer used for managing display warmup timing.
		/// </summary>
		protected CTimer WarmupTimer;

		/// <summary>
		/// Timer used for managing display cooldown timing.
		/// </summary>
		protected CTimer CooldownTimer;

		#region IRoutingInputs Members

		/// <summary>
		/// Gets the collection of input ports available on this display device.
		/// </summary>
		public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

		#endregion

		/// <summary>
		/// Initializes a new instance of the DisplayBase class.
		/// </summary>
		/// <param name="key">The unique key identifier for this display device.</param>
		/// <param name="name">The friendly name for this display device.</param>
		protected DisplayBase(string key, string name)
		: base(key, name)
		{
			IsCoolingDownFeedback = new BoolFeedback("IsCoolingDown", IsCoolingDownFeedbackFunc);
			IsWarmingUpFeedback = new BoolFeedback("IsWarmingUp", IsWarmingUpFeedbackFunc);

			Feedbacks.Add(IsCoolingDownFeedback);
			Feedbacks.Add(IsWarmingUpFeedback);

			InputPorts = new RoutingPortCollection<RoutingInputPort>();

			CurrentSources = new Dictionary<eRoutingSignalType, SourceListItem>
			{
				{ eRoutingSignalType.Audio, null },
				{ eRoutingSignalType.Video, null },
			};

			CurrentSourceKeys = new Dictionary<eRoutingSignalType, string>
			{
				{ eRoutingSignalType.Audio, string.Empty },
				{ eRoutingSignalType.Video, string.Empty },
			};
		}

		/// <summary>
		/// Powers on the display device. Must be implemented by derived classes.
		/// </summary>
		public abstract void PowerOn();

		/// <summary>
		/// Powers off the display device. Must be implemented by derived classes.
		/// </summary>
		public abstract void PowerOff();

		/// <summary>
		/// Toggles the power state of the display device. Must be implemented by derived classes.
		/// </summary>
		public abstract void PowerToggle();

		/// <summary>
		/// Gets the collection of feedback objects for this display device.
		/// </summary>
		/// <inheritdoc />
		public virtual FeedbackCollection<Feedback> Feedbacks { get; private set; } = new FeedbackCollection<Feedback>();


		/// <summary>
		/// Executes a switch to the specified input on the display device. Must be implemented by derived classes.
		/// </summary>
		/// <param name="selector">The selector object that identifies which input to switch to.</param>
		public abstract void ExecuteSwitch(object selector);

		/// <summary>
		/// Links the display device to an API using a trilist, join start, join map key, and bridge.
		/// This overload uses serialized join map configuration.
		/// </summary>
		/// <param name="displayDevice">The display device to link.</param>
		/// <param name="trilist">The BasicTriList for communication.</param>
		/// <param name="joinStart">The starting join number for the device.</param>
		/// <param name="joinMapKey">The key for the join map configuration.</param>
		/// <param name="bridge">The EISC API bridge instance.</param>
		protected void LinkDisplayToApi(DisplayBase displayDevice, BasicTriList trilist, uint joinStart, string joinMapKey,
				EiscApiAdvanced bridge)
		{
			var joinMap = new DisplayControllerJoinMap(joinStart);

			var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

			if (!string.IsNullOrEmpty(joinMapSerialized))
				joinMap = JsonConvert.DeserializeObject<DisplayControllerJoinMap>(joinMapSerialized);

			if (bridge != null)
			{
				bridge.AddJoinMap(Key, joinMap);
			}
			else
			{
				Debug.LogMessage(LogEventLevel.Information, this, "Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
			}

			LinkDisplayToApi(displayDevice, trilist, joinMap);
		}

		/// <summary>
		/// Links the display device to an API using a trilist and join map.
		/// This overload uses a pre-configured join map instance.
		/// </summary>
		/// <param name="displayDevice">The display device to link.</param>
		/// <param name="trilist">The BasicTriList for communication.</param>
		/// <param name="joinMap">The join map configuration for the device.</param>
		protected void LinkDisplayToApi(DisplayBase displayDevice, BasicTriList trilist, DisplayControllerJoinMap joinMap)
		{
			this.LogDebug("Linking to Trilist {ipId}", trilist.ID.ToString("X"));
			this.LogDebug("Linking to Display: {displayName}", displayDevice.Name);

			trilist.StringInput[joinMap.Name.JoinNumber].StringValue = displayDevice.Name;

			if (displayDevice is ICommunicationMonitor commMonitor)
			{
				commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
			}

			// TODO: revisit this as there could be issues with this approach
			var inputNumber = 0;
			var inputKeys = new List<string>();

			var inputNumberFeedback = new IntFeedback("inputNumber", () => inputNumber);

			// Add input number feedback to the device feedback collection to keep it around...
			Feedbacks.Add(inputNumberFeedback);

			// Two way feedbacks			
			if (displayDevice is TwoWayDisplayBase twoWayDisplay)
			{
				trilist.SetBool(joinMap.IsTwoWayDisplay.JoinNumber, true);

				twoWayDisplay.CurrentInputFeedback.OutputChange += (o, a) => this.LogDebug("CurrentInputFeedback_OutputChange {input}", a.StringValue);

				inputNumberFeedback.LinkInputSig(trilist.UShortInput[joinMap.InputSelect.JoinNumber]);

				twoWayDisplay.PowerIsOnFeedback.OutputChange += (o, a) =>
				{
					if (!a.BoolValue)
					{
						inputNumber = 102;
						inputNumberFeedback.FireUpdate();

					}
					else
					{
						inputNumber = 0;
						inputNumberFeedback.FireUpdate();
					}
				};

				twoWayDisplay.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PowerOff.JoinNumber]);
				twoWayDisplay.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn.JoinNumber]);
			}

			// Power Off
			trilist.SetSigTrueAction(joinMap.PowerOff.JoinNumber, () =>
			{
				inputNumber = 102;
				inputNumberFeedback.FireUpdate();
				displayDevice.PowerOff();
			});

			// PowerOn
			trilist.SetSigTrueAction(joinMap.PowerOn.JoinNumber, () =>
			{
				inputNumber = 0;
				inputNumberFeedback.FireUpdate();
				displayDevice.PowerOn();
			});


			for (int i = 0; i < displayDevice.InputPorts.Count; i++)
			{
				var localindex = i;
				if (localindex >= joinMap.InputNamesOffset.JoinSpan)
				{
					this.LogWarning("Device has {inputCount} inputs.  The Join Map allows up to {joinSpan} inputs.  Discarding inputs {discardStart} - {discardEnd} from bridge.",
						displayDevice.InputPorts.Count, joinMap.InputNamesOffset.JoinSpan, localindex + 1, displayDevice.InputPorts.Count);

					continue;
				}
				else
				{
					inputKeys.Add(displayDevice.InputPorts[localindex].Key);

					var tempKey = inputKeys.ElementAt(localindex);

					trilist.SetSigTrueAction((ushort)(joinMap.InputSelectOffset.JoinNumber + localindex), () => displayDevice.ExecuteSwitch(displayDevice.InputPorts[tempKey].Selector));

					this.LogDebug("Setting Input Select Action on Digital Join {joinNumber} to Input: {input}", joinMap.InputSelectOffset.JoinNumber + localindex, displayDevice.InputPorts[tempKey].Key);

					trilist.SetString((uint)(joinMap.InputNamesOffset.JoinNumber + localindex), displayDevice.InputPorts[localindex].Key);
				}
			}

			this.LogDebug("Setting Input Select Action on Analog Join {inputSelectJoin}", joinMap.InputSelect);

			trilist.SetUShortSigAction(joinMap.InputSelect.JoinNumber, (requestedInput) =>
			{
				if (requestedInput == 0)
				{
					displayDevice.PowerOff();
					inputNumber = 0;
					return;
				}

				// using 1-based indexing for inputs coming from SIMPL, so need to check if the input is <= the count, not <
				if (requestedInput > 0 && requestedInput <= displayDevice.InputPorts.Count && requestedInput != inputNumber)
				{
					displayDevice.ExecuteSwitch(displayDevice.InputPorts.ElementAt(requestedInput - 1).Selector);

					inputNumber = requestedInput;

					return;
				}

				if (requestedInput == 102)
				{
					displayDevice.PowerToggle();
					return;
				}

				if (displayDevice is TwoWayDisplayBase)
				{
					inputNumberFeedback?.FireUpdate();
				}
			});


			var volumeDisplay = displayDevice as IBasicVolumeControls;
			if (volumeDisplay == null) return;

			trilist.SetBoolSigAction(joinMap.VolumeUp.JoinNumber, volumeDisplay.VolumeUp);
			trilist.SetBoolSigAction(joinMap.VolumeDown.JoinNumber, volumeDisplay.VolumeDown);
			trilist.SetSigTrueAction(joinMap.VolumeMute.JoinNumber, volumeDisplay.MuteToggle);

			var volumeDisplayWithFeedback = volumeDisplay as IBasicVolumeWithFeedback;

			if (volumeDisplayWithFeedback == null) return;
			trilist.SetSigTrueAction(joinMap.VolumeMuteOn.JoinNumber, volumeDisplayWithFeedback.MuteOn);
			trilist.SetSigTrueAction(joinMap.VolumeMuteOff.JoinNumber, volumeDisplayWithFeedback.MuteOff);


			trilist.SetUShortSigAction(joinMap.VolumeLevel.JoinNumber, volumeDisplayWithFeedback.SetVolume);
			volumeDisplayWithFeedback.VolumeLevelFeedback.LinkInputSig(trilist.UShortInput[joinMap.VolumeLevel.JoinNumber]);
			volumeDisplayWithFeedback.MuteFeedback.LinkInputSig(trilist.BooleanInput[joinMap.VolumeMute.JoinNumber]);
			volumeDisplayWithFeedback.MuteFeedback.LinkInputSig(trilist.BooleanInput[joinMap.VolumeMuteOn.JoinNumber]);
			volumeDisplayWithFeedback.MuteFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.VolumeMuteOff.JoinNumber]);
		}

		/// <inheritdoc />
		public virtual void SetCurrentSource(eRoutingSignalType signalType, string sourceListKey, SourceListItem sourceListItem)
		{
			foreach (eRoutingSignalType type in Enum.GetValues(typeof(eRoutingSignalType)))
			{
				var flagValue = Convert.ToInt32(type);
				// Skip if flagValue is 0 or not a power of two (i.e., not a single-bit flag).
				// (flagValue & (flagValue - 1)) != 0 checks if more than one bit is set.
				if (flagValue == 0 || (flagValue & (flagValue - 1)) != 0)
				{
					this.LogDebug("Skipping {type}", type);
					continue;
				}

				this.LogDebug("setting {type}", type);

				if (signalType.HasFlag(type))
				{
					UpdateCurrentSources(type, sourceListKey, sourceListItem);
				}
			}
			// Raise the CurrentSourcesChanged event
			CurrentSourcesChanged?.Invoke(this, EventArgs.Empty);
		}

		private void UpdateCurrentSources(eRoutingSignalType signalType, string sourceListKey, SourceListItem sourceListItem)
		{
			if (CurrentSources.ContainsKey(signalType))
			{
				CurrentSources[signalType] = sourceListItem;
			}
			else
			{
				CurrentSources.Add(signalType, sourceListItem);
			}

			// Update the current source key for the specified signal type
			if (CurrentSourceKeys.ContainsKey(signalType))
			{
				CurrentSourceKeys[signalType] = sourceListKey;
			}
			else
			{
				CurrentSourceKeys.Add(signalType, sourceListKey);
			}
		}

	}
}