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
	public abstract class DisplayBase : EssentialsBridgeableDevice, IHasFeedback, IRoutingSinkWithSwitching, IPower, IWarmingCooling, IUsageTracking
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

		public BoolFeedback PowerIsOnFeedback { get; protected set; }
		public BoolFeedback IsCoolingDownFeedback { get; protected set; }
		public BoolFeedback IsWarmingUpFeedback { get; private set; }

        public UsageTracking UsageTracker { get; set; }

		public uint WarmupTime { get; set; }
		public uint CooldownTime { get; set; }

		/// <summary>
		/// Bool Func that will provide a value for the PowerIsOn Output. Must be implemented
		/// by concrete sub-classes
		/// </summary>
		abstract protected Func<bool> PowerIsOnFeedbackFunc { get; }
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
			PowerIsOnFeedback = new BoolFeedback("PowerOnFeedback", PowerIsOnFeedbackFunc);
			IsCoolingDownFeedback = new BoolFeedback("IsCoolingDown", IsCoolingDownFeedbackFunc);
			IsWarmingUpFeedback = new BoolFeedback("IsWarmingUp", IsWarmingUpFeedbackFunc);

			InputPorts = new RoutingPortCollection<RoutingInputPort>();

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

		public abstract void PowerOn();
		public abstract void PowerOff();
		public abstract void PowerToggle();

        public virtual FeedbackCollection<Feedback> Feedbacks
		{
			get
			{
                return new FeedbackCollection<Feedback>
				{
					PowerIsOnFeedback,
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

            var joinMap = new DisplayControllerJoinMap();

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<DisplayControllerJoinMap>(joinMapSerialized);

            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to Display: {0}", displayDevice.Name);

            trilist.StringInput[joinMap.Name].StringValue = displayDevice.Name;

            var commMonitor = displayDevice as ICommunicationMonitor;
            if (commMonitor != null)
            {
                commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            }

            var inputNumberFeedback = new IntFeedback(() => inputNumber);

            // Two way feedbacks
            var twoWayDisplay = displayDevice as TwoWayDisplayBase;

            if (twoWayDisplay != null)
            {
                trilist.SetBool(joinMap.IsTwoWayDisplay, true);

                twoWayDisplay.CurrentInputFeedback.OutputChange += (o, a) => Debug.Console(0, "CurrentInputFeedback_OutputChange {0}", a.StringValue);


                inputNumberFeedback.LinkInputSig(trilist.UShortInput[joinMap.InputSelect]);
            }

            // Power Off
            trilist.SetSigTrueAction(joinMap.PowerOff, () =>
            {
                inputNumber = 102;
                inputNumberFeedback.FireUpdate();
                displayDevice.PowerOff();
            });

            displayDevice.PowerIsOnFeedback.OutputChange += (o, a) =>
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

            displayDevice.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PowerOff]);

            // PowerOn
            trilist.SetSigTrueAction(joinMap.PowerOn, () =>
            {
                inputNumber = 0;
                inputNumberFeedback.FireUpdate();
                displayDevice.PowerOn();
            });


            displayDevice.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn]);

            var count = 1;
            foreach (var input in displayDevice.InputPorts)
            {
                inputKeys.Add(input.Key);
                var tempKey = inputKeys.ElementAt(count - 1);
                trilist.SetSigTrueAction((ushort)(joinMap.InputSelectOffset + count), () => displayDevice.ExecuteSwitch(displayDevice.InputPorts[tempKey].Selector));
                Debug.Console(2, displayDevice, "Setting Input Select Action on Digital Join {0} to Input: {1}", joinMap.InputSelectOffset + count, displayDevice.InputPorts[tempKey].Key.ToString());
                trilist.StringInput[(ushort)(joinMap.InputNamesOffset + count)].StringValue = input.Key.ToString();
                count++;
            }

            Debug.Console(2, displayDevice, "Setting Input Select Action on Analog Join {0}", joinMap.InputSelect);
            trilist.SetUShortSigAction(joinMap.InputSelect, (a) =>
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

	        trilist.SetBoolSigAction(joinMap.VolumeUp, volumeDisplay.VolumeUp);
	        trilist.SetBoolSigAction(joinMap.VolumeDown, volumeDisplay.VolumeDown);
	        trilist.SetSigTrueAction(joinMap.VolumeMute, volumeDisplay.MuteToggle);

	        var volumeDisplayWithFeedback = volumeDisplay as IBasicVolumeWithFeedback;

	        if (volumeDisplayWithFeedback == null) return;

	        trilist.SetUShortSigAction(joinMap.VolumeLevel, volumeDisplayWithFeedback.SetVolume);
	        volumeDisplayWithFeedback.VolumeLevelFeedback.LinkInputSig(trilist.UShortInput[joinMap.VolumeLevel]);
	        volumeDisplayWithFeedback.MuteFeedback.LinkInputSig(trilist.BooleanInput[joinMap.VolumeMute]);
	    }
	}

	/// <summary>
	/// 
	/// </summary>
    public abstract class TwoWayDisplayBase : DisplayBase
	{
        public StringFeedback CurrentInputFeedback { get; private set; }

        abstract protected Func<string> CurrentInputFeedbackFunc { get; }


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

            Feedbacks.Add(CurrentInputFeedback);

            
		}

	}
}