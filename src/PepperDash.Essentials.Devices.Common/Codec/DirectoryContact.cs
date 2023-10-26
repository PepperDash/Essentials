extern alias Full;
using System.Collections.Generic;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Represents a contact type DirectoryItem
    /// </summary>
    public class DirectoryContact : DirectoryItem
    {
        [JsonProperty("contactId")]
        public string ContactId { get; set; } 

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("contactMethods")]
        public List<ContactMethod> ContactMethods { get; set; }

        public DirectoryContact()
        {
            ContactMethods = new List<ContactMethod>();
        }
    }
}