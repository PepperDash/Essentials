using PepperDash.Core;

namespace PepperDash.Essentials.Devices.Common
{
    /// <summary>
    /// Represents a SetTopBoxPropertiesConfig
    /// </summary>
    public class SetTopBoxPropertiesConfig : PepperDash.Essentials.Core.Config.SourceDevicePropertiesConfigBase
    {
        /// <summary>
        /// Gets or sets the HasPresets
        /// </summary>
        public bool HasPresets { get; set; }
        /// <summary>
        /// Gets or sets the HasDvr
        /// </summary>
        public bool HasDvr { get; set; }
        /// <summary>
        /// Gets or sets the HasDpad
        /// </summary>
        public bool HasDpad { get; set; }
        /// <summary>
        /// Gets or sets the HasNumeric
        /// </summary>
        public bool HasNumeric { get; set; }
        /// <summary>
        /// Gets or sets the IrPulseTime
        /// </summary>
        public int IrPulseTime { get; set; }

        /// <summary>
        /// Gets or sets the Control
        /// </summary>
        public ControlPropertiesConfig Control { get; set; }
    }
}