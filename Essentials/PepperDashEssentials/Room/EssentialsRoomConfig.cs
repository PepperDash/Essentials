using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.DM;

namespace PepperDash.Essentials
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
                huddle.SourceListKey = props.SourceListKey;
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

#warning Will need to define audio routing somewhere as well
                
                var presRoom = new EssentialsPresentationRoom(Key, Name, displaysDict, masterVolumeControlDev, props);
                return presRoom;
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
        public EssentialsRoomOccSensorConfig OccupancySensors { get; set; }
	}

    /// <summary>
    /// Represents occupancy sensor(s) setup for a room
    /// </summary>
    public class EssentialsRoomOccSensorConfig
    {
        public string Mode { get; set; }
        public List<string> Types { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EssentialsHuddleRoomPropertiesConfig : EssentialsRoomPropertiesConfig
    {
        public string DefaultDisplayKey { get; set; }
        public string DefaultAudioKey { get; set; }
        public string SourceListKey { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EssentialsPresentationRoomPropertiesConfig : EssentialsRoomPropertiesConfig
    {
        public string DefaultAudioBehavior { get; set; }
        public string DefaultAudioKey { get; set; }
        public string DefaultVideoBehavior { get; set; }
        public List<string> DisplayKeys { get; set; }
        public string SourceListKey { get; set; }
        public bool HasDsp { get; set; }
        public EssentialsPresentationVolumesConfig Volumes { get; set; }

        public EssentialsPresentationRoomPropertiesConfig()
        {
            DisplayKeys = new List<string>();
        }
    }    
}