using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
	/// <summary>
	/// 
	/// </summary>
	public enum eSourceListItemType
	{
		Route, Off, Other, SomethingAwesomerThanThese
	}

	/// <summary>
	/// Represents an item in a source list - can be deserialized into.
	/// </summary>
	public class SourceListItem
	{
		public string SourceKey { get; set; }
		/// <summary>
		/// Returns the source Device for this, if it exists in DeviceManager
		/// </summary>
		[JsonIgnore]
		public Device SourceDevice
		{
			get
			{
				if (_SourceDevice == null)
					_SourceDevice = DeviceManager.GetDeviceForKey(SourceKey) as Device;
				return _SourceDevice;
			}
		}
		Device _SourceDevice;

		/// <summary>
		/// Gets either the source's Name or this AlternateName property, if 
		/// defined.  If source doesn't exist, returns "Missing source"
		/// </summary>
		public string PreferredName
		{
			get
			{
				if (string.IsNullOrEmpty(Name))
				{
					if (SourceDevice == null)
						return "---";
					return SourceDevice.Name;
				}
				return Name;
			}
		}

		/// <summary>
		/// A name that will override the source's name on the UI
		/// </summary>
		public string Name { get; set; }
		public string Icon { get; set; }
		public string AltIcon { get; set; }
		public bool IncludeInSourceList { get; set; }
        public int Order { get; set; }
		public string VolumeControlKey { get; set; }
		public eSourceListItemType Type { get; set; }
		public List<SourceRouteListItem> RouteList { get; set; }
		public bool DisableCodecSharing { get; set; }
		public bool DisableRoutedSharing { get; set; }

		public SourceListItem()
		{
			Icon = "Blank";
		}
	}

	public class SourceRouteListItem
	{
		[JsonProperty("sourceKey")]
		public string SourceKey { get; set; }

		[JsonProperty("destinationKey")]
		public string DestinationKey { get; set; }

		[JsonProperty("type")]
		public eRoutingSignalType Type { get; set; }
	}
}