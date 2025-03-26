using System;

namespace PepperDash.Core.WebApi.Presets
{
    /// <summary>
    /// Represents a preset
    /// </summary>
	public class Preset
	{
        /// <summary>
        /// ID of preset
        /// </summary>
		public int Id { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
		public int UserId { get; set; }

        /// <summary>
        /// Room Type ID
        /// </summary>
		public int RoomTypeId { get; set; }

        /// <summary>
        /// Preset Name
        /// </summary>
		public string PresetName { get; set; }

        /// <summary>
        /// Preset Number
        /// </summary>
		public int PresetNumber { get; set; }

        /// <summary>
        /// Preset Data
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
	/// 
	/// </summary>
	public class PresetReceivedEventArgs : EventArgs
	{
        /// <summary>
        /// True when the preset is found
        /// </summary>
        public bool LookupSuccess { get; private set; }
        
        /// <summary>
        /// S+ helper
        /// </summary>
        public ushort ULookupSuccess { get { return (ushort)(LookupSuccess ? 1 : 0); } }

        /// <summary>
        /// The preset
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