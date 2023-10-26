using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public interface IHasCallFavorites
    {
        CodecCallFavorites CallFavorites { get; }
    }
}