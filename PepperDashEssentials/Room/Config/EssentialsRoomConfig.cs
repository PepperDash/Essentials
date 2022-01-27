using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crestron.SimplSharp;
using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Room.Config
{
	public class EssentialsRoomConfigHelper
	{
		/// <summary>
		/// Returns a room object from this config data
		/// </summary>
		/// <returns></returns>
		public static Device GetRoomObject(DeviceConfig roomConfig)
		{
			var typeName = roomConfig.Type.ToLower();

			if (typeName == "huddle")
			{
                return new EssentialsHuddleSpaceRoom(roomConfig);
			}
		    if (typeName == "huddlevtc1")
		    {
		        return new EssentialsHuddleVtc1Room(roomConfig);
		    }
		    if (typeName == "ddvc01bridge")
		    {
		        return new Device(roomConfig.Key, roomConfig.Name); // placeholder device that does nothing.
		    }
		    if (typeName == "dualdisplay")
		    {
		        return new EssentialsDualDisplayRoom(roomConfig);
		    }
            if (typeName == "combinedhuddlevtc1")
            {
                return new EssentialsCombinedHuddleVtc1Room(roomConfig);
            }

		    return typeName != "techroom" ? null : new EssentialsTechRoom(roomConfig);
		}

        /// <summary>
        /// Gets and operating, standalone emergegncy object that can be plugged into a room.
        /// Returns null if there is no emergency defined
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
            }
            return null;
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="props"></param>
		/// <param name="room"></param>
		/// <returns></returns>
		public static Core.Privacy.MicrophonePrivacyController GetMicrophonePrivacy(
			EssentialsRoomPropertiesConfig props, IPrivacy room)
		{
			var microphonePrivacy = props.MicrophonePrivacy;
			if (microphonePrivacy == null)
			{
				Debug.Console(0, "Cannot create microphone privacy with null properties");
				return null;
			}
			// Get the MicrophonePrivacy device from the device manager
			var mP = (DeviceManager.GetDeviceForKey(props.MicrophonePrivacy.DeviceKey) as
				Core.Privacy.MicrophonePrivacyController);
			// Set this room as the IPrivacy device
			if (mP == null)
			{
				Debug.Console(0, "ERROR: Selected device {0} is not MicrophonePrivacyController", props.MicrophonePrivacy.DeviceKey);
				return null;
			}
			mP.SetPrivacyDevice(room);

			var behaviour = props.MicrophonePrivacy.Behaviour.ToLower();

			if (behaviour == null)
			{
				Debug.Console(0, "WARNING: No behaviour defined for MicrophonePrivacyController");
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
    /// 
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
		public EssentialsHelpPropertiesConfig Help { get; set; }

		[JsonProperty("helpMessage")]
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
		public EssentialsEnvironmentPropertiesConfig Environment { get; set; }

		[JsonProperty("logo")]
		public EssentialsLogoPropertiesConfig LogoLight { get; set; }

        [JsonProperty("logoDark")]
        public EssentialsLogoPropertiesConfig LogoDark { get; set; }
	
		[JsonProperty("microphonePrivacy")]
		public EssentialsRoomMicrophonePrivacyConfig MicrophonePrivacy { get; set; }

		[JsonProperty("occupancy")]
		public EssentialsRoomOccSensorConfig Occupancy { get; set; }

		[JsonProperty("oneButtonMeeting")]
		public EssentialsOneButtonMeetingPropertiesConfig OneButtonMeeting { get; set; }

		[JsonProperty("shutdownVacancySeconds")]
		public int ShutdownVacancySeconds { get; set; }

		[JsonProperty("shutdownPromptSeconds")]
		public int ShutdownPromptSeconds { get; set; }

		[JsonProperty("tech")]
		public EssentialsRoomTechConfig Tech { get; set; }

		[JsonProperty("volumes")]
		public EssentialsRoomVolumesConfig Volumes { get; set; }

        [JsonProperty("fusion")]
        public EssentialsRoomFusionConfig Fusion { get; set; }

		[JsonProperty("zeroVolumeWhenSwtichingVolumeDevices")]
		public bool ZeroVolumeWhenSwtichingVolumeDevices { get; set; }

        /// <summary>
        /// Indicates if this room represents a combination of other rooms
        /// </summary>
        [JsonProperty("isRoomCombinationScenario")]
        public bool IsRoomCombinationScenario { get; set; }

        public EssentialsRoomPropertiesConfig()
        {
            LogoLight = new EssentialsLogoPropertiesConfig();
            LogoDark = new EssentialsLogoPropertiesConfig();
        }
	}

    public class EssentialsAvRoomPropertiesConfig : EssentialsRoomPropertiesConfig
    {
        [JsonProperty("defaultAudioKey")]
        public string DefaultAudioKey { get; set; }
        [JsonProperty("sourceListKey")]
        public string SourceListKey { get; set; }
        [JsonProperty("destinationListKey")]
        public string DestinationListKey { get; set; }
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
    }

    public class EssentialsConferenceRoomPropertiesConfig : EssentialsAvRoomPropertiesConfig
    {
        [JsonProperty("videoCodecKey")]
        public string VideoCodecKey { get; set; }
        [JsonProperty("audioCodecKey")]
        public string AudioCodecKey { get; set; }
    }

	public class EssentialsEnvironmentPropertiesConfig
	{
		public bool Enabled { get; set; }

        [JsonProperty("deviceKeys")]
        public List<string> DeviceKeys { get; set; }

        public EssentialsEnvironmentPropertiesConfig()
        {
            DeviceKeys = new List<string>();
        }

	}

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
        public string IpId { get; set; }

        [JsonProperty("joinMapKey")]
        public string JoinMapKey { get; set; }

    }

    public class EssentialsRoomMicrophonePrivacyConfig
    {
		[JsonProperty("deviceKey")]
		public string DeviceKey { get; set; }

		[JsonProperty("behaviour")]
		public string Behaviour { get; set; }
    }

    /// <summary>
    /// Properties for the help text box
    /// </summary>
    public class EssentialsHelpPropertiesConfig
    {
		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("showCallButton")]
		public bool ShowCallButton { get; set; }
        
		/// <summary>
        /// Defaults to "Call Help Desk"
        /// </summary>
		[JsonProperty("callButtonText")]
		public string CallButtonText { get; set; }

        public EssentialsHelpPropertiesConfig()
        {
            CallButtonText = "Call Help Desk";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EssentialsOneButtonMeetingPropertiesConfig
    {
		[JsonProperty("enable")]
		public bool Enable { get; set; }
    }

    public class EssentialsRoomAddressPropertiesConfig
    {
		[JsonProperty("phoneNumber")]
		public string PhoneNumber { get; set; }

		[JsonProperty("sipAddress")]
		public string SipAddress { get; set; }
    }


    /// <summary>
    /// Properties for the room's logo on panels
    /// </summary>
    public class EssentialsLogoPropertiesConfig
    {
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }
        /// <summary>
        /// Gets either the custom URL, a local-to-processor URL, or null if it's a default logo
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
    /// Represents occupancy sensor(s) setup for a room
    /// </summary>
    public class EssentialsRoomOccSensorConfig
    {
		[JsonProperty("deviceKey")]
		public string DeviceKey { get; set; }

		[JsonProperty("timeoutMinutes")]
		public int TimeoutMinutes { get; set; }
    }

	public class EssentialsRoomTechConfig
	{
		[JsonProperty("password")]
		public string Password { get; set; }
	}
}