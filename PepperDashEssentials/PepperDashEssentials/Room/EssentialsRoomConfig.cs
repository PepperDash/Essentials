using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

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
			EssentialsHuddleSpaceRoom room = null;
			if (typeName == "huddle")
			{
                var props = JsonConvert.DeserializeObject<EssentialsHuddleRoomPropertiesConfig>
                    (this.Properties.ToString());
				var disp = DeviceManager.GetDeviceForKey(props.DefaultDisplayKey) as IRoutingSinkWithSwitching;
				var audio = DeviceManager.GetDeviceForKey(props.DefaultAudioKey) as IRoutingSinkNoSwitching;
				room = new EssentialsHuddleSpaceRoom(Key, Name, disp, audio, props);
				room.SourceListKey = props.SourceListKey;
			}
			else if (typeName == "presentation")
			{
                var props = JsonConvert.DeserializeObject<EssentialsPresentationRoomPropertiesConfig>
                    (this.Properties.ToString());
                // assign displays

                // assign audio. How??????

            }
			return room;
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

    public class EssentialsPresentationRoomPropertiesConfig
    {
        public string DefaultAudioBehavior { get; set; }
        public string DefaultAudioKey { get; set; }
        public string DefaultVideoBehavior { get; set; }
        public List<string> DisplayKeys { get; set; }
        public string SourceListKey { get; set; }
        public Dictionary<string, EssentialsVolumeLevelConfig> Volumes { get; set; }
    }

    public class EssentialsVolumeLevelConfig
    {
        public string DeviceKey { get; set; }
        public string Label { get; set; }
        public int Level { get; set; }
    }
}