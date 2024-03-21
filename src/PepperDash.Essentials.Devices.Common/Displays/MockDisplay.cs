using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Routing;
using Serilog.Events;

namespace PepperDash.Essentials.Devices.Common.Displays
{
    public class MockDisplay : TwoWayDisplayBase, IBasicVolumeWithFeedback, IBridgeAdvanced, IHasInputs<string, string>
	{
        public ISelectableItems<string> Inputs { get; private set; }

		bool _PowerIsOn;
		bool _IsWarmingUp;
		bool _IsCoolingDown;

        protected override Func<bool> PowerIsOnFeedbackFunc
        {
            get
            {
                return () =>
                    {
                        Debug.LogMessage(LogEventLevel.Verbose, this, "*************************************************** Display Power is {0}", _PowerIsOn ? "on" : "off");
                        return _PowerIsOn;
                    };
        } }
		protected override Func<bool> IsCoolingDownFeedbackFunc
        {
            get
            {
                return () =>
                {
                    Debug.LogMessage(LogEventLevel.Verbose, this, "*************************************************** {0}", _IsCoolingDown ? "Display is cooling down" : "Display has finished cooling down");
                    return _IsCoolingDown;
                };
            }
        }
		protected override Func<bool> IsWarmingUpFeedbackFunc
        {
            get
            {
                return () =>
                {
                    Debug.LogMessage(LogEventLevel.Verbose, this, "*************************************************** {0}", _IsWarmingUp ? "Display is warming up" : "Display has finished warming up");
                    return _IsWarmingUp;
                };
            }
        }
        protected override Func<string> CurrentInputFeedbackFunc { get { return () => Inputs.CurrentItem; } }

        int VolumeHeldRepeatInterval = 200;
        ushort VolumeInterval = 655;
		ushort _FakeVolumeLevel = 31768;
		bool _IsMuted;

		public MockDisplay(string key, string name)
			: base(key, name)
		{
            Inputs = new MockDisplayInputs
            {
                Items = new Dictionary<string, ISelectableItem>
				{
					{ "HDMI1", new MockDisplayInput ( "HDMI1", "HDMI 1",this ) },
					{ "HDMI2", new MockDisplayInput ("HDMI2", "HDMI 2",this ) },
					{ "HDMI3", new MockDisplayInput ("HDMI3", "HDMI 3",this ) },
					{ "HDMI4", new MockDisplayInput ("HDMI4", "HDMI 4",this )},
					{ "DP", new MockDisplayInput ("DP", "DisplayPort", this ) }
				}
            };

			Inputs.CurrentItemChanged += (o, a) => CurrentInputFeedback.FireUpdate();
           
            var hdmiIn1 = new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.AudioVideo,
				eRoutingPortConnectionType.Hdmi, "HDMI1", this);
			var hdmiIn2 = new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.AudioVideo,
				eRoutingPortConnectionType.Hdmi, "HDMI2", this);
			var hdmiIn3 = new RoutingInputPort(RoutingPortNames.HdmiIn3, eRoutingSignalType.AudioVideo,
				eRoutingPortConnectionType.Hdmi, "HDMI3", this);
			var hdmiIn4 = new RoutingInputPort(RoutingPortNames.ComponentIn, eRoutingSignalType.AudioVideo,
				eRoutingPortConnectionType.Hdmi, "HDMI4", this);
			var dpIn = new RoutingInputPort(RoutingPortNames.DisplayPortIn, eRoutingSignalType.AudioVideo,
				eRoutingPortConnectionType.DisplayPort, "DP", this);
			InputPorts.AddRange(new[] { hdmiIn1, hdmiIn2, hdmiIn3, hdmiIn4, dpIn });

			VolumeLevelFeedback = new IntFeedback(() => { return _FakeVolumeLevel; });
			MuteFeedback = new BoolFeedback("MuteOn", () => _IsMuted);

