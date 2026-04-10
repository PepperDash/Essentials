using System.Collections.Generic;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
 /// <summary>
 /// Represents a SimplRoomPropertiesConfig
 /// </summary>
	public class SimplRoomPropertiesConfig : EssentialsHuddleVtc1PropertiesConfig
	{
        /// <summary>
        /// Gets or sets the RoomPhoneNumber
        /// </summary>
		[JsonProperty("roomPhoneNumber")]
		public string RoomPhoneNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the RoomURI
        /// </summary>
		[JsonProperty("roomURI")]
		public string RoomURI { get; set; }
        
        /// <summary>
        /// Gets or sets the SpeedDials
        /// </summary>
		[JsonProperty("speedDials")]
		public List<SimplSpeedDial> SpeedDials { get; set; }
        
        /// <summary>
        /// Gets or sets the VolumeSliderNames
        /// </summary>
		[JsonProperty("volumeSliderNames")]
		public List<string> VolumeSliderNames { get; set; }
	}

    /// <summary>
    /// Represents a SimplSpeedDial
    /// </summary>
	public class SimplSpeedDial
	{
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the Number
        /// </summary>
		[JsonProperty("number")]
		public string Number { get; set; }
	}
}