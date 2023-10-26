using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Shades
{
	/// <summary>
	/// Requirements for an object that contains shades
	/// </summary>
    public interface IShades
    {
        List<ShadeBase> Shades { get; }
    }
}