            WarmupTime = 10000;
            CooldownTime = 10000;
		}

		public override void PowerOn()
		{
			if (!PowerIsOnFeedback.BoolValue && !_IsWarmingUp && !_IsCoolingDown)
			{
				_IsWarmingUp = true;
				IsWarmingUpFeedback.InvokeFireUpdate();
				// Fake power-up cycle
				WarmupTimer = new CTimer(o =>
					{
						_IsWarmingUp = false;
						_PowerIsOn = true;
						IsWarmingUpFeedback.InvokeFireUpdate();
						PowerIsOnFeedback.InvokeFireUpdate();
					}, WarmupTime);
			}
		}

		public override void PowerOff()
		{
			// If a display has unreliable-power off feedback, just override this and
			// remove this check.
			if (PowerIsOnFeedback.BoolValue && !_IsWarmingUp && !_IsCoolingDown)
			{
				_IsCoolingDown = true;
				IsCoolingDownFeedback.InvokeFireUpdate();
				// Fake cool-down cycle
				CooldownTimer = new CTimer(o =>
					{
						Debug.LogMessage(LogEventLevel.Verbose, this, "Cooldown timer ending");
						_IsCoolingDown = false;
						IsCoolingDownFeedback.InvokeFireUpdate();
                        _PowerIsOn = false;
                        PowerIsOnFeedback.InvokeFireUpdate();
					}, CooldownTime);
			}
		}		
		
		public override void PowerToggle()
		{
			if (PowerIsOnFeedback.BoolValue && !IsWarmingUpFeedback.BoolValue)
				PowerOff();
			else if (!PowerIsOnFeedback.BoolValue && !IsCoolingDownFeedback.BoolValue)
				PowerOn();
		}

		public override void ExecuteSwitch(object selector)
		{
			Debug.LogMessage(LogEventLevel.Verbose, this, "ExecuteSwitch: {0}", selector);

		    if (!_PowerIsOn)
		    {
		        PowerOn();
		    }

			if (!Inputs.Items.TryGetValue(selector.ToString(), out var input))
				return;

			input.Select();
		}

        public void SetInput(string selector)
        {
			ISelectableItem currentInput = null;

			try
			{
				currentInput = Inputs.Items.SingleOrDefault(Inputs => Inputs.Value.IsSelected).Value;
			}
			catch { }
			

            if (currentInput != null)
            {
                Debug.LogMessage(LogEventLevel.Verbose, this, "SetInput: {0}", selector);
                currentInput.IsSelected = false;
            }

			if (!Inputs.Items.TryGetValue(selector, out var input))
                return;

			input.IsSelected = true;

			Inputs.CurrentItem = selector;
        }



        #region IBasicVolumeWithFeedback Members

        public IntFeedback VolumeLevelFeedback { get; private set; }

		public void SetVolume(ushort level)
		{
			_FakeVolumeLevel = level;
			VolumeLevelFeedback.InvokeFireUpdate();
		}

		public void MuteOn()
		{
			_IsMuted = true;
			MuteFeedback.InvokeFireUpdate();
		}

		public void MuteOff()
		{
			_IsMuted = false;
			MuteFeedback.InvokeFireUpdate();
		}

		public BoolFeedback MuteFeedback { get; private set; }


        #endregion

        #region IBasicVolumeControls Members

        public void VolumeUp(bool pressRelease)
		{
            //while (pressRelease)
            //{
                Debug.LogMessage(LogEventLevel.Verbose, this, "Volume Down {0}", pressRelease);
                if (pressRelease)
                {
                    var newLevel = _FakeVolumeLevel + VolumeInterval;
                    SetVolume((ushort)newLevel);
                    CrestronEnvironment.Sleep(VolumeHeldRepeatInterval);
                }
            //}
		}

		public void VolumeDown(bool pressRelease)
		{
            //while (pressRelease)
            //{
                Debug.LogMessage(LogEventLevel.Verbose, this, "Volume Up {0}", pressRelease);
                if (pressRelease)
                {
                    var newLevel = _FakeVolumeLevel - VolumeInterval;
                    SetVolume((ushort)newLevel);
                    CrestronEnvironment.Sleep(VolumeHeldRepeatInterval);
                }
            //}
		}

		public void MuteToggle()
		{
			_IsMuted = !_IsMuted;
			MuteFeedback.InvokeFireUpdate();
		}

		#endregion

	    public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
	    {
	        LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
	    }


    }
}