using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

using PepperDash.Core;
using PepperDash.Essentials.Core.Routing;


namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public class MockDisplay : TwoWayDisplayBase, IBasicVolumeWithFeedback

	{
		public RoutingInputPort HdmiIn1 { get; private set; }
		public RoutingInputPort HdmiIn2 { get; private set; }
		public RoutingInputPort HdmiIn3 { get; private set; }
		public RoutingInputPort ComponentIn1 { get; private set; }
		public RoutingInputPort VgaIn1 { get; private set; }

		bool _PowerIsOn;
		bool _IsWarmingUp;
		bool _IsCoolingDown;

		protected override Func<bool> PowerIsOnFeedbackFunc { get { return () => _PowerIsOn; } }
		protected override Func<bool> IsCoolingDownFeedbackFunc { get { return () => _IsCoolingDown; } }
		protected override Func<bool> IsWarmingUpFeedbackFunc { get { return () => _IsWarmingUp; } }

        int VolumeHeldRepeatInterval = 200;
        ushort VolumeInterval = 655;
		ushort _FakeVolumeLevel = 31768;
		bool _IsMuted;

		public MockDisplay(string key, string name)
			: base(key, name)
		{
			HdmiIn1 = new RoutingInputPort(RoutingPortNames.HdmiIn1, eRoutingSignalType.AudioVideo,
				eRoutingPortConnectionType.Hdmi, null, this);
			HdmiIn2 = new RoutingInputPort(RoutingPortNames.HdmiIn2, eRoutingSignalType.AudioVideo,
				eRoutingPortConnectionType.Hdmi, null, this);
			HdmiIn3 = new RoutingInputPort(RoutingPortNames.HdmiIn3, eRoutingSignalType.AudioVideo,
				eRoutingPortConnectionType.Hdmi, null, this);
			ComponentIn1 = new RoutingInputPort(RoutingPortNames.ComponentIn, eRoutingSignalType.Video,
				eRoutingPortConnectionType.Component, null, this);
			VgaIn1 = new RoutingInputPort(RoutingPortNames.VgaIn, eRoutingSignalType.Video,
				eRoutingPortConnectionType.Composite, null, this);
			InputPorts.AddRange(new[] { HdmiIn1, HdmiIn2, HdmiIn3, ComponentIn1, VgaIn1 });

			VolumeLevelFeedback = new IntFeedback(() => { return _FakeVolumeLevel; });
			MuteFeedback = new BoolFeedback(CommonBoolCue.MuteOn, () => _IsMuted);
		}

		public override void PowerOn()
		{
			if (!PowerIsOnFeedback.BoolValue && !_IsWarmingUp && !_IsCoolingDown)
			{
				_IsWarmingUp = true;
				IsWarmingUpFeedback.FireUpdate();
				// Fake power-up cycle
				WarmupTimer = new CTimer(o =>
					{
						_IsWarmingUp = false;
						_PowerIsOn = true;
						IsWarmingUpFeedback.FireUpdate();
						PowerIsOnFeedback.FireUpdate();
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
				_PowerIsOn = false;
				PowerIsOnFeedback.FireUpdate();
				IsCoolingDownFeedback.FireUpdate();
				// Fake cool-down cycle
				CooldownTimer = new CTimer(o =>
					{
						Debug.Console(2, this, "Cooldown timer ending");
						_IsCoolingDown = false;
						IsCoolingDownFeedback.FireUpdate();
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
			Debug.Console(2, this, "ExecuteSwitch: {0}", selector);
		}



		#region IBasicVolumeWithFeedback Members

		public IntFeedback VolumeLevelFeedback { get; private set; }

		public void SetVolume(ushort level)
		{
			_FakeVolumeLevel = level; 
			VolumeLevelFeedback.FireUpdate();
		}

		public void MuteOn()
		{
			_IsMuted = true;
			MuteFeedback.FireUpdate();
		}

		public void MuteOff()
		{
			_IsMuted = false;
			MuteFeedback.FireUpdate();
		}

		public BoolFeedback MuteFeedback { get; private set; }

		#endregion

		#region IBasicVolumeControls Members

		public void VolumeUp(bool pressRelease)
		{
            //while (pressRelease)
            //{
                Debug.Console(2, this, "Volume Down {0}", pressRelease);
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
                Debug.Console(2, this, "Volume Up {0}", pressRelease);
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
			MuteFeedback.FireUpdate();
		}

		#endregion
	}
}