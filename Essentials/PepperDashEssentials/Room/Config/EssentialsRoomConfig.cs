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
	public class EssentialsRoomConfig : DeviceConfig
	{
		/// <summary>
		/// Returns a room object from this config data
		/// </summary>
		/// <returns></returns>
		public Device GetRoomObject()
		{
			var typeName = Type.ToLower();
			if (typeName == "huddle")
			{
                var props = JsonConvert.DeserializeObject<EssentialsHuddleRoomPropertiesConfig>
                    (this.Properties.ToString());
				var disp = DeviceManager.GetDeviceForKey(props.DefaultDisplayKey) as IRoutingSinkWithSwitching;
				var audio = DeviceManager.GetDeviceForKey(props.DefaultAudioKey) as IRoutingSinkNoSwitching;
				var huddle = new EssentialsHuddleSpaceRoom(Key, Name, disp, audio, props);
                huddle.LogoUrl = props.Logo.GetUrl();
                huddle.SourceListKey = props.SourceListKey;
                huddle.DefaultSourceItem = props.DefaultSourceItem;
                huddle.DefaultVolume = (ushort)(props.Volumes.Master.Level * 65535 / 100);
                return huddle;
			}
			else if (typeName == "presentation")
			{
                var props = JsonConvert.DeserializeObject<EssentialsPresentationRoomPropertiesConfig>
                    (this.Properties.ToString());
                var displaysDict = new Dictionary<uint, IRoutingSinkNoSwitching>();
                uint i = 1;
                foreach (var dispKey in props.DisplayKeys) // read in the ordered displays list
                {
                    var disp = DeviceManager.GetDeviceForKey(dispKey) as IRoutingSinkWithSwitching;
                    displaysDict.Add(i++, disp);
                }

                // Get the master volume control
                IBasicVolumeWithFeedback masterVolumeControlDev = props.Volumes.Master.GetDevice();

                
                var presRoom = new EssentialsPresentationRoom(Key, Name, displaysDict, masterVolumeControlDev, props);
                return presRoom;
            }
            else if (typeName == "huddlevtc1")
            {
                var props = JsonConvert.DeserializeObject<EssentialsHuddleVtc1PropertiesConfig>
                    (this.Properties.ToString());
                var disp = DeviceManager.GetDeviceForKey(props.DefaultDisplayKey) as IRoutingSinkWithSwitching;

                var codec = DeviceManager.GetDeviceForKey(props.VideoCodecKey) as
                    PepperDash.Essentials.Devices.Common.VideoCodec.VideoCodecBase;

                var rm = new EssentialsHuddleVtc1Room(Key, Name, disp, codec, codec, props);
                rm.LogoUrl = props.Logo.GetUrl();
                rm.SourceListKey = props.SourceListKey;
                rm.DefaultSourceItem = props.DefaultSourceItem;
                rm.DefaultVolume = (ushort)(props.Volumes.Master.Level * 65535 / 100);

                return rm;
            }

            return null;
		}
	}

    /// <summary>
    /// 
    /// </summary>
	public class EssentialsRoomPropertiesConfig
	{
		public string HelpMessage { get; set; }
        public string Description { get; set; }
        public int ShutdownVacancySeconds { get; set; }
        public int ShutdownPromptSeconds { get; set; }
        public EssentialsHelpPropertiesConfig Help { get; set; }
        public EssentialsOneButtonMeetingPropertiesConfig OneButtonMeeting { get; set; }
        public EssentialsRoomAddressPropertiesConfig Addresses { get; set; }
        public EssentialsRoomOccSensorConfig OccupancySensors { get; set; }
        public EssentialsLogoPropertiesConfig Logo { get; set; }
        public EssentialsRoomVolumesConfig Volumes { get; set; }
	}

    /// <summary>
    /// Properties for the help text box
    /// </summary>
    public class EssentialsHelpPropertiesConfig
    {
        public string Message { get; set; }
        public bool ShowCallButton { get; set; }
        /// <summary>
        /// Defaults to "Call Help Desk"
        /// </summary>
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
        public bool Enable { get; set; }
    }

    public class EssentialsRoomAddressPropertiesConfig
    {
        public string PhoneNumber { get; set; }
        public string SipAddress { get; set; }
    }


    /// <summary>
    /// Properties for the room's logo on panels
    /// </summary>
    public class EssentialsLogoPropertiesConfig
    {
        public string Type { get; set; }
        public string Url { get; set; }
        /// <summary>
        /// Gets either the custom URL, a local-to-processor URL, or null if it's a default logo
        /// </summary>
        public string GetUrl()
        {
            if (Type == "url")
                return Url;
            if (Type == "system")
                return string.Format("http://{0}:8080/logo.png", 
                    CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0));
            return null;
        }
    }

    /// <summary>
    /// Represents occupancy sensor(s) setup for a room
    /// </summary>
    public class EssentialsRoomOccSensorConfig
    {
        public string Mode { get; set; }
        public List<string> Types { get; set; }
    }

}