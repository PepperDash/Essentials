using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Shades
{
    public interface IShades
    {
        List<ShadeBase> Shades { get; }
    }

    /// <summary>
    /// Requirements for a device that implements basic shade control
    /// </summary>
    public interface iShadesRaiseLower
    {
        void Open();
        void Stop();
        void Close();
    }
}