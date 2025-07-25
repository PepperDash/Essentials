using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
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
	public abstract class DisplayBase : EssentialsDevice, IDisplay, ICurrentSources
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

				if (handler != null)
					handler(_CurrentSourceInfo, ChangeType.WillChange);

				_CurrentSourceInfo = value;

				if (handler != null)
					handler(_CurrentSourceInfo, ChangeType.DidChange);
			}
		}
		SourceListItem _CurrentSourceInfo;

		/// <inheritdoc/> 
		public Dictionary<eRoutingSignalType, SourceListItem> CurrentSources { get; private set; }

		/// <inheritdoc/>
		public Dictionary<eRoutingSignalType, string> CurrentSourceKeys { get; private set; }

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
		public virtual FeedbackCollection<Feedback> Feedbacks
		{
			get
			{
				return new FeedbackCollection<Feedback>
				{
					IsCoolingDownFeedback,
					IsWarmingUpFeedback
				};
			}
		}

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
			Debug.LogMessage(LogEventLevel.Debug, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			Debug.LogMessage(LogEventLevel.Information, "Linking to Display: {0}", displayDevice.Name);

			trilist.StringInput[joinMap.Name.JoinNumber].StringValue = displayDevice.Name;

			var commMonitor = displayDevice as ICommunicationMonitor;
			if (commMonitor != null)
			{
				commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
			}

			var inputNumber = 0;
			var inputKeys = new List<string>();

			var inputNumberFeedback = new IntFeedback(() => inputNumber);

			// Two way feedbacks
			var twoWayDisplay = displayDevice as TwoWayDisplayBase;

			if (twoWayDisplay != null)
			{
				trilist.SetBool(joinMap.IsTwoWayDisplay.JoinNumber, true);

				twoWayDisplay.CurrentInputFeedback.OutputChange += (o, a) => Debug.LogMessage(LogEventLevel.Information, "CurrentInputFeedback_OutputChange {0}", a.StringValue);


				inputNumberFeedback.LinkInputSig(trilist.UShortInput[joinMap.InputSelect.JoinNumber]);
			}

			// Power Off
			trilist.SetSigTrueAction(joinMap.PowerOff.JoinNumber, () =>
			{
				inputNumber = 102;
				inputNumberFeedback.FireUpdate();
				displayDevice.PowerOff();
			});

			var twoWayDisplayDevice = displayDevice as TwoWayDisplayBase;
			if (twoWayDisplayDevice != null)
			{
				twoWayDisplayDevice.PowerIsOnFeedback.OutputChange += (o, a) =>
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

				twoWayDisplayDevice.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PowerOff.JoinNumber]);
				twoWayDisplayDevice.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn.JoinNumber]);
			}

			// PowerOn
			trilist.SetSigTrueAction(joinMap.PowerOn.JoinNumber, () =>
			{
				inputNumber = 0;
				inputNumberFeedback.FireUpdate();
				displayDevice.PowerOn();
			});



			for (int i = 0; i < displayDevice.InputPorts.Count; i++)
			{
				if (i < joinMap.InputNamesOffset.JoinSpan)
				{
					inputKeys.Add(displayDevice.InputPorts[i].Key);
					var tempKey = inputKeys.ElementAt(i);
					trilist.SetSigTrueAction((ushort)(joinMap.InputSelectOffset.JoinNumber + i),
						() => displayDevice.ExecuteSwitch(displayDevice.InputPorts[tempKey].Selector));
					Debug.LogMessage(LogEventLevel.Verbose, displayDevice, "Setting Input Select Action on Digital Join {0} to Input: {1}",
						joinMap.InputSelectOffset.JoinNumber + i, displayDevice.InputPorts[tempKey].Key.ToString());
					trilist.StringInput[(ushort)(joinMap.InputNamesOffset.JoinNumber + i)].StringValue = displayDevice.InputPorts[i].Key.ToString();
				}
				else
					Debug.LogMessage(LogEventLevel.Information, displayDevice, "Device has {0} inputs.  The Join Map allows up to {1} inputs.  Discarding inputs {2} - {3} from bridge.",
						displayDevice.InputPorts.Count, joinMap.InputNamesOffset.JoinSpan, i + 1, displayDevice.InputPorts.Count);
			}

			Debug.LogMessage(LogEventLevel.Verbose, displayDevice, "Setting Input Select Action on Analog Join {0}", joinMap.InputSelect);
			trilist.SetUShortSigAction(joinMap.InputSelect.JoinNumber, (a) =>
			{
				if (a == 0)
				{
					displayDevice.PowerOff();
					inputNumber = 0;
				}
				else if (a > 0 && a < displayDevice.InputPorts.Count && a != inputNumber)
				{
					displayDevice.ExecuteSwitch(displayDevice.InputPorts.ElementAt(a - 1).Selector);
					inputNumber = a;
				}
				else if (a == 102)
				{
					displayDevice.PowerToggle();

				}
				if (twoWayDisplay != null)
					inputNumberFeedback.FireUpdate();
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

	}

	/// <summary>
	/// Abstract base class for two-way display devices that provide feedback capabilities.
	/// Extends DisplayBase with routing feedback and power control feedback functionality.
	/// </summary>
	public abstract class TwoWayDisplayBase : DisplayBase, IRoutingFeedback, IHasPowerControlWithFeedback
	{
		/// <summary>
		/// Gets feedback for the current input selection on the display.
		/// </summary>
		public StringFeedback CurrentInputFeedback { get; private set; }

		/// <summary>
		/// Abstract function that must be implemented by derived classes to provide the current input feedback value.
		/// Must be implemented by concrete sub-classes.
		/// </summary>
		abstract protected Func<string> CurrentInputFeedbackFunc { get; }

		/// <summary>
		/// Gets feedback indicating whether the display is currently powered on.
		/// </summary>
		public BoolFeedback PowerIsOnFeedback { get; protected set; }

		/// <summary>
		/// Abstract function that must be implemented by derived classes to provide the power state feedback value.
		/// Must be implemented by concrete sub-classes.
		/// </summary>
		abstract protected Func<bool> PowerIsOnFeedbackFunc { get; }

		/// <summary>
		/// Gets the default mock display instance for testing and development purposes.
		/// </summary>
		public static MockDisplay DefaultDisplay
		{
			get
			{
				if (_DefaultDisplay == null)
					_DefaultDisplay = new MockDisplay("default", "Default Display");
				return _DefaultDisplay;
			}
		}
		static MockDisplay _DefaultDisplay;

		/// <summary>
		/// Initializes a new instance of the TwoWayDisplayBase class.
		/// </summary>
		/// <param name="key">The unique key identifier for this display device.</param>
		/// <param name="name">The friendly name for this display device.</param>
		public TwoWayDisplayBase(string key, string name)
			: base(key, name)
		{
			CurrentInputFeedback = new StringFeedback(CurrentInputFeedbackFunc);

			WarmupTime = 7000;
			CooldownTime = 15000;

			PowerIsOnFeedback = new BoolFeedback("PowerOnFeedback", PowerIsOnFeedbackFunc);

			Feedbacks.Add(CurrentInputFeedback);
			Feedbacks.Add(PowerIsOnFeedback);

			PowerIsOnFeedback.OutputChange += PowerIsOnFeedback_OutputChange;

		}

		void PowerIsOnFeedback_OutputChange(object sender, EventArgs e)
		{
			if (UsageTracker != null)
			{
				if (PowerIsOnFeedback.BoolValue)
					UsageTracker.StartDeviceUsage();
				else
					UsageTracker.EndDeviceUsage();
			}
		}

		/// <summary>
		/// Event that is raised when a numeric switch change occurs on the display.
		/// </summary>
		public event EventHandler<RoutingNumericEventArgs> NumericSwitchChange;

		/// <summary>
		/// Raise an event when the status of a switch object changes.
		/// </summary>
		/// <param name="e">Arguments defined as IKeyName sender, output, input, and eRoutingSignalType</param>
		protected void OnSwitchChange(RoutingNumericEventArgs e)
		{
			var newEvent = NumericSwitchChange;
			if (newEvent != null) newEvent(this, e);
		}
	}
}