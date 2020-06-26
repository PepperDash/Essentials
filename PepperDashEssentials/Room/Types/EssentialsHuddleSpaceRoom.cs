using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Room.Config;

namespace PepperDash.Essentials
{
    public class EssentialsHuddleSpaceRoom : EssentialsRoomBase, IRunRouteAction,
        IRunDefaultPresentRoute, IHasCurrentVolumeControls, IHasDefaultDisplay
    {
        public EssentialsHuddleRoomPropertiesConfig PropertiesConfig { get; private set; }

        /// <summary>
        /// If room is off, enables power on to last source. Default true
        /// </summary>
        public bool EnablePowerOnToLastSource { get; set; }

        public EssentialsHuddleSpaceRoom(DeviceConfig config)
            : base(config)
        {
            try
            {
                PropertiesConfig = JsonConvert.DeserializeObject<EssentialsHuddleRoomPropertiesConfig>
                    (config.Properties.ToString());
                DefaultDisplay =
                    DeviceManager.GetDeviceForKey(PropertiesConfig.DefaultDisplayKey) as IRoutingSinkWithSwitching;


                DefaultAudioDevice =
                    DeviceManager.GetDeviceForKey(PropertiesConfig.DefaultAudioKey) as IRoutingSinkWithSwitching;

                Initialize();
            }
            catch (Exception e)
            {
                Debug.Console(1, this, "Error building room: \n{0}", e);
            }
        }

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

            SourceListKey = "default";
            EnablePowerOnToLastSource = true;

            var disp = DefaultDisplay as DisplayBase;
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
            var display = sender as DisplayBase;

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

        /// <summary>
        /// 
        /// </summary>
        protected override void EndShutdown()
        {
            SetDefaultLevels();

            RunDefaultPresentRoute();

            //CrestronEnvironment.Sleep(1000); //why?

            Debug.Console(0, this, Debug.ErrorLogLevel.Notice, "Shutting down room");

            RunRouteAction("roomOff");
        }

        /// <summary>
        /// Routes the default source item, if any
        /// </summary>
        public override bool RunDefaultPresentRoute()
        {
            if (DefaultSourceItem == null)
            {
                Debug.Console(0, this, "Unable to run default present route, DefaultSourceItem is null.");
                return false;
            }

            RunRouteAction(DefaultSourceItem);
            return true;
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

        /// <summary>
        /// Will power the room on with the last-used source
        /// </summary>
        public override void PowerOnToDefaultOrLastSource()
        {
            if (!EnablePowerOnToLastSource || LastSourceKey == null)
            {
                return;
            }
            RunRouteAction(LastSourceKey);
        }

        /// <summary>
        /// Does what it says
        /// </summary>
        public override void SetDefaultLevels()
        {
            Debug.Console(1, this, "Restoring default levels");
            var vc = CurrentVolumeControls as IBasicVolumeWithFeedback;
            if (vc != null)
            {
                vc.SetVolume(DefaultVolume);
            }
        }

        public override void RoomVacatedForTimeoutPeriod(object o)
        {
            //TODO: Implement RoomVacatedForTimeoutPeriod 
        }
    }
}