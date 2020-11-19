using System;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Rooms;
using PepperDash.Essentials.Core.Rooms.Config;

namespace PepperDash.Essentials
{
    public class EssentialsHuddleSpaceRoom : EssentialsRoomBase, IRunRouteAction,
        IRunDefaultPresentRoute, IHasCurrentVolumeControls, IHasDefaultDisplay, IHasCurrentSourceInfoChange
    {
        public EssentialsHuddleSpaceRoom(DeviceConfig config)
            : base(config)
        {
            try
            {
                PropertiesConfig = config.Properties.ToObject<EssentialsHuddleRoomPropertiesConfig>();
                DefaultDisplay =
                    DeviceManager.GetDeviceForKey(PropertiesConfig.DefaultDisplayKey) as IRoutingSinkWithSwitching;
                    //why are we assuming IRoutingSinkWithSwitching here?

                DefaultAudioDevice =
                    DeviceManager.GetDeviceForKey(PropertiesConfig.DefaultAudioKey) as IRoutingSinkWithSwitching;

                Initialize();
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error building room: \n{0}", e);
            }
        }

        public EssentialsHuddleRoomPropertiesConfig PropertiesConfig { get; private set; }

        private void Initialize()
        {
            if (DefaultAudioDevice is IBasicVolumeControls)
            {
                DefaultVolumeControls = DefaultAudioDevice as IBasicVolumeControls;
            }
            else if (DefaultAudioDevice is IHasVolumeDevice)
            {
                DefaultVolumeControls = (DefaultAudioDevice as IHasVolumeDevice).VolumeDevice;
            }
            CurrentVolumeControls = DefaultVolumeControls;

            SourceListKey = String.IsNullOrEmpty(PropertiesConfig.SourceListKey)
                ? DefaultSourceListKey
                : PropertiesConfig.SourceListKey;

            EnablePowerOnToLastSource = true;

            var disp = DefaultDisplay as TwoWayDisplayBase;
            if (disp == null)
            {
                return;
            }

            IsWarmingFeedbackFunc = () => disp.IsWarmingUpFeedback.BoolValue;

            IsCoolingFeedbackFunc = () => disp.IsCoolingDownFeedback.BoolValue;

            OnFeedbackFunc = () => CurrentSourceInfo != null
                                   && CurrentSourceInfo.Type == eSourceListItemType.Route;

            InitializeDisplay(disp);
        }

        protected override void IsCoolingDownFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            IsCoolingDownFeedback.FireUpdate();
        }

        protected override void PowerIsOnFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            var display = sender as TwoWayDisplayBase;

            if (display == null)
            {
                return;
            }

            if (display.PowerIsOnFeedback.BoolValue == OnFeedback.BoolValue)
            {
                return;
            }

            if (!display.PowerIsOnFeedback.BoolValue)
            {
                CurrentSourceInfo = null;
            }
            OnFeedback.FireUpdate();
        }

        protected override void IsWarmingUpFeedbackOnOutputChange(object sender, FeedbackEventArgs feedbackEventArgs)
        {
            IsWarmingUpFeedback.FireUpdate();

            if (IsWarmingUpFeedback.BoolValue)
            {
                return;
            }

            var displayVolumeControl = DefaultDisplay as IBasicVolumeWithFeedback;

            if (displayVolumeControl == null)
            {
                Debug.Console(0, this, Debug.ErrorLogLevel.Error,
                    "Default display {0} is not volume control control provider", DefaultDisplay.Key);
                return;
            }

            displayVolumeControl.SetVolume(DefaultVolume);
        }

        protected override void CustomSetConfig(DeviceConfig config)
        {
            var newPropertiesConfig =
                JsonConvert.DeserializeObject<EssentialsHuddleRoomPropertiesConfig>(config.Properties.ToString());

            if (newPropertiesConfig != null)
            {
                PropertiesConfig = newPropertiesConfig;
            }

            ConfigWriter.UpdateRoomConfig(config);
        }

        public override bool CustomActivate()
        {
            // Add Occupancy object from config
            if (PropertiesConfig.Occupancy != null)
            {
                SetRoomOccupancy(DeviceManager.GetDeviceForKey(PropertiesConfig.Occupancy.DeviceKey) as
                    IOccupancyStatusProvider, PropertiesConfig.Occupancy.TimeoutMinutes);
            }

            LogoUrl = PropertiesConfig.Logo.GetUrl();
            SourceListKey = PropertiesConfig.SourceListKey;
            DefaultSourceItem = PropertiesConfig.DefaultSourceItem;
            DefaultVolume = (ushort) (PropertiesConfig.Volumes.Master.Level*65535/100);

            return base.CustomActivate();
        }

        public override void RoomVacatedForTimeoutPeriod(object o)
        {
            //TODO: Implement RoomVacatedForTimeoutPeriod 
        }
    }
}