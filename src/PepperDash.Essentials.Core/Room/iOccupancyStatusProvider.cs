using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Core;

public interface IOccupancyStatusProvider
{
    BoolFeedback RoomIsOccupiedFeedback { get; }
}