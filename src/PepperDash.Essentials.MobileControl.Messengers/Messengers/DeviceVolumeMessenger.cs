using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.AppServer.Messengers
{
    /// <summary>
    /// Represents a DeviceVolumeMessenger
    /// </summary>
    public class DeviceVolumeMessenger : MessengerBase
    {
        private readonly IBasicVolumeControls device;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceVolumeMessenger"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="messagePath">The message path.</param>
        /// <param name="device">The device.</param>
        public DeviceVolumeMessenger(string key, string messagePath, IBasicVolumeControls device)
            : base(key, messagePath, device as IKeyName)
        {
            this.device = device;
        }

        private void SendStatus(string id = null)
        {
            try
            {
                if (!(device is IBasicVolumeWithFeedback feedbackDevice))
                {
                    return;
                }

                var messageObj = new VolumeStateMessage
                {
                    Volume = new Volume
                    {
                        Level = feedbackDevice?.VolumeLevelFeedback.IntValue ?? -1,
                        Muted = feedbackDevice?.MuteFeedback.BoolValue ?? false,
                        HasMute = true,  // assume all devices have mute for now
                    }
                };

                if (device is IBasicVolumeWithFeedbackAdvanced volumeAdvanced)
                {
                    messageObj.Volume.RawValue = volumeAdvanced.RawVolumeLevel.ToString();
                    messageObj.Volume.Units = volumeAdvanced.Units;
                }

                PostStatusMessage(messageObj, id);
            }
            catch (Exception ex)
            {
                Debug.LogMessage(ex, "Exception sending full status", this);
            }
        }

        #region Overrides of MessengerBase

        /// <inheritdoc />
        protected override void RegisterActions()
        {
            AddAction("/volumeUp", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) =>
                        {
                            Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Calling {localDevice} volume up with {value}", DeviceKey, b);
                            try
                            {
                                device.VolumeUp(b);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogMessage(ex, "Got exception during volume up: {Exception}", null, ex);
                            }
                        }));

            AddAction("/muteToggle", (id, content) =>
                        {
                            device.MuteToggle();
                        });

            AddAction("/volumeDown", (id, content) => PressAndHoldHandler.HandlePressAndHold(DeviceKey, content, (b) =>
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Verbose, "Calling {localDevice} volume down with {value}", DeviceKey, b);

                try
                {
                    device.VolumeDown(b);
                }
                catch (Exception ex)
                {
                    Debug.LogMessage(ex, "Got exception during volume down: {Exception}", null, ex);
                }
            }));

            if (!(device is IBasicVolumeWithFeedback feedback))
            {
                this.LogDebug("Skipping feedback methods for {deviceKey}", (device as IKeyName)?.Key);
                return;
            }

            AddAction("/fullStatus", (id, content) => SendStatus(id));

            AddAction("/volumeStatus", (id, content) => SendStatus(id));

            AddAction("/level", (id, content) =>
            {
                var volume = content.ToObject<MobileControlSimpleContent<ushort>>();

                feedback.SetVolume(volume.Value);
            });



            AddAction("/muteOn", (id, content) =>
            {
                feedback.MuteOn();
            });

            AddAction("/muteOff", (id, content) =>
            {
                feedback.MuteOff();
            });



            feedback.MuteFeedback.OutputChange += (sender, args) =>
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

            feedback.VolumeLevelFeedback.OutputChange += (sender, args) =>
            {
                var rawValue = "";
                if (feedback is IBasicVolumeWithFeedbackAdvanced volumeAdvanced)
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
    /// Represents a VolumeStateMessage
    /// </summary>
    public class VolumeStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("volume", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the Volume
        /// </summary>
        public Volume Volume { get; set; }
    }

    /// <summary>
    /// Represents a Volume
    /// </summary>
    public class Volume
    {
        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public int? Level { get; set; }

        [JsonProperty("hasMute", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasMute { get; set; }

        [JsonProperty("muted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Muted { get; set; }

        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the Label
        /// </summary>
        public string Label { get; set; }

        [JsonProperty("rawValue", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the RawValue
        /// </summary>
        public string RawValue { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("units", NullValueHandling = NullValueHandling.Ignore)]
        public eVolumeLevelUnits? Units { get; set; }
    }
}