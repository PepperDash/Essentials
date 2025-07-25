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
		[JsonProperty("addresses")]
		public EssentialsRoomAddressPropertiesConfig Addresses { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("emergency")]
		public EssentialsRoomEmergencyConfig Emergency { get; set; }

		[JsonProperty("help")]
  /// <summary>
  /// Gets or sets the Help
  /// </summary>
		public EssentialsHelpPropertiesConfig Help { get; set; }

		[JsonProperty("helpMessage")]
  /// <summary>
  /// Gets or sets the HelpMessage
  /// </summary>
		public string HelpMessage { get; set; }

        /// <summary>
        /// Read this value to get the help message.  It checks for the old and new config format.
        /// </summary>
        public string HelpMessageForDisplay
        {
            get
            {
                if(Help != null && !string.IsNullOrEmpty(Help.Message))
                {
                    return Help.Message;
                }
                else
                {
                    return HelpMessage; 
                }
            }
        }

		[JsonProperty("environment")]
  /// <summary>
  /// Gets or sets the Environment
  /// </summary>
		public EssentialsEnvironmentPropertiesConfig Environment { get; set; }

		[JsonProperty("logo")]
  /// <summary>
  /// Gets or sets the LogoLight
  /// </summary>
		public EssentialsLogoPropertiesConfig LogoLight { get; set; }

        [JsonProperty("logoDark")]
        /// <summary>
        /// Gets or sets the LogoDark
        /// </summary>
        public EssentialsLogoPropertiesConfig LogoDark { get; set; }
	
		[JsonProperty("microphonePrivacy")]
  /// <summary>
  /// Gets or sets the MicrophonePrivacy
  /// </summary>
		public EssentialsRoomMicrophonePrivacyConfig MicrophonePrivacy { get; set; }

		[JsonProperty("occupancy")]
  /// <summary>
  /// Gets or sets the Occupancy
  /// </summary>
		public EssentialsRoomOccSensorConfig Occupancy { get; set; }

		[JsonProperty("oneButtonMeeting")]
  /// <summary>
  /// Gets or sets the OneButtonMeeting
  /// </summary>
		public EssentialsOneButtonMeetingPropertiesConfig OneButtonMeeting { get; set; }

		[JsonProperty("shutdownVacancySeconds")]
  /// <summary>
  /// Gets or sets the ShutdownVacancySeconds
  /// </summary>
		public int ShutdownVacancySeconds { get; set; }

		[JsonProperty("shutdownPromptSeconds")]
  /// <summary>
  /// Gets or sets the ShutdownPromptSeconds
  /// </summary>
		public int ShutdownPromptSeconds { get; set; }

		[JsonProperty("tech")]
  /// <summary>
  /// Gets or sets the Tech
  /// </summary>
		public EssentialsRoomTechConfig Tech { get; set; }

		[JsonProperty("volumes")]
  /// <summary>
  /// Gets or sets the Volumes
  /// </summary>
		public EssentialsRoomVolumesConfig Volumes { get; set; }

        [JsonProperty("fusion")]
        /// <summary>
        /// Gets or sets the Fusion
        /// </summary>
        public EssentialsRoomFusionConfig Fusion { get; set; }

        [JsonProperty("essentialsRoomUiBehaviorConfig", NullValueHandling=NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the UiBehavior
        /// </summary>
        public EssentialsRoomUiBehaviorConfig UiBehavior { get; set; }

		[JsonProperty("zeroVolumeWhenSwtichingVolumeDevices")]
  /// <summary>
  /// Gets or sets the ZeroVolumeWhenSwtichingVolumeDevices
  /// </summary>
		public bool ZeroVolumeWhenSwtichingVolumeDevices { get; set; }

        /// <summary>
        /// Indicates if this room represents a combination of other rooms
        /// </summary>
        [JsonProperty("isRoomCombinationScenario")]
        /// <summary>
        /// Gets or sets the IsRoomCombinationScenario
        /// </summary>
        public bool IsRoomCombinationScenario { get; set; }

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
        [JsonProperty("disableActivityButtonsWhileWarmingCooling")]
        /// <summary>
        /// Gets or sets the DisableActivityButtonsWhileWarmingCooling
        /// </summary>
        public bool DisableActivityButtonsWhileWarmingCooling { get; set; }
    }

    /// <summary>
    /// Represents a EssentialsAvRoomPropertiesConfig
    /// </summary>
    public class EssentialsAvRoomPropertiesConfig : EssentialsRoomPropertiesConfig
    {
        [JsonProperty("defaultAudioKey")]
        /// <summary>
        /// Gets or sets the DefaultAudioKey
        /// </summary>
        public string DefaultAudioKey { get; set; }
        [JsonProperty("sourceListKey")]
        /// <summary>
        /// Gets or sets the SourceListKey
        /// </summary>
        public string SourceListKey { get; set; }
        [JsonProperty("destinationListKey")]
        /// <summary>
        /// Gets or sets the DestinationListKey
        /// </summary>
        public string DestinationListKey { get; set; }
        [JsonProperty("audioControlPointListKey")]
        /// <summary>
        /// Gets or sets the AudioControlPointListKey
        /// </summary>
        public string AudioControlPointListKey { get; set; }
        [JsonProperty("cameraListKey")]
        /// <summary>
        /// Gets or sets the CameraListKey
        /// </summary>
        public string CameraListKey { get; set; }


        [JsonProperty("defaultSourceItem")]
        /// <summary>
        /// Gets or sets the DefaultSourceItem
        /// </summary>
        public string DefaultSourceItem { get; set; }
        /// <summary>
        /// Indicates if the room supports advanced sharing
        /// </summary>
        [JsonProperty("supportsAdvancedSharing")]
        /// <summary>
        /// Gets or sets the SupportsAdvancedSharing
        /// </summary>
        public bool SupportsAdvancedSharing { get; set; }
        /// <summary>
        /// Indicates if non-tech users can change the share mode
        /// </summary>
        [JsonProperty("userCanChangeShareMode")]
        /// <summary>
        /// Gets or sets the UserCanChangeShareMode
        /// </summary>
        public bool UserCanChangeShareMode { get; set; }


        [JsonProperty("matrixRoutingKey", NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// Gets or sets the MatrixRoutingKey
        /// </summary>
        public string MatrixRoutingKey { get; set; }
    }

    public class EssentialsConferenceRoomPropertiesConfig : EssentialsAvRoomPropertiesConfig
    {
        [JsonProperty("videoCodecKey")]
        /// <summary>
        /// Gets or sets the VideoCodecKey
        /// </summary>
        public string VideoCodecKey { get; set; }
        [JsonProperty("audioCodecKey")]
        /// <summary>
        /// Gets or sets the AudioCodecKey
        /// </summary>
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

        [JsonProperty("deviceKeys")]
        /// <summary>
        /// Gets or sets the DeviceKeys
        /// </summary>
        public List<string> DeviceKeys { get; set; }

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

        [JsonProperty("ipId")]
        /// <summary>
        /// Gets or sets the IpId
        /// </summary>
        public string IpId { get; set; }

        [JsonProperty("joinMapKey")]
        /// <summary>
        /// Gets or sets the JoinMapKey
        /// </summary>
        public string JoinMapKey { get; set; }

    }

    /// <summary>
    /// Represents a EssentialsRoomMicrophonePrivacyConfig
    /// </summary>
    public class EssentialsRoomMicrophonePrivacyConfig
    {
		[JsonProperty("deviceKey")]
  /// <summary>
  /// Gets or sets the DeviceKey
  /// </summary>
		public string DeviceKey { get; set; }

		[JsonProperty("behaviour")]
  /// <summary>
  /// Gets or sets the Behaviour
  /// </summary>
		public string Behaviour { get; set; }
    }

    /// <summary>
    /// Represents a EssentialsHelpPropertiesConfig
    /// </summary>
    public class EssentialsHelpPropertiesConfig
    {
		[JsonProperty("message")]
  /// <summary>
  /// Gets or sets the Message
  /// </summary>
		public string Message { get; set; }

		[JsonProperty("showCallButton")]
		public bool ShowCallButton { get; set; }
        
		/// <summary>
        /// Defaults to "Call Help Desk"
        /// </summary>
		[JsonProperty("callButtonText")]
  /// <summary>
  /// Gets or sets the CallButtonText
  /// </summary>
		public string CallButtonText { get; set; }

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
		[JsonProperty("enable")]
  /// <summary>
  /// Gets or sets the Enable
  /// </summary>
		public bool Enable { get; set; }
    }

    public class EssentialsRoomAddressPropertiesConfig
    {
		[JsonProperty("phoneNumber")]
		public string PhoneNumber { get; set; }

		[JsonProperty("sipAddress")]
  /// <summary>
  /// Gets or sets the SipAddress
  /// </summary>
		public string SipAddress { get; set; }
    }


    /// <summary>
    /// Represents a EssentialsLogoPropertiesConfig
    /// </summary>
    public class EssentialsLogoPropertiesConfig
    {
		[JsonProperty("type")]
  /// <summary>
  /// Gets or sets the Type
  /// </summary>
		public string Type { get; set; }

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
		[JsonProperty("deviceKey")]
  /// <summary>
  /// Gets or sets the DeviceKey
  /// </summary>
		public string DeviceKey { get; set; }

		[JsonProperty("timeoutMinutes")]
		public int TimeoutMinutes { get; set; }
    }

	public class EssentialsRoomTechConfig
	{
		[JsonProperty("password")]
  /// <summary>
  /// Gets or sets the Password
  /// </summary>
		public string Password { get; set; }
	}
}