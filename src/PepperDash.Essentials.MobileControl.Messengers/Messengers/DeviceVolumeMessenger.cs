using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using System;

namespace PepperDash.Essentials.AppServer.Messengers
{
    public class DeviceVolumeMessenger : MessengerBase
    {
        private readonly IBasicVolumeWithFeedback _localDevice;

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
            } catch(Exception ex)
            {
                Debug.LogMessage(ex, "Exception sending full status", this);
            }
        }

        #region Overrides of MessengerBase

#if SERIES4
        protected override void RegisterActions()
#else
        protected override void CustomRegisterWithAppServer(MobileControlSystemController appServerController)
#endif
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
                } catch (Exception ex)
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

    public class VolumeStateMessage : DeviceStateMessageBase
    {
        [JsonProperty("volume", NullValueHandling = NullValueHandling.Ignore)]
        public Volume Volume { get; set; }
    }

    public class Volume
    {
        [JsonProperty("level", NullValueHandling = NullValueHandling.Ignore)]
        public int? Level { get; set; }

        [JsonProperty("hasMute", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasMute { get; set; }

        [JsonProperty("muted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Muted { get; set; }

        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        [JsonProperty("rawValue", NullValueHandling = NullValueHandling.Ignore)]
        public string RawValue { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("units", NullValueHandling = NullValueHandling.Ignore)]
        public eVolumeLevelUnits? Units { get; set; }
    }
}