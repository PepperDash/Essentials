using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec;

public interface IHasCallFavorites
{
    CodecCallFavorites CallFavorites { get; }
}

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