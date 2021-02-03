using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core.DeviceTypeInterfaces;

namespace PepperDash.Essentials.Core.Interfaces.Room
{
    public interface IHasMobileControl
    {
        /// <summary>
        /// Indicates if this room is Mobile Control Enabled
        /// </summary>
        bool IsMobileControlEnabled { get; }

        /// <summary>
        /// The bridge for this room if Mobile Control is enabled
        /// </summary>
        IMobileControlRoomBridge MobileControlRoomBridge { get; }

    }
}