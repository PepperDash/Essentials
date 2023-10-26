extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Represents a folder type DirectoryItem
    /// </summary>
    public class DirectoryFolder : DirectoryItem
    {
        [JsonProperty("contacts")]
        public List<DirectoryContact> Contacts { get; set; }


        public DirectoryFolder()
        {
            Contacts = new List<DirectoryContact>();
        }
    }
}