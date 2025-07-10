using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Provides messaging capabilities for device volume control operations.
    /// Handles volume level adjustment, mute control, and volume status reporting.
    /// </summary>
    public class DeviceVolumeMessenger : MessengerBase
    {
        private readonly IBasicVolumeWithFeedback _localDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceVolumeMessenger"/> class.
        /// </summary>
        /// <param name="key">The unique identifier for this messenger instance.</param>
        /// <param name="messagePath">The message path for volume control messages.</param>
        /// <param name="device">The device that provides volume control functionality.</param>
        public DeviceVolumeMessenger(string key, string messagePath, IBasicVolumeWithFeedback device)
            : base(key, messagePath, device as IKeyName)
        {
            _localDevice = device;
        }

        private void SendStatus()
        {
            try
            {
                var messageObj = new VolumeStateMessage
                {
                    Volume = new Volume
                    {
                        Level = _localDevice?.VolumeLevelFeedback.IntValue ?? -1,
                        Muted = _localDevice?.MuteFeedback.BoolValue ?? false,
                        HasMute = true,  // assume all devices have mute for now
                    }
                };

                if (_localDevice is IBasicVolumeWithFeedbackAdvanced volumeAdvanced)
                {
                    messageObj.Volume.RawValue = volumeAdvanced.RawVolumeLevel.ToString();
                    messageObj.Volume.Units = volumeAdvanced.Units;
                }

                PostStatusMessage(messageObj);
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception sending full status", this);
            }
        }

        #region Overrides of MessengerBase


        /// <summary>
        /// Registers actions for handling volume control operations.
        /// Includes volume level adjustment, mute control, and full status reporting.
        /// </summary>
        protected override void RegisterActions()
        {
            AddAction("/fullStatus", (id, content) => SendStatus());

            AddAction("/level", (id, content) =>
            {
                var volume = content.ToObject<MobileControlSimpleContent<ushort>>();

                _localDevice.SetVolume(volume.Value);
            });

            AddAction("/muteToggle", (id, content) =>
            {
                _localDevice.MuteToggle();
            });

            AddAction("/muteOn", (id, content) =>
            {
                _localDevice.MuteOn();
            });

            AddAction("/muteOff", (id, content) =>
            {
                _localDevice.MuteOff();
            });

            AddAction("/volumeUp", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) =>
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Calling {localDevice} volume up with {value}", DeviceKey, b);
                try
                {
                    _localDevice.VolumeUp(b);
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(ex, "Got exception during volume up: {Exception}", null, ex);
                }
            }));



            AddAction("/volumeDown", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) =>
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Calling {localDevice} volume down with {value}", DeviceKey, b);

                try
                {
                    _localDevice.VolumeDown(b);
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(ex, "Got exception during volume down: {Exception}", null, ex);
                }
            }));

            _localDevice.MuteFeedback.OutputChange += (sender, args) =>
            {
                PostStatusMessage(JToken.FromObject(
                        new
                        {
                            volume = new
                            {
                                muted = args.BoolValue
                            }
                        })
                );
            };

            _localDevice.VolumeLevelFeedback.OutputChange += (sender, args) =>
            {
                var rawValue = "";
                if (_localDevice is IBasicVolumeWithFeedbackAdvanced volumeAdvanced)
                {
                    rawValue = volumeAdvanced.RawVolumeLevel.ToString();
                }

                var message = new
                {
                    volume = new
                    {
                        level = args.IntValue,
                        rawValue
                    }
                };

                PostStatusMessage(JToken.FromObject(message));
            };


        }

        #endregion
    }

    /// <summary>
    /// Represents a volume state message containing volume information.
    /// </summary>
    public class VolumeStateMessage : DeviceStateMessageBase
    {
        /// <summary>
        /// Gets or sets the volume information.
        /// </summary>
        [JsonProperty("volume", NullValueHandling = NullValueHandling.Ignore)]
        public Volume Volume { get; set; }
    }

    /// <summary>
    /// Represents volume control information including level, mute status, and units.
    /// </summary>
    public class Volume
    {
        /// <summary>
        /// Gets or sets the volume level.
        /// </summary>
        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public int? Level { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the device has mute capability.
        /// </summary>
        [JsonProperty("hasMute", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasMute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the device is currently muted.
        /// </summary>
        [JsonProperty("muted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Muted { get; set; }

        /// <summary>
        /// Gets or sets the volume label for display purposes.
        /// </summary>
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the raw volume value as a string.
        /// </summary>
        [JsonProperty("rawValue", NullValueHandling = NullValueHandling.Ignore)]
        public string RawValue { get; set; }

        /// <summary>
        /// Gets or sets the volume level units.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("units", NullValueHandling = NullValueHandling.Ignore)]
        public eVolumeLevelUnits? Units { get; set; }
    }
}