using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Essentials.Core.Bridges;


namespace PepperDash.Essentials.Core.Fusion
{
    public class FusionRoomJoinMap : JoinMapBaseAdvanced
    {

        // Processor Attributes
        [JoinName("ProcessorIp1")]
        public JoinDataComplete ProcessorIp1 = new JoinDataComplete(new JoinData { JoinNumber = 50, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor IP Address 1", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorIp2")]
        public JoinDataComplete ProcessorIp2 = new JoinDataComplete(new JoinData { JoinNumber = 51, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor IP Address 2", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorGateway")]
        public JoinDataComplete ProcessorGateway = new JoinDataComplete(new JoinData { JoinNumber = 52, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Gateway Address", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorHostname")]
        public JoinDataComplete ProcessorHostname = new JoinDataComplete(new JoinData { JoinNumber = 53, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Hostname", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorDomain")]
        public JoinDataComplete ProcessorDomain = new JoinDataComplete(new JoinData { JoinNumber = 55, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Domain", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorDns1")]
        public JoinDataComplete ProcessorDns1 = new JoinDataComplete(new JoinData { JoinNumber = 55, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor DNS 1", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorDns2")]
        public JoinDataComplete ProcessorDns2 = new JoinDataComplete(new JoinData { JoinNumber = 56, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor DNS 2", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorMac1")]
        public JoinDataComplete ProcessorMac1 = new JoinDataComplete(new JoinData { JoinNumber = 57, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor MAC Address 1", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorMac2")]
        public JoinDataComplete ProcessorMac2 = new JoinDataComplete(new JoinData { JoinNumber = 58, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor MAC Address 2", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorNetMask1")]
        public JoinDataComplete ProcessorNetMask1 = new JoinDataComplete(new JoinData { JoinNumber = 59, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor NetMask Address 1", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorNetMask2")]
        public JoinDataComplete ProcessorNetMask2 = new JoinDataComplete(new JoinData { JoinNumber = 60, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor NetMask Address 2", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorFirmware")]
        public JoinDataComplete ProcessorFirmware = new JoinDataComplete(new JoinData { JoinNumber = 61, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Firmware Version", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProgramNameStart")]
        public JoinDataComplete ProgramNameStart = new JoinDataComplete(new JoinData { JoinNumber = 62, JoinSpan = 10 },
            new JoinMetadata { Description = "Program Names", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorReboot")]
        public JoinDataComplete ProcessorReboot = new JoinDataComplete(new JoinData { JoinNumber = 74, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Reboot", JoinCapabilities = eJoinCapabilities.FromFusion, JoinType = eJoinType.Digital });


        // Source Attributes 
        [JoinName("CurrentRoomSourceName")]
        public JoinDataComplete CurrentRoomSourceName = new JoinDataComplete(new JoinData { JoinNumber = 84, JoinSpan = 1 },
            new JoinMetadata { Description = "Current Room Source Name", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("TouchpanelOnlineStart")]
        public JoinDataComplete TouchpanelOnlineStart = new JoinDataComplete(new JoinData { JoinNumber = 150, JoinSpan = 10 },
            new JoinMetadata { Description = "Touchpanel Online Start", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Digital });

        [JoinName("XpanelOnlineStart")]
        public JoinDataComplete XpanelOnlineStart = new JoinDataComplete(new JoinData { JoinNumber = 160, JoinSpan = 10 },
            new JoinMetadata { Description = "Xpanel Online Start", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Digital });

        [JoinName("DisplayOnlineStart")]
        public JoinDataComplete DisplayOnlineStart = new JoinDataComplete(new JoinData { JoinNumber = 170, JoinSpan = 10 },
            new JoinMetadata { Description = "Display Online Start", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Digital });

        [JoinName("Display1LaptopSourceStart")]
        public JoinDataComplete Display1LaptopSourceStart = new JoinDataComplete(new JoinData { JoinNumber = 166, JoinSpan = 10 },
            new JoinMetadata { Description = "Display 1 - Source Laptop Start", JoinCapabilities = eJoinCapabilities.ToFromFusion, JoinType = eJoinType.Digital });
        
        [JoinName("Display1DiscPlayerSourceStart")]
        public JoinDataComplete Display1DiscPlayerSourceStart = new JoinDataComplete(new JoinData { JoinNumber = 181, JoinSpan = 5 },
            new JoinMetadata { Description = "Display 1 - Source Disc Player Start", JoinCapabilities = eJoinCapabilities.ToFromFusion, JoinType = eJoinType.Digital });

        [JoinName("Display1SetTopBoxSourceStart")]
        public JoinDataComplete Display1SetTopBoxSourceStart = new JoinDataComplete(new JoinData { JoinNumber = 188, JoinSpan = 5 },
            new JoinMetadata { Description = "Display 1 - Source TV Start", JoinCapabilities = eJoinCapabilities.ToFromFusion, JoinType = eJoinType.Digital });


        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public FusionRoomJoinMap(uint joinStart)
            : base(joinStart, typeof(FusionRoomJoinMap))
        {

        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        public FusionRoomJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }   
}