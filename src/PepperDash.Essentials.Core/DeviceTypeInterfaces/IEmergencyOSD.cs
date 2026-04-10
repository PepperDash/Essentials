using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Core.DeviceTypeInterfaces
{
    /// <summary>
    /// Defines the contract for IEmergencyOSD
    /// </summary>
    public interface IEmergencyOSD
    {
        /// <summary>
        /// Shows an emergency message on the OSD
        /// </summary>
        /// <param name="url">The URL of the emergency message to display</param>
        void ShowEmergencyMessage(string url);

        /// <summary>
        /// Hides the emergency message from the OSD
        /// </summary>
        void HideEmergencyMessage();
    }
}
