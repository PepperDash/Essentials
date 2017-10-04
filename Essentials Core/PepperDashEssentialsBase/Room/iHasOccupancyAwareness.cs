using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.GeneralIO;

namespace PepperDash.Essentials.Core.Room
{
    public interface IHasOccupancyAwareness
    {
        OccupancyStatus RoomOccupancy { get; }

    }

    public class OccupancyStatus
    {
        BoolFeedback RoomIsOccupied { get; }

        
    }


}