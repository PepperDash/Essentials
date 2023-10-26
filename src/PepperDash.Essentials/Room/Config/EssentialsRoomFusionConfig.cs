extern alias Full;
using System;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Room.Config
{
    public class EssentialsRoomFusionConfig
    {
        public uint IpIdInt
        {
            get
            {
                try 
                {
                    return Convert.ToUInt32(IpId, 16);
                }
                catch (Exception)
                {
                    throw new FormatException(string.Format("ERROR:Unable to convert IP ID: {0} to hex.  Error:\n{1}", IpId));
                }
          
            }
        }

        [JsonProperty("ipId")]
        public string IpId { get; set; }

        [JsonProperty("joinMapKey")]
        public string JoinMapKey { get; set; }

    }
}