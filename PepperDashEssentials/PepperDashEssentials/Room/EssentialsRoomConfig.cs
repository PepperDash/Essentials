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

                // Need to assign the volume control point and also audio routing endpoint, if routing
                // is required: For DSP, typically no. 
 
                IBasicVolumeWithFeedback masterVolumeControlDev = null;
                if (props.Volumes.ContainsKey("master"))
                {
                    var audioConfig = props.Volumes["master"];
                    // need to either get a device or drill down into a device for a card or port

                    // Check for DM output port format
                    var match = Regex.Match(audioConfig.DeviceKey, @"([-_\w]+)--(\w+)~(\d+)");
                    if(match.Success)
                    {
                        var devKey = match.Groups[1].Value;
                        var chassis = DeviceManager.GetDeviceForKey(devKey) as DmChassisController;
                        if (chassis != null)
                        {
                            var outputNum = Convert.ToUInt32(match.Groups[3].Value);
                            if (chassis.VolumeControls.ContainsKey(outputNum)) // should always...
                            {
                                masterVolumeControlDev = chassis.VolumeControls[outputNum];
                                Debug.Console(2, "Setting '{0}' as master volume control on room", audioConfig.DeviceKey);
                            }
                        }
                    }
                }

                var presRoom = new EssentialsPresentationRoom(Key, Name, displaysDict, masterVolumeControlDev, props);
                return presRoom;
            }
            return null;
		}
	}

	public class EssentialsRoomPropertiesConfig
	{
		public string HelpMessage { get; set; }
        public string Description { get; set; }
	}

    public class EssentialsHuddleRoomPropertiesConfig : EssentialsRoomPropertiesConfig
    {
        public string DefaultDisplayKey { get; set; }
        public string DefaultAudioKey { get; set; }
        public string SourceListKey { get; set; }
    }

    public class EssentialsPresentationRoomPropertiesConfig : EssentialsRoomPropertiesConfig
    {
        public string DefaultAudioBehavior { get; set; }
        public string DefaultAudioKey { get; set; }
        public string DefaultVideoBehavior { get; set; }
        public List<string> DisplayKeys { get; set; }
        public string SourceListKey { get; set; }
        public Dictionary<string, EssentialsVolumeLevelConfig> Volumes { get; set; }

        public EssentialsPresentationRoomPropertiesConfig()
        {
            DisplayKeys = new List<string>();
            Volumes = new Dictionary<string, EssentialsVolumeLevelConfig>();
        }
    }

    public class EssentialsVolumeLevelConfig
    {
        public string DeviceKey { get; set; }
        public string Label { get; set; }
        public int Level { get; set; }
    }
}