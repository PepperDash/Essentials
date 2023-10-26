extern alias Full;
using System;
using Crestron.SimplSharpPro;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class EssentialsControlPropertiesConfig : 
        PepperDash.Core.ControlPropertiesConfig
    {

        [JsonConverter(typeof(ComSpecJsonConverter))]
        public ComPort.ComPortSpec ComParams { get; set; }

        public string RoomId { get; set; }

        public string CresnetId { get; set; }

        /// <summary>
        /// Attempts to provide uint conversion of string CresnetId
        /// </summary>
        public uint CresnetIdInt
        {
            get
            {
                try 
                {
                    return Convert.ToUInt32(CresnetId, 16);
                }
                catch (Exception)
                {
                    throw new FormatException(string.Format("ERROR:Unable to convert Cresnet ID: {0} to hex.  Error:\n{1}", CresnetId));
                }
            }
        }

        public string InfinetId { get; set; }

        /// <summary>
        /// Attepmts to provide uiont conversion of string InifinetId
        /// </summary>
        public uint InfinetIdInt
        {
            get
            {
                try
                {
                    return Convert.ToUInt32(InfinetId, 16);
                }
                catch (Exception)
                {
                    throw new FormatException(string.Format("ERROR:Unable to conver Infinet ID: {0} to hex.  Error:\n{1}", InfinetId));
                }
            }
        }
    }
}