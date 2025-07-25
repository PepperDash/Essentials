using System;

namespace PepperDash.Core.WebApi.Presets
{
 /// <summary>
 /// Represents a Preset
 /// </summary>
	public class Preset
	{
        /// <summary>
        /// ID of preset
        /// </summary>
		public int Id { get; set; }

  /// <summary>
  /// Gets or sets the UserId
  /// </summary>
		public int UserId { get; set; }

  /// <summary>
  /// Gets or sets the RoomTypeId
  /// </summary>
		public int RoomTypeId { get; set; }

  /// <summary>
  /// Gets or sets the PresetName
  /// </summary>
		public string PresetName { get; set; }

  /// <summary>
  /// Gets or sets the PresetNumber
  /// </summary>
		public int PresetNumber { get; set; }

  /// <summary>
  /// Gets or sets the Data
  /// </summary>
		public string Data { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
		public Preset()
		{
			PresetName = "";
			PresetNumber = 1;
			Data = "{}";
		}
	}

 /// <summary>
 /// Represents a PresetReceivedEventArgs
 /// </summary>
	public class PresetReceivedEventArgs : EventArgs
	{
        /// <summary>
        /// True when the preset is found
        /// </summary>
        public bool LookupSuccess { get; private set; }
        
        /// <summary>
        /// Gets or sets the ULookupSuccess
        /// </summary>
        public ushort ULookupSuccess { get { return (ushort)(LookupSuccess ? 1 : 0); } }

        /// <summary>
        /// Gets or sets the Preset
        /// </summary>
        public Preset Preset { get; private set; }

		/// <summary>
		/// For Simpl+
		/// </summary>
		public PresetReceivedEventArgs() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="preset"></param>
        /// <param name="success"></param>
		public PresetReceivedEventArgs(Preset preset, bool success)
		{
            LookupSuccess = success;
			Preset = preset;
		}
	}
}