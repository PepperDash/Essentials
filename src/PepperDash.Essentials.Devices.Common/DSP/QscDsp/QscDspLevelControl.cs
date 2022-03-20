using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using System.Text.RegularExpressions;


namespace PepperDash.Essentials.Devices.Common.DSP
{
	public class QscDspLevelControl : QscDspControlPoint, IBasicVolumeWithFeedback, IKeyed
    {
        bool _IsMuted;
        ushort _VolumeLevel;

        public BoolFeedback MuteFeedback { get; private set; }

        public IntFeedback VolumeLevelFeedback { get; private set; }

        public bool Enabled { get; set; }
		public ePdtLevelTypes Type;
		CTimer VolumeUpRepeatTimer;
		CTimer VolumeDownRepeatTimer;

        /// <summary>
        /// Used for to identify level subscription values
        /// </summary>
        public string LevelCustomName { get; private set; }

        /// <summary>
        /// Used for to identify mute subscription values
        /// </summary>
        public string MuteCustomName { get; private set; }

        /// <summary>
        /// Minimum fader level
        /// </summary>
        double MinLevel;

        /// <summary>
        /// Maximum fader level
        /// </summary>
        double MaxLevel;

        /// <summary>
        /// Checks if a valid subscription string has been recieved for all subscriptions
        /// </summary>
        public bool IsSubsribed
        {
            get
            {
                bool isSubscribed = false;

                if (HasMute && MuteIsSubscribed)
                    isSubscribed = true;

                if (HasLevel && LevelIsSubscribed)
                    isSubscribed = true;

                return isSubscribed;
            }
        }

        public bool AutomaticUnmuteOnVolumeUp { get; private set; }

        public bool HasMute { get; private set; }

        public bool HasLevel { get; private set; }

        bool MuteIsSubscribed;

        bool LevelIsSubscribed;

        //public TesiraForteLevelControl(string label, string id, int index1, int index2, bool hasMute, bool hasLevel, BiampTesiraForteDsp parent)
        //    : base(id, index1, index2, parent)
        //{
        //    Initialize(label, hasMute, hasLevel);
        //}

        public QscDspLevelControl(string key, QscDspLevelControlBlockConfig config, QscDsp parent)
            : base(config.LevelInstanceTag, config.MuteInstanceTag, parent)
        {
			if (!config.Disabled)
			{
				Initialize(key, config);
			}
        }


        /// <summary>
        /// Initializes this attribute based on config values and generates subscriptions commands and adds commands to the parent's queue.
        /// </summary>
        public void Initialize(string key, QscDspLevelControlBlockConfig config)
        {
            Key = string.Format("{0}--{1}", Parent.Key, key);
			Enabled = true;
            DeviceManager.AddDevice(this);
			if (config.IsMic)
			{
				Type = ePdtLevelTypes.microphone;
			}
			else
			{
				Type = ePdtLevelTypes.speaker;
			}
			
            Debug.Console(2, this, "Adding LevelControl '{0}'", Key);

            this.IsSubscribed = false;

            MuteFeedback = new BoolFeedback(() => _IsMuted);

            VolumeLevelFeedback = new IntFeedback(() => _VolumeLevel);

			VolumeUpRepeatTimer = new CTimer(VolumeUpRepeat, Timeout.Infinite); 
			VolumeDownRepeatTimer = new CTimer(VolumeDownRepeat, Timeout.Infinite);
			LevelCustomName = config.Label;
            HasMute = config.HasMute;
            HasLevel = config.HasLevel;
        }

        public void Subscribe()
        {
            // Do subscriptions and blah blah

            // Subscribe to mute
            if (this.HasMute)
            {

				SendSubscriptionCommand(this.MuteInstanceTag, "1");
                // SendSubscriptionCommand(config. , "mute", 500);
            }

            // Subscribe to level
            if (this.HasLevel)
            {

				SendSubscriptionCommand(this.LevelInstanceTag, "1");
                // SendSubscriptionCommand(this.con, "level", 250);

                //SendFullCommand("get", "minLevel", null);

                //SendFullCommand("get", "maxLevel", null);
            }
        }


        /// <summary>
        /// Parses the response from the DspBase
        /// </summary>
        /// <param name="customName"></param>
        /// <param name="value"></param>
        public void ParseSubscriptionMessage(string customName, string value)
        {

            // Check for valid subscription response
			Debug.Console(1, this, "Level {0} Response: '{1}'", customName, value);
            if (customName == MuteInstanceTag)
            {
                if (value == "muted")
                {
                    _IsMuted = true;
                    MuteIsSubscribed = true;

                }
                else if (value == "unmuted")
                {
                    _IsMuted = false;
                    MuteIsSubscribed = true;
                }

                MuteFeedback.FireUpdate();
            }
            else if (customName == LevelInstanceTag)
            {


                var _value = Double.Parse(value);

                _VolumeLevel = (ushort)(_value * 65535);
				Debug.Console(1, this, "Level {0} VolumeLevel: '{1}'", customName, _VolumeLevel);
                LevelIsSubscribed = true;

                VolumeLevelFeedback.FireUpdate();
            }
            
        }


        /// <summary>
        /// Turns the mute off
        /// </summary>
        public void MuteOff()
        {
			SendFullCommand("csv", this.MuteInstanceTag, "0");
        }

        /// <summary>
        /// Turns the mute on
        /// </summary>
        public void MuteOn()
        {
			SendFullCommand("csv", this.MuteInstanceTag, "1");
        }

        /// <summary>
        /// Sets the volume to a specified level
        /// </summary>
        /// <param name="level"></param>
        public void SetVolume(ushort level)
        {
            Debug.Console(1, this, "volume: {0}", level);
            // Unmute volume if new level is higher than existing
			if (AutomaticUnmuteOnVolumeUp && _IsMuted)
			{
				MuteOff();
			}
			double newLevel = Scale(level);
			Debug.Console(1, this, "newVolume: {0}", newLevel);
			SendFullCommand("csp", this.LevelInstanceTag, string.Format("{0}", newLevel));          
        }

        /// <summary>
        /// Toggles mute status
        /// </summary>
        public void MuteToggle()
        {

				if (_IsMuted)
				{
					SendFullCommand("csv", this.MuteInstanceTag, "0");
				}
				else
				{
					SendFullCommand("csv", this.MuteInstanceTag, "1");
				}
		
        }

		public void VolumeUpRepeat(object callbackObject)
		{
			this.VolumeUp(true);
		}
		public void VolumeDownRepeat(object callbackObject)
		{
			this.VolumeDown(true);
		}

        public void VolumeDown(bool press)
        {
			

			if (press)
			{
				VolumeDownRepeatTimer.Reset(100);
				SendFullCommand("css ", this.LevelInstanceTag, "--");
				
				
			}
			else
			{
				VolumeDownRepeatTimer.Stop();
				// VolumeDownRepeatTimer.Dispose();
			}
        }

        /// <summary>
        /// Increments volume level
        /// </summary>
        /// <param name="pressRelease"></param>
		public void VolumeUp(bool press)
		{
			if (press)
			{
				VolumeUpRepeatTimer.Reset(100);
				SendFullCommand("css ", this.LevelInstanceTag, "++");

				if (AutomaticUnmuteOnVolumeUp)
					if (!_IsMuted)
						MuteOff();
			}
			else
			{
				VolumeUpRepeatTimer.Stop();
			}
		}
        /// <returns></returns>
        double Scale(double input)
        {
            Debug.Console(1, this, "Scaling (double) input '{0}'",input );

            var output = (input / 65535);

            Debug.Console(1, this, "Scaled output '{0}'", output);

            return output;
        }
    }
	public enum ePdtLevelTypes
	{
		speaker = 0,
		microphone = 1
	}

}