extern alias Full;
using System;
using Full::Newtonsoft.Json;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Represents an item in the directory
    /// </summary>
    public class DirectoryItem : ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        [JsonProperty("folderId")]
        public string FolderId { get; set; }

        [JsonProperty("name")]	
        public string Name { get; set; }

        [JsonProperty("parentFolderId")]
        public string ParentFolderId { get; set; }
    }
}