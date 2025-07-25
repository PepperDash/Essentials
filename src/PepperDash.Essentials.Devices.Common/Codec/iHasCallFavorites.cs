using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    /// <summary>
    /// Defines the contract for IHasCallFavorites
    /// </summary>
    public interface IHasCallFavorites
    {
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

        public CodecCallFavorites()
        {
            Favorites = new List<CodecActiveCallItem>();
        }
    }
}