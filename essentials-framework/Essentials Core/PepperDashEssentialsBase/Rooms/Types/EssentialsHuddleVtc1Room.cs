using System;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices.AudioCodec;
using PepperDash.Essentials.Core.Devices.Codec;
using PepperDash.Essentials.Core.Devices.VideoCodec;
using PepperDash.Essentials.Core.Rooms;
using PepperDash.Essentials.Core.Rooms.Config;

namespace PepperDash.Essentials
{
    public class EssentialsHuddleVtc1Room : EssentialsRoomBase, IHasCurrentSourceInfoChange,
        IPrivacy, IHasCurrentVolumeControls, IRunRouteAction, IRunDefaultCallRoute, IHasVideoCodec, IHasAudioCodec,
        IHasDefaultDisplay, IHasInCallFeedback
    {

        /// <summary>
        /// "codecOsd"
        /// </summary>
        public const string DefaultCodecRouteString = "codecOsd";

        private EssentialsHuddleVtc1PropertiesConfig _propertiesConfig;

        public EssentialsHuddleVtc1Room(DeviceConfig config)
            : base(config)
        {
            try
            {
                _propertiesConfig = JsonConvert.DeserializeObject<EssentialsHuddleVtc1PropertiesConfig>
                    (config.Properties.ToString());
                DefaultDisplay =
                    DeviceManager.GetDeviceForKey(_propertiesConfig.DefaultDisplayKey) as IRoutingSinkWithSwitching;

                VideoCodec = DeviceManager.GetDeviceForKey(_propertiesConfig.VideoCodecKey) as
                    VideoCodecBase;
                if (VideoCodec == null)
                {
                    throw new ArgumentNullException("codec cannot be null");
                }

                AudioCodec = DeviceManager.GetDeviceForKey(_propertiesConfig.AudioCodecKey) as
                    AudioCodecBase;
                if (AudioCodec == null)
                {
                    Debug.Console(0, this, "No Audio Codec Found");
                }

                DefaultAudioDevice = DeviceManager.GetDeviceForKey(_propertiesConfig.DefaultAudioKey) as IRoutingSink;

                Initialize();
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error building room: \n{0}", e);
            }
        }

        /// <summary>
        /// Temporary implementation. Returns the schedule-ready object or null if none. Fow now,
        /// always returns the VideoCodec if it is capable
        /// </summary>
        public IHasScheduleAwareness ScheduleSource
        {
            get { return VideoCodec as IHasScheduleAwareness; }
        }

        #region IHasAudioCodec Members

        public AudioCodecBase AudioCodec { get; private set; }

        #endregion

        #region IHasVideoCodec Members

        public BoolFeedback InCallFeedback { get; private set; }


        /// <summary>
        /// States: 0 for on hook, 1 for video, 2 for audio, 3 for telekenesis
        /// </summary>
        public IntFeedback CallTypeFeedback { get; private set; }

        /// <summary>
        /// When something in the room is sharing with the far end or through other means
        /// </summary>
        public BoolFeedback IsSharingFeedback { get; private set; }

        //************************

        public VideoCodecBase VideoCodec { get; private set; }

        #endregion

        #region IPrivacy Members

        /// <summary>
        /// 
        /// </summary>
        public BoolFeedback PrivacyModeIsOnFeedback { get; private set; }

        public void PrivacyModeOff()
        {
            VideoCodec.PrivacyModeOff();
        }

        public void PrivacyModeOn()
        {
            VideoCodec.PrivacyModeOn();
        }

        public void PrivacyModeToggle()
        {
            VideoCodec.PrivacyModeToggle();
        }

        #endregion

        #region IRunDefaultCallRoute Members

        /// <summary>
        /// Sets up the room when started into call mode without presenting a source
        /// </summary>
        /// <returns></returns>
        public bool RunDefaultCallRoute()
        {
            RunRouteAction(DefaultCodecRouteString);
            return true;
        }

        #endregion

        private void Initialize()
        {
            try
            {
                if (DefaultAudioDevice != null)
                {
                    DefaultVolumeControls = DefaultAudioDevice as IBasicVolumeControls;
                }
                else if (DefaultAudioDevice is IHasVolumeDevice)
                {
                    DefaultVolumeControls = (DefaultAudioDevice as IHasVolumeDevice).VolumeDevice;
                }
                CurrentVolumeControls = DefaultVolumeControls;


                // Combines call feedback from both codecs if available
                InCallFeedback = new BoolFeedback(() =>
                {
                    var inAudioCall = false;
                    var inVideoCall = false;

                    if (AudioCodec != null)
                    {
                        inAudioCall = AudioCodec.IsInCall;
                    }

                    if (VideoCodec != null)
                    {
                        inVideoCall = VideoCodec.IsInCall;
                    }

                    return inAudioCall || inVideoCall;
                });

                // Get Microphone Privacy object, if any  MUST HAPPEN AFTER setting InCallFeedback
                MicrophonePrivacy = EssentialsRoomConfigHelper.GetMicrophonePrivacy(_propertiesConfig, this);

                Debug.Console(2, this, "Microphone Privacy Config evaluated.");

                // Get emergency object, if any
                Emergency = EssentialsRoomConfigHelper.GetEmergency(_propertiesConfig, this);

                Debug.Console(2, this, "Emergency Config evaluated.");


                VideoCodec.CallStatusChange += (o, a) => InCallFeedback.FireUpdate();

                if (AudioCodec != null)
                {
                    AudioCodec.CallStatusChange += (o, a) => InCallFeedback.FireUpdate();
                }

                IsSharingFeedback = new BoolFeedback(() => VideoCodec.SharingContentIsOnFeedback.BoolValue);
                VideoCodec.SharingContentIsOnFeedback.OutputChange += (o, a) => IsSharingFeedback.FireUpdate();

                // link privacy to VC (for now?)
                PrivacyModeIsOnFeedback = new BoolFeedback(() => VideoCodec.PrivacyModeIsOnFeedback.BoolValue);
                VideoCodec.PrivacyModeIsOnFeedback.OutputChange += (o, a) => PrivacyModeIsOnFeedback.FireUpdate();

                CallTypeFeedback = new IntFeedback(() => 0);

                SourceListKey = "default";
                EnablePowerOnToLastSource = true;

                var disp = DefaultDisplay as DisplayBase;
                if (disp == null)
                {
                    return;
                }

                OnFeedbackFunc = () => CurrentSourceInfo != null
                                       && CurrentSourceInfo.Type == eSourceListItemType.Route;

                IsWarmingFeedbackFunc = () => disp.IsWarmingUpFeedback.BoolValue;
                IsCoolingFeedbackFunc = () => disp.IsCoolingDownFeedback.BoolValue;

                InitializeDisplay(disp);
            }
            catch (Exception e)
            {
                Debug.Console(0, this, "Error Initializing Room: {0}", e);
            }
        }

        protected override void CustomSetConfig(DeviceConfig config)
        {
            var newPropertiesConfig =
                JsonConvert.DeserializeObject<EssentialsHuddleVtc1PropertiesConfig>(config.Properties.ToString());

            if (newPropertiesConfig != null)
            {
                _propertiesConfig = newPropertiesConfig;
            }

            ConfigWriter.UpdateRoomConfig(config);
        }

        public override bool CustomActivate()
        {
            // Add Occupancy object from config
            if (_propertiesConfig.Occupancy != null)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Setting Occupancy Provider for room");
                SetRoomOccupancy(DeviceManager.GetDeviceForKey(_propertiesConfig.Occupancy.DeviceKey) as
                    IOccupancyStatusProvider, _propertiesConfig.Occupancy.TimeoutMinutes);
            }

            LogoUrl = _propertiesConfig.Logo.GetUrl();
            SourceListKey = _propertiesConfig.SourceListKey;
            DefaultSourceItem = _propertiesConfig.DefaultSourceItem;
            DefaultVolume = (ushort) (_propertiesConfig.Volumes.Master.Level*65535/100);

            return base.CustomActivate();
        }


        /// <summary>
        /// 
        /// </summary>
        protected override void EndShutdown()
        {
            VideoCodec.EndAllCalls();

            if (AudioCodec != null)
            {
                AudioCodec.EndAllCalls();
            }

            base.EndShutdown();
        }


        public override void RoomVacatedForTimeoutPeriod(object o)
        {
            //Implement this
        }

        #region Overrides of EssentialsRoomBase

        protected override void PowerIsOnFeedbackOnOutputChange(object sender, FeedbackEventArgs args)
        {
            var disp = sender as DisplayBase;

            if (disp == null)
            {
                return;
            }

            if (disp.PowerIsOnFeedback.BoolValue != OnFeedback.BoolValue)
            {
                if (!disp.PowerIsOnFeedback.BoolValue)
                {
                    CurrentSourceInfo = null;
                }
                OnFeedback.FireUpdate();
            }
            if (disp.PowerIsOnFeedback.BoolValue)
            {
                SetDefaultLevels();
            }
        }

        protected override void IsCoolingDownFeedbackOnOutputChange(object sender, FeedbackEventArgs args)
        {
            IsCoolingDownFeedback.FireUpdate();
        }

        protected override void IsWarmingUpFeedbackOnOutputChange(object sender, FeedbackEventArgs args)
        {
            IsWarmingUpFeedback.FireUpdate();

            if (IsWarmingUpFeedback.BoolValue)
            {
                return;
            }

            var basicVolumeWithFeedback = CurrentVolumeControls as IBasicVolumeWithFeedback;
            if (basicVolumeWithFeedback != null)
            {
                basicVolumeWithFeedback.SetVolume(DefaultVolume);
            }
        }

        #endregion
    }
}