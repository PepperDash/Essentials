using System.Collections.Generic;
using Newtonsoft.Json;

namespace PepperDash.Essentials.Core.Rooms.Config
{
 /// <summary>
 /// Represents a SimplRoomPropertiesConfig
 /// </summary>
	public class SimplRoomPropertiesConfig : EssentialsHuddleVtc1PropertiesConfig
	{
		[JsonProperty("roomPhoneNumber")]
  /// <summary>
  /// Gets or sets the RoomPhoneNumber
  /// </summary>
		public string RoomPhoneNumber { get; set; }
		[JsonProperty("roomURI")]
  /// <summary>
  /// Gets or sets the RoomURI
  /// </summary>
		public string RoomURI { get; set; }
		[JsonProperty("speedDials")]
  /// <summary>
  /// Gets or sets the SpeedDials
  /// </summary>
		public List<SimplSpeedDial> SpeedDials { get; set; }
		[JsonProperty("volumeSliderNames")]
  /// <summary>
  /// Gets or sets the VolumeSliderNames
  /// </summary>
		public List<string> VolumeSliderNames { get; set; }
	}

 /// <summary>
 /// Represents a SimplSpeedDial
 /// </summary>
	public class SimplSpeedDial
	{
		[JsonProperty("name")]
  /// <summary>
  /// Gets or sets the Name
  /// </summary>
		public string Name { get; set; }
		[JsonProperty("number")]
  /// <summary>
  /// Gets or sets the Number
  /// </summary>
		public string Number { get; set; }
	}
}