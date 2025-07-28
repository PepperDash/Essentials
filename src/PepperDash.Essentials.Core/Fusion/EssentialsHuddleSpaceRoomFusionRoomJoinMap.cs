using PepperDash.Essentials.Core.JoinMaps;
using System;


namespace PepperDash.Essentials.Core.Fusion
{
    /// <summary>
    /// Represents a EssentialsHuddleSpaceRoomFusionRoomJoinMap
    /// </summary>
    public class EssentialsHuddleSpaceRoomFusionRoomJoinMap : JoinMapBaseAdvanced
    {

        // Processor Attributes
        [JoinName("ProcessorIp1")]
        public JoinDataComplete ProcessorIp1 = new JoinDataComplete(new JoinData { JoinNumber = 50, JoinSpan = 1, AttributeName = "Info - Processor - IP 1" },
            new JoinMetadata { Description = "Info - Processor - IP 1", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorIp2")]
        public JoinDataComplete ProcessorIp2 = new JoinDataComplete(new JoinData { JoinNumber = 51, JoinSpan = 1, AttributeName = "Info - Processor - IP 2" },
            new JoinMetadata { Description = "Info - Processor - IP 2", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorGateway")]
        public JoinDataComplete ProcessorGateway = new JoinDataComplete(new JoinData { JoinNumber = 52, JoinSpan = 1, AttributeName = "Info - Processor - Gateway" },
            new JoinMetadata { Description = "Info - Processor - Gateway", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorHostname")]
        public JoinDataComplete ProcessorHostname = new JoinDataComplete(new JoinData { JoinNumber = 53, JoinSpan = 1, AttributeName = "Info - Processor - Hostname" },
            new JoinMetadata { Description = "Info - Processor - Hostname", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorDomain")]
        public JoinDataComplete ProcessorDomain = new JoinDataComplete(new JoinData { JoinNumber = 54, JoinSpan = 1, AttributeName = "Info - Processor - Domain" },
            new JoinMetadata { Description = "Info - Processor - Domain", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorDns1")]
        public JoinDataComplete ProcessorDns1 = new JoinDataComplete(new JoinData { JoinNumber = 55, JoinSpan = 1, AttributeName = "Info - Processor - DNS 1" },
            new JoinMetadata { Description = "Info - Processor - DNS 1", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorDns2")]
        public JoinDataComplete ProcessorDns2 = new JoinDataComplete(new JoinData { JoinNumber = 56, JoinSpan = 1, AttributeName = "Info - Processor - DNS 2" },
            new JoinMetadata { Description = "Info - Processor - DNS 2", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorMac1")]
        public JoinDataComplete ProcessorMac1 = new JoinDataComplete(new JoinData { JoinNumber = 57, JoinSpan = 1, AttributeName = "Info - Processor - MAC 1" },
            new JoinMetadata { Description = "Info - Processor - MAC 1", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorMac2")]
        public JoinDataComplete ProcessorMac2 = new JoinDataComplete(new JoinData { JoinNumber = 58, JoinSpan = 1, AttributeName = "Info - Processor - MAC 2" },
            new JoinMetadata { Description = "Info - Processor - MAC 2", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorNetMask1")]
        public JoinDataComplete ProcessorNetMask1 = new JoinDataComplete(new JoinData { JoinNumber = 59, JoinSpan = 1, AttributeName = "Info - Processor - Net Mask 1" },
            new JoinMetadata { Description = "Info - Processor - Net Mask 1", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorNetMask2")]
        public JoinDataComplete ProcessorNetMask2 = new JoinDataComplete(new JoinData { JoinNumber = 60, JoinSpan = 1, AttributeName = "Info - Processor - Net Mask 2" },
            new JoinMetadata { Description = "Info - Processor - Net Mask 2", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorFirmware")]
        public JoinDataComplete ProcessorFirmware = new JoinDataComplete(new JoinData { JoinNumber = 61, JoinSpan = 1, AttributeName = "Info - Processor - Firmware" },
            new JoinMetadata { Description = "Info - Processor - Firmware", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProgramNameStart")]
        public JoinDataComplete ProgramNameStart = new JoinDataComplete(new JoinData { JoinNumber = 62, JoinSpan = 10, AttributeName = "Info - Processor - Program" },
            new JoinMetadata { Description = "Info - Processor - Program", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("ProcessorReboot")]
        public JoinDataComplete ProcessorReboot = new JoinDataComplete(new JoinData { JoinNumber = 74, JoinSpan = 1, AttributeName = "Processor - Reboot" },
            new JoinMetadata { Description = "Processor - Reboot", JoinCapabilities = eJoinCapabilities.FromFusion, JoinType = eJoinType.Digital });

        // Volume Controls
        [JoinName("VolumeFader1")]
        public JoinDataComplete VolumeFader1 = new JoinDataComplete(new JoinData { JoinNumber = 50, JoinSpan = 1, AttributeName = "Volume - Fader01" },
            new JoinMetadata { Description = "Volume - Fader01", JoinCapabilities = eJoinCapabilities.ToFromFusion, JoinType = eJoinType.Analog });

        // Codec Info
        [JoinName("VcCodecInCall")]
        public JoinDataComplete VcCodecInCall = new JoinDataComplete(new JoinData { JoinNumber = 69, JoinSpan = 1, AttributeName = "Conf - VC 1 In Call" },
            new JoinMetadata { Description = "Conf - VC 1 In Call", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Digital });

        [JoinName("VcCodecOnline")]
        public JoinDataComplete VcCodecOnline = new JoinDataComplete(new JoinData { JoinNumber = 122, JoinSpan = 1, AttributeName = "Online - VC 1" },
            new JoinMetadata { Description = "Online - VC 1", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Digital });

        [JoinName("VcCodecIpAddress")]
        public JoinDataComplete VcCodecIpAddress = new JoinDataComplete(new JoinData { JoinNumber = 121, JoinSpan = 1, AttributeName = "IP Address - VC" },
            new JoinMetadata { Description = "IP Address - VC", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        [JoinName("VcCodecIpPort")]
        public JoinDataComplete VcCodecIpPort = new JoinDataComplete(new JoinData { JoinNumber = 150, JoinSpan = 1, AttributeName = "IP Port - VC" },
            new JoinMetadata { Description = "IP Port - VC", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });

        // Source Attributes 
        [JoinName("Display1CurrentSourceName")]
        public JoinDataComplete Display1CurrentSourceName = new JoinDataComplete(new JoinData { JoinNumber = 84, JoinSpan = 1, AttributeName = "Display 1 - Current Source" },
            new JoinMetadata { Description = "Display 1 - Current Source", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Serial });


        // Device Online Status
        [JoinName("TouchpanelOnlineStart")]
        public JoinDataComplete TouchpanelOnlineStart = new JoinDataComplete(new JoinData { JoinNumber = 150, JoinSpan = 10, AttributeName = "Online - Touch Panel" },
            new JoinMetadata { Description = "Online - Touch Panel", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Digital });

        [JoinName("XpanelOnlineStart")]
        public JoinDataComplete XpanelOnlineStart = new JoinDataComplete(new JoinData { JoinNumber = 160, JoinSpan = 5, AttributeName = "Online - XPanel" },
            new JoinMetadata { Description = "Online - XPanel", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Digital });

        [JoinName("DisplayOnlineStart")]
        public JoinDataComplete DisplayOnlineStart = new JoinDataComplete(new JoinData { JoinNumber = 170, JoinSpan = 10, AttributeName = "Online - Display" },
            new JoinMetadata { Description = "Online - Display", JoinCapabilities = eJoinCapabilities.ToFusion, JoinType = eJoinType.Digital });

        [JoinName("Display1LaptopSourceStart")]
        public JoinDataComplete Display1LaptopSourceStart = new JoinDataComplete(new JoinData { JoinNumber = 166, JoinSpan = 5, AttributeName = "Display 1 - Source Laptop" },
            new JoinMetadata { Description = "Display 1 - Source Laptop", JoinCapabilities = eJoinCapabilities.ToFromFusion, JoinType = eJoinType.Digital });
        
        [JoinName("Display1DiscPlayerSourceStart")]
        public JoinDataComplete Display1DiscPlayerSourceStart = new JoinDataComplete(new JoinData { JoinNumber = 181, JoinSpan = 5, AttributeName = "Display 1 - Source Disc Player" },
            new JoinMetadata { Description = "Display 1 - Source Disc Player", JoinCapabilities = eJoinCapabilities.ToFromFusion, JoinType = eJoinType.Digital });

        [JoinName("Display1SetTopBoxSourceStart")]
        public JoinDataComplete Display1SetTopBoxSourceStart = new JoinDataComplete(new JoinData { JoinNumber = 188, JoinSpan = 5, AttributeName = "Display 1 - Source TV" },
            new JoinMetadata { Description = "Display 1 - Source TV", JoinCapabilities = eJoinCapabilities.ToFromFusion, JoinType = eJoinType.Digital });

        // Display 1 
        [JoinName("Display1Start")]
        public JoinDataComplete Display1Start = new JoinDataComplete(new JoinData { JoinNumber = 158, JoinSpan = 1 },
            new JoinMetadata { Description = "Display 1 Start", JoinCapabilities = eJoinCapabilities.ToFromFusion, JoinType = eJoinType.Digital });


        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public EssentialsHuddleSpaceRoomFusionRoomJoinMap(uint joinStart)
            : base(joinStart, typeof(EssentialsHuddleSpaceRoomFusionRoomJoinMap))
        {

        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        public EssentialsHuddleSpaceRoomFusionRoomJoinMap(uint joinStart, Type type) : base(joinStart, type)
        {
        }
    }   
}