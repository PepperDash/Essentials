using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core.Bridges;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
    public abstract class DisplayBase : EssentialsDevice, IHasFeedback, IRoutingSinkWithSwitching, IHasPowerControl, IWarmingCooling, IUsageTracking
	{
        public event SourceInfoChangeHandler CurrentSourceChange;

        public string CurrentSourceInfoKey { get; set; }
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

		public BoolFeedback IsCoolingDownFeedback { get; protected set; }
		public BoolFeedback IsWarmingUpFeedback { get; private set; }

        public UsageTracking UsageTracker { get; set; }

		public uint WarmupTime { get; set; }
		public uint CooldownTime { get; set; }

		/// <summary>
		/// Bool Func that will provide a value for the PowerIsOn Output. Must be implemented
		/// by concrete sub-classes
		/// </summary>
		abstract protected Func<bool> IsCoolingDownFeedbackFunc { get; }
		abstract protected Func<bool> IsWarmingUpFeedbackFunc { get; }
        

		protected CTimer WarmupTimer;
		protected CTimer CooldownTimer;

		#region IRoutingInputs Members

		public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }

		#endregion

	    protected DisplayBase(string key, string name)
			: base(key, name)
		{
			IsCoolingDownFeedback = new BoolFeedback("IsCoolingDown", IsCoolingDownFeedbackFunc);
			IsWarmingUpFeedback = new BoolFeedback("IsWarmingUp", IsWarmingUpFeedbackFunc);

			InputPorts = new RoutingPortCollection<RoutingInputPort>();

		}

		public abstract void PowerOn();
		public abstract void PowerOff();
		public abstract void PowerToggle();

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

	    public abstract void ExecuteSwitch(object selector);

	    protected void LinkDisplayToApi(DisplayBase displayDevice, BasicTriList trilist, uint joinStart, string joinMapKey,
	        EiscApiAdvanced bridge)
	    {
            var inputNumber = 0;
	        var inputKeys = new List<string>();

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
	            Debug.Console(0,this,"Please update config to use 'eiscapiadvanced' to get all join map features for this device.");
	        }

	        Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to Display: {0}", displayDevice.Name);

            trilist.StringInput[joinMap.Name.JoinNumber].StringValue = displayDevice.Name;

            var commMonitor = displayDevice as ICommunicationMonitor;
            if (commMonitor != null)
            {
                commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
            }

            var inputNumberFeedback = new IntFeedback(() => inputNumber);

            // Two way feedbacks
            var twoWayDisplay = displayDevice as TwoWayDisplayBase;

            if (twoWayDisplay != null)
            {
                trilist.SetBool(joinMap.IsTwoWayDisplay.JoinNumber, true);

                twoWayDisplay.CurrentInputFeedback.OutputChange += (o, a) => Debug.Console(0, "CurrentInputFeedback_OutputChange {0}", a.StringValue);


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
                    Debug.Console(2, displayDevice, "Setting Input Select Action on Digital Join {0} to Input: {1}",
                        joinMap.InputSelectOffset.JoinNumber + i, displayDevice.InputPorts[tempKey].Key.ToString());
                    trilist.StringInput[(ushort)(joinMap.InputNamesOffset.JoinNumber + i)].StringValue = displayDevice.InputPorts[i].Key.ToString();
                }
                else
                    Debug.Console(0, displayDevice, Debug.ErrorLogLevel.Warning, "Device has {0} inputs.  The Join Map allows up to {1} inputs.  Discarding inputs {2} - {3} from bridge.",
                        displayDevice.InputPorts.Count, joinMap.InputNamesOffset.JoinSpan, i + 1, displayDevice.InputPorts.Count);
            }

            Debug.Console(2, displayDevice, "Setting Input Select Action on Analog Join {0}", joinMap.InputSelect);
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
	/// 
	/// </summary>
    public abstract class TwoWayDisplayBase : DisplayBase, IRoutingFeedback, IHasPowerControlWithFeedback
	{
        public StringFeedback CurrentInputFeedback { get; private set; }

        abstract protected Func<string> CurrentInputFeedbackFunc { get; }

        public BoolFeedback PowerIsOnFeedback { get; protected set; }
        abstract protected Func<bool> PowerIsOnFeedbackFunc { get; }


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