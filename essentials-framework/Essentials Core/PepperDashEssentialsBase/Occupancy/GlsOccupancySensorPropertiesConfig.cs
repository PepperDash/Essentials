using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Defines configuration properties for Crestron GLS series occupancy sensors
    /// </summary>
    public class GlsOccupancySensorPropertiesConfig
    {
        // Single Technology Sensors (PIR): GlsOccupancySensorBase
        [JsonProperty("enablePir")]
        public bool? EnablePir { get; set; }

        [JsonProperty("enableLedFlash")]
        public bool? EnableLedFlash { get; set; }

        [JsonProperty("shortTimeoutState")]
        public bool? ShortTimeoutState { get; set; }

        [JsonProperty("enableRawStates")]
        public bool? EnableRawStates { get; set; }

        [JsonProperty("remoteTimeout")]
        public ushort? RemoteTimeout { get; set; }

        [JsonProperty("internalPhotoSensorMinChange")]
        public ushort? InternalPhotoSensorMinChange { get; set; }

        [JsonProperty("externalPhotoSensorMinChange")]
        public ushort? ExternalPhotoSensorMinChange { get; set; }

        // Dual Technology Sensors: GlsOdtCCn
        [JsonProperty("enableUsA")]
        public bool? EnableUsA { get; set; }

        [JsonProperty("enableUsB")]
        public bool? EnableUsB { get; set; }

        [JsonProperty("orWhenVacatedState")]
        public bool? OrWhenVacatedState { get; set; }

        [JsonProperty("andWhenVacatedState")]
        public bool? AndWhenVacatedState { get; set; }

        // PoE Sensors: CenOdtCPoe

        /// <summary>
        /// Sets the sensitivity level for US while sensor is in occupied state
        /// 1 = low; 2 = medium; 3 = high; 4 = xlow; 5 = 2xlow; 6 = 3xlow
        /// </summary>
        [JsonProperty("usSensitivityOccupied")]
        public ushort? UsSensitivityOccupied { get; set; }

        /// <summary>
        /// Sets the sensitivity level for US while sensor is in vacant state
        /// 1 = low; 2 = medium; 3 = high; 4 = xlow; 5 = 2xlow; 6 = 3xlow
        /// </summary>
        [JsonProperty("usSensitivityVacant")]
        public ushort? UsSensitivityVacant { get; set; }

        /// <summary>
        /// Sets the sensitivity level for PIR while sensor is in occupied state
        /// 1 = low; 2 = medium; 3 = high
        /// </summary>
        [JsonProperty("pirSensitivityOccupied")]
        public ushort? PirSensitivityOccupied { get; set; }

        /// <summary>
        /// Sets the sensitivity level for PIR while sensor is in vacant state
        /// 1 = low; 2 = medium; 3 = high
        /// </summary>
        [JsonProperty("PirSensitivityVacant")]
        public ushort? PirSensitivityVacant { get; set; }
    }
}