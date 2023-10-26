using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Lighting;

namespace PepperDash.Essentials.Devices.Common.Environment.Lutron
{
    public class LutronQuantumPropertiesConfig
    {
        public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }
        public ControlPropertiesConfig Control { get; set; }

        public string IntegrationId { get; set; }
        public List<LightingScene> Scenes { get; set; }

        // Moved to use existing properties in Control object
        // public string Username { get; set; } 
        // public string Password { get; set; }
    }
}