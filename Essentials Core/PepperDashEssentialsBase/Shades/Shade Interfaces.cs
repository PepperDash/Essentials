using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Shades
{
    public interface IShades
    {
        List<ShadeBase> Shades { get; }
    }

    /// <summary>
    /// Requirements for a device that implements basic Open/Close shade control
    /// </summary>
    public interface IShadesOpenClose
    {
        void Open();
        void Close();
    }

    /// <summary>
    /// Requirements for a device that implements basic Open/Close/Stop shade control
    /// </summary>
    public interface IShadesOpenCloseStop : IShadesOpenClose
    {
        void Stop();
    }

    /// <summary>
    /// Requirements for a shade device that provides open/closed feedback
    /// </summary>
    public interface iShadesRaiseLowerFeedback
    {
        BoolFeedback ShadeIsOpenFeedback { get; }
        BoolFeedback ShadeIsClosedFeedback { get; }
    }

}