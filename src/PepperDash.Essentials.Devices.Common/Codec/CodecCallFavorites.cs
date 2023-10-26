using System.Collections.Generic;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Represents favorites entries for a codec device
    /// </summary>
    public class CodecCallFavorites
    {
        public List<CodecActiveCallItem> Favorites { get; set; }

        public CodecCallFavorites()
        {
            Favorites = new List<CodecActiveCallItem>();
        }
    }
}