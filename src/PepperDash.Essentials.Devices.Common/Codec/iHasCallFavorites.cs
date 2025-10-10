using System.Collections.Generic;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Defines the contract for IHasCallFavorites
    /// </summary>
    public interface IHasCallFavorites
    {
        /// <summary>
        /// Gets the call favorites for this device
        /// </summary>
        CodecCallFavorites CallFavorites { get; }
    }

    /// <summary>
    /// Represents a CodecCallFavorites
    /// </summary>
    public class CodecCallFavorites
    {
        /// <summary>
        /// Gets or sets the Favorites
        /// </summary>
        public List<CodecActiveCallItem> Favorites { get; set; }

        /// <summary>
        /// Initializes a new instance of the CodecCallFavorites class
        /// </summary>
        public CodecCallFavorites()
        {
            Favorites = new List<CodecActiveCallItem>();
        }
    }
}