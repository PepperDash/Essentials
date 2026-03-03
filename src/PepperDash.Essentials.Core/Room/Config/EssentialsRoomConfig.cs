using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Privacy;
using Serilog.Events;

namespace PepperDash.Essentials.Room.Config
{
    /// <summary>
    /// Represents a EssentialsRoomConfigHelper
    /// </summary>
    public class EssentialsRoomConfigHelper
    {
        /// <summary>
        /// GetEmergency method
        /// </summary>
        public static EssentialsRoomEmergencyBase GetEmergency(EssentialsRoomPropertiesConfig props, IEssentialsRoom room)
        {
            // This emergency 
            var emergency = props.Emergency;
            if (emergency != null)
            {
                //switch on emergency type here.  Right now only contact and shutdown
                var e = new EssentialsRoomEmergencyContactClosure(room.Key + "-emergency", props.Emergency, room);
                DeviceManager.AddDevice(e);
                return e;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="props"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        /// <summary>
        /// GetMicrophonePrivacy method
        /// </summary>
        public static MicrophonePrivacyController GetMicrophonePrivacy(
            EssentialsRoomPropertiesConfig props, IPrivacy room)
        {
            var microphonePrivacy = props.MicrophonePrivacy;
            if (microphonePrivacy == null)
            {
                Debug.LogMessage(LogEventLevel.Information, "Cannot create microphone privacy with null properties");
                return null;
            }
            // Get the MicrophonePrivacy device from the device manager
            var mP = (DeviceManager.GetDeviceForKey(props.MicrophonePrivacy.DeviceKey) as MicrophonePrivacyController);
            // Set this room as the IPrivacy device
            if (mP == null)
            {
                Debug.LogMessage(LogEventLevel.Information, "ERROR: Selected device {0} is not MicrophonePrivacyController", props.MicrophonePrivacy.DeviceKey);
                return null;
            }
            mP.SetPrivacyDevice(room);

            var behaviour = props.MicrophonePrivacy.Behaviour.ToLower();

            if (behaviour == null)
            {
                Debug.LogMessage(LogEventLevel.Information, "WARNING: No behaviour defined for MicrophonePrivacyController");
                return null;
            }
            if (behaviour == "trackroomstate")
            {
                // Tie LED enable to room power state
                var essRoom = room as IEssentialsRoom;
                essRoom.OnFeedback.OutputChange += (o, a) =>
                {
                    if (essRoom.OnFeedback.BoolValue)
                        mP.EnableLeds = true;
                    else
                        mP.EnableLeds = false;
                };

                mP.EnableLeds = essRoom.OnFeedback.BoolValue;
            }
            else if (behaviour == "trackcallstate")
            {
                // Tie LED enable to room power state
                var inCallRoom = room as IHasInCallFeedback;
                inCallRoom.InCallFeedback.OutputChange += (o, a) =>
                {
                    if (inCallRoom.InCallFeedback.BoolValue)
                        mP.EnableLeds = true;
                    else
                        mP.EnableLeds = false;
                };

                mP.EnableLeds = inCallRoom.InCallFeedback.BoolValue;
            }

            return mP;
        }

    }

    /// <summary>
    /// Represents a EssentialsRoomPropertiesConfig
    /// </summary>
    public class EssentialsRoomPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the Addresses
        /// </summary>
        [JsonProperty("addresses")]
        public EssentialsRoomAddressPropertiesConfig Addresses { get; set; }

        /// <summary>
        /// Gets or sets the Description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Emergency
        /// </summary>
        [JsonProperty("emergency")]
        public EssentialsRoomEmergencyConfig Emergency { get; set; }

        /// <summary>
        /// Gets or sets the Help
        /// </summary>
        [JsonProperty("help")]
        public EssentialsHelpPropertiesConfig Help { get; set; }

        /// <summary>
        /// Gets or sets the HelpMessage
        /// </summary>
        [JsonProperty("helpMessage")]
        public string HelpMessage { get; set; }

        /// <summary>
        /// Read this value to get the help message.  It checks for the old and new config format.
        /// </summary>
        public string HelpMessageForDisplay
        {
            get
            {
                if (Help != null && !string.IsNullOrEmpty(Help.Message))
                {
                    return Help.Message;
                }
                else
                {
                    return HelpMessage;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Environment
        /// </summary>
        [JsonProperty("environment")]
        public EssentialsEnvironmentPropertiesConfig Environment { get; set; }

        /// <summary>
        /// Gets or sets the LogoLight
        /// </summary>
        [JsonProperty("logo")]
        public EssentialsLogoPropertiesConfig LogoLight { get; set; }

        /// <summary>
        /// Gets or sets the LogoDark
        /// </summary>
        [JsonProperty("logoDark")]
        public EssentialsLogoPropertiesConfig LogoDark { get; set; }

        /// <summary>
        /// Gets or sets the MicrophonePrivacy
        /// </summary>
        [JsonProperty("microphonePrivacy")]
        public EssentialsRoomMicrophonePrivacyConfig MicrophonePrivacy { get; set; }

        /// <summary>
        /// Gets or sets the Occupancy
        /// </summary>
        [JsonProperty("occupancy")]
        public EssentialsRoomOccSensorConfig Occupancy { get; set; }

        /// <summary>
        /// Gets or sets the OneButtonMeeting
        /// </summary>
        [JsonProperty("oneButtonMeeting")]
        public EssentialsOneButtonMeetingPropertiesConfig OneButtonMeeting { get; set; }

        /// <summary>
        /// Gets or sets the ShutdownVacancySeconds
        /// </summary>
        [JsonProperty("shutdownVacancySeconds")]
        public int ShutdownVacancySeconds { get; set; }

        /// <summary>
        /// Gets or sets the ShutdownPromptSeconds
        /// </summary>
        [JsonProperty("shutdownPromptSeconds")]
        public int ShutdownPromptSeconds { get; set; }

        /// <summary>
        /// Gets or sets the Tech
        /// </summary>
        [JsonProperty("tech")]
        public EssentialsRoomTechConfig Tech { get; set; }

        /// <summary>
        /// Gets or sets the Volumes
        /// </summary>
        [JsonProperty("volumes")]
        public EssentialsRoomVolumesConfig Volumes { get; set; }

        /// <summary>
        /// Gets or sets the Fusion
        /// </summary>
        [JsonProperty("fusion")]
        public EssentialsRoomFusionConfig Fusion { get; set; }

        /// <summary>
        /// Gets or sets the UiBehavior
        /// </summary>
        [JsonProperty("essentialsRoomUiBehaviorConfig", NullValueHandling = NullValueHandling.Ignore)]
        public EssentialsRoomUiBehaviorConfig UiBehavior { get; set; }

        /// <summary>
        /// Gets or sets the ZeroVolumeWhenSwtichingVolumeDevices
        /// </summary>
        [JsonProperty("zeroVolumeWhenSwtichingVolumeDevices")]
        public bool ZeroVolumeWhenSwtichingVolumeDevices { get; set; }

        /// <summary>
        /// Indicates if this room represents a combination of other rooms
        /// </summary>
        [JsonProperty("isRoomCombinationScenario")]
        public bool IsRoomCombinationScenario { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public EssentialsRoomPropertiesConfig()
        {
            LogoLight = new EssentialsLogoPropertiesConfig();
            LogoDark = new EssentialsLogoPropertiesConfig();
        }
    }

    /// <summary>
    /// Represents a EssentialsRoomUiBehaviorConfig
    /// </summary>
    public class EssentialsRoomUiBehaviorConfig
    {
        /// <summary>
        /// Gets or sets the DisableActivityButtonsWhileWarmingCooling
        /// </summary>
        [JsonProperty("disableActivityButtonsWhileWarmingCooling")]
        public bool DisableActivityButtonsWhileWarmingCooling { get; set; }
    }

    /// <summary>
    /// Represents a EssentialsAvRoomPropertiesConfig
    /// </summary>
    public class EssentialsAvRoomPropertiesConfig : EssentialsRoomPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the DefaultAudioKey
        /// </summary>
        [JsonProperty("defaultAudioKey")]
        public string DefaultAudioKey { get; set; }

        /// <summary>
        /// Gets or sets the DefaultOnDspPresetKey
        /// </summary>
        [JsonProperty("defaultOnDspPresetKey")]
        public string DefaultOnDspPresetKey { get; set; }

        /// <summary>
        /// Gets or sets the DefaultOffDspPresetKey
        /// </summary>
        [JsonProperty("defaultOffDspPresetKey")]
        public string DefaultOffDspPresetKey { get; set; }

        /// <summary>
        /// Gets or sets the SourceListKey
        /// </summary>
        [JsonProperty("sourceListKey")]
        public string SourceListKey { get; set; }
        /// <summary>
        /// Gets or sets the DestinationListKey
        /// </summary>
        [JsonProperty("destinationListKey")]
        public string DestinationListKey { get; set; }
        /// <summary>
        /// Gets or sets the AudioControlPointListKey
        /// </summary>
        [JsonProperty("audioControlPointListKey")]
        public string AudioControlPointListKey { get; set; }
        /// <summary>
        /// Gets or sets the CameraListKey
        /// </summary>
        [JsonProperty("cameraListKey")]
        public string CameraListKey { get; set; }


        /// <summary>
        /// Gets or sets the DefaultSourceItem
        /// </summary>
        [JsonProperty("defaultSourceItem")]
        public string DefaultSourceItem { get; set; }
        /// <summary>
        /// Indicates if the room supports advanced sharing
        /// </summary>
        [JsonProperty("supportsAdvancedSharing")]
        public bool SupportsAdvancedSharing { get; set; }

        /// <summary>
        /// Indicates if non-tech users can change the share mode
        /// </summary>
        [JsonProperty("userCanChangeShareMode")]
        public bool UserCanChangeShareMode { get; set; }


        /// <summary>
        /// Gets or sets the MatrixRoutingKey
        /// </summary>
        [JsonProperty("matrixRoutingKey", NullValueHandling = NullValueHandling.Ignore)]
        public string MatrixRoutingKey { get; set; }
    }

    /// <summary>
    /// Represents a EssentialsConferenceRoomPropertiesConfig
    /// </summary>
    public class EssentialsConferenceRoomPropertiesConfig : EssentialsAvRoomPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the VideoCodecKey
        /// </summary>
        [JsonProperty("videoCodecKey")]
        public string VideoCodecKey { get; set; }
        /// <summary>
        /// Gets or sets the AudioCodecKey
        /// </summary>
        [JsonProperty("audioCodecKey")]
        public string AudioCodecKey { get; set; }

    }

    /// <summary>
    /// Represents a EssentialsEnvironmentPropertiesConfig
    /// </summary>
    public class EssentialsEnvironmentPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the Enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the DeviceKeys
        /// </summary>
        [JsonProperty("deviceKeys")]
        public List<string> DeviceKeys { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public EssentialsEnvironmentPropertiesConfig()
        {
            DeviceKeys = new List<string>();
        }

    }

    /// <summary>
    /// Represents a EssentialsRoomFusionConfig
    /// </summary>
    public class EssentialsRoomFusionConfig
    {
        /// <summary>
        /// Gets the the IpId as a UInt16
        /// </summary>
        public uint IpIdInt
        {
            get
            {
                try
                {
                    return Convert.ToUInt32(IpId, 16);
                }
                catch (Exception)
                {
                    throw new FormatException(string.Format("ERROR:Unable to convert IP ID: {0} to hex.  Error:\n{1}", IpId));
                }

            }
        }

        /// <summary>
        /// Gets or sets the IpId
        /// </summary>
        [JsonProperty("ipId")]
        public string IpId { get; set; }

        /// <summary>
        /// Gets or sets the JoinMapKey
        /// </summary>
        [JsonProperty("joinMapKey")]
        public string JoinMapKey { get; set; }

    }

    /// <summary>
    /// Represents a EssentialsRoomMicrophonePrivacyConfig
    /// </summary>
    public class EssentialsRoomMicrophonePrivacyConfig
    {
        /// <summary>
        /// Gets or sets the DeviceKey
        /// </summary>
        [JsonProperty("deviceKey")]
        public string DeviceKey { get; set; }

        /// <summary>
        /// Gets or sets the Behaviour
        /// </summary>
        [JsonProperty("behaviour")]
        public string Behaviour { get; set; }
    }

    /// <summary>
    /// Represents a EssentialsHelpPropertiesConfig
    /// </summary>
    public class EssentialsHelpPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the Message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the ShowCallButton
        /// </summary>
        [JsonProperty("showCallButton")]
        public bool ShowCallButton { get; set; }

        /// <summary>
        /// Defaults to "Call Help Desk"
        /// </summary>
        [JsonProperty("callButtonText")]
        public string CallButtonText { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public EssentialsHelpPropertiesConfig()
        {
            CallButtonText = "Call Help Desk";
        }
    }

    /// <summary>
    /// Represents a EssentialsOneButtonMeetingPropertiesConfig
    /// </summary>
    public class EssentialsOneButtonMeetingPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the Enable
        /// </summary>
        [JsonProperty("enable")]
        public bool Enable { get; set; }
    }

    /// <summary>
    /// Represents a EssentialsRoomAddressPropertiesConfig
    /// </summary>
    public class EssentialsRoomAddressPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the PhoneNumber
        /// </summary>
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the SipAddress
        /// </summary>
        [JsonProperty("sipAddress")]
        public string SipAddress { get; set; }
    }


    /// <summary>
    /// Represents a EssentialsLogoPropertiesConfig
    /// </summary>
    public class EssentialsLogoPropertiesConfig
    {
        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Url
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// GetLogoUrlLight method
        /// </summary>
        public string GetLogoUrlLight()
        {
            if (Type == "url")
                return Url;
            if (Type == "system")
                return string.Format("http://{0}:8080/logo.png",
                    CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0));
            return null;
        }

        /// <summary>
        /// GetLogoUrlDark method
        /// </summary>
        public string GetLogoUrlDark()
        {
            if (Type == "url")
                return Url;
            if (Type == "system")
                return string.Format("http://{0}:8080/logo-dark.png",
                    CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0));
            return null;
        }
    }

    /// <summary>
    /// Represents a EssentialsRoomOccSensorConfig
    /// </summary>
    public class EssentialsRoomOccSensorConfig
    {
        /// <summary>
        /// Gets or sets the DeviceKey
        /// </summary>
        [JsonProperty("deviceKey")]
        public string DeviceKey { get; set; }

        /// <summary>
        /// Gets or sets the TimeoutMinutes
        /// </summary>
        [JsonProperty("timeoutMinutes")]
        public int TimeoutMinutes { get; set; }
    }

    /// <summary>
    /// Represents a EssentialsRoomTechConfig
    /// </summary>
    public class EssentialsRoomTechConfig
    {
        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}