using System;

namespace PepperDash.Essentials.Core.Bridges
{
    /// <summary>
    /// Represents a SystemMonitorJoinMap
    /// </summary>
    public class SystemMonitorJoinMap : JoinMapBaseAdvanced
    {
        /// <summary>
        /// Processor Timezone
        /// </summary>
        [JoinName("TimeZone")]
        public JoinDataComplete TimeZone = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Timezone", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Analog });

        /// <summary>
        /// Processor Timezone Name
        /// </summary>
        [JoinName("TimeZoneName")]
        public JoinDataComplete TimeZoneName = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Timezone Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor OS Version
        /// </summary>
        [JoinName("IOControllerVersion")]
        public JoinDataComplete IOControllerVersion = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor IO Controller Version", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor SNMP App Version
        /// </summary>
        [JoinName("SnmpAppVersion")]
        public JoinDataComplete SnmpAppVersion = new JoinDataComplete(new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor SNMP App Version", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor BACNet App Version
        /// </summary>
        [JoinName("BACnetAppVersion")]
        public JoinDataComplete BACnetAppVersion = new JoinDataComplete(new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor BACNet App Version", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Controller Version
        /// </summary>
        [JoinName("ControllerVersion")]
        public JoinDataComplete ControllerVersion = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Controller Version", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Serial Number
        /// </summary>
        [JoinName("SerialNumber")]
        public JoinDataComplete SerialNumber = new JoinDataComplete(new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Serial Number", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Model
        /// </summary>
        [JoinName("Model")]
        public JoinDataComplete Model = new JoinDataComplete(new JoinData { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Model", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Uptime
        /// </summary>
        [JoinName("Uptime")]
        public JoinDataComplete Uptime = new JoinDataComplete(new JoinData { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Uptime", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Last Boot Time
        /// </summary>
        [JoinName("LastBoot")]
        public JoinDataComplete LastBoot = new JoinDataComplete(new JoinData { JoinNumber = 9, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Last Boot", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Program Offset Join
        /// </summary>
        [JoinName("ProgramOffsetJoin")]
        public JoinDataComplete ProgramOffsetJoin = new JoinDataComplete(new JoinData { JoinNumber = 5, JoinSpan = 5 },
            new JoinMetadata { Description = "All Program Data is offset between slots by 5 - First Joins Start at 11", JoinCapabilities = eJoinCapabilities.None, JoinType = eJoinType.None });
		
        /// <summary>
        /// Processor Program Start
        /// </summary>
        [JoinName("ProgramStart")]
        public JoinDataComplete ProgramStart = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Program Start / Fb", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Processor Program Stop
        /// </summary>
        [JoinName("ProgramStop")]
        public JoinDataComplete ProgramStop = new JoinDataComplete(new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Program Stop / Fb", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Processor Program Register
        /// </summary>
        [JoinName("ProgramRegister")]
        public JoinDataComplete ProgramRegister = new JoinDataComplete(new JoinData { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Program Register / Fb", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Processor Program Unregister
        /// </summary>
        [JoinName("ProgramUnregister")]
        public JoinDataComplete ProgramUnregister = new JoinDataComplete(new JoinData { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Program UnRegister / Fb", JoinCapabilities = eJoinCapabilities.ToFromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Processor Program Name
        /// </summary>
        [JoinName("ProgramName")]
        public JoinDataComplete ProgramName = new JoinDataComplete(new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Program Name", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Program Version
        /// </summary>
        [JoinName("ProgramCompiledTime")]
        public JoinDataComplete ProgramCompiledTime = new JoinDataComplete(new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Program Compile Time", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Program Crestron Database Version
        /// </summary>
        [JoinName("ProgramCrestronDatabaseVersion")]
        public JoinDataComplete ProgramCrestronDatabaseVersion = new JoinDataComplete(new JoinData { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Program Database Version", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Program Environment Version
        /// </summary>
        [JoinName("ProgramEnvironmentVersion")]
        public JoinDataComplete ProgramEnvironmentVersion = new JoinDataComplete(new JoinData { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Program Environment Version", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Program Aggregate Info
        /// </summary>
        [JoinName("AggregatedProgramInfo")]
        public JoinDataComplete AggregatedProgramInfo = new JoinDataComplete(new JoinData { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Program Aggregate Info Json", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Ethernet Offset Join
        /// </summary>
        [JoinName("EthernetOffsetJoin")]
        public JoinDataComplete EthernetOffsetJoin = new JoinDataComplete(new JoinData { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata { Description = "All Ethernet Data is offset between Nics by 5 - First Joins Start at 76", JoinCapabilities = eJoinCapabilities.None, JoinType = eJoinType.None });

        /// <summary>
        /// Processor Ethernet Hostname
        /// </summary>
        [JoinName("HostName")]
        public JoinDataComplete HostName = new JoinDataComplete(new JoinData { JoinNumber = 76, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Ethernet Hostname", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Ethernet Current Ip Address
        /// </summary>
        [JoinName("CurrentIpAddress")]
        public JoinDataComplete CurrentIpAddress = new JoinDataComplete(new JoinData { JoinNumber = 77, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Ethernet Current Ip Address", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Ethernet Current Subnet Mask
        /// </summary>
        [JoinName("CurrentSubnetMask")]
        public JoinDataComplete CurrentSubnetMask = new JoinDataComplete(new JoinData { JoinNumber = 78, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Ethernet Current Subnet Mask", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Ethernet Current Default Gateway
        /// </summary>
        [JoinName("CurrentDefaultGateway")]
        public JoinDataComplete CurrentDefaultGateway = new JoinDataComplete(new JoinData { JoinNumber = 79, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Ethernet Current Default Gateway", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Ethernet Static Ip Address
        /// </summary>
        [JoinName("StaticIpAddress")]
        public JoinDataComplete StaticIpAddress = new JoinDataComplete(new JoinData { JoinNumber = 80, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Ethernet Static Ip Address", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Ethernet Static Subnet Mask
        /// </summary>
        [JoinName("StaticSubnetMask")]
        public JoinDataComplete StaticSubnetMask = new JoinDataComplete(new JoinData { JoinNumber = 81, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Ethernet Static Subnet Mask", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Ethernet Static Default Gateway
        /// </summary>
        [JoinName("StaticDefaultGateway")]
        public JoinDataComplete StaticDefaultGateway = new JoinDataComplete(new JoinData { JoinNumber = 82, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Ethernet Static Default Gateway", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Ethernet Domain
        /// </summary>
        [JoinName("Domain")]
        public JoinDataComplete Domain = new JoinDataComplete(new JoinData { JoinNumber = 83, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Ethernet Domain", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Ethernet Dns Server
        /// </summary>
        [JoinName("DnsServer")]
        public JoinDataComplete DnsServer = new JoinDataComplete(new JoinData { JoinNumber = 84, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Ethernet Dns Server", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Ethernet Mac Address
        /// </summary>
        [JoinName("MacAddress")]
        public JoinDataComplete MacAddress = new JoinDataComplete(new JoinData { JoinNumber = 85, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Ethernet Mac Address", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Ethernet Dhcp Status
        /// </summary>
        [JoinName("DhcpStatus")]
        public JoinDataComplete DhcpStatus = new JoinDataComplete(new JoinData { JoinNumber = 86, JoinSpan = 1 },
            new JoinMetadata { Description = "Processor Ethernet Dhcp Status", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Serial });

        /// <summary>
        /// Processor Reboot
        /// </summary>
		[JoinName("ProcessorRebot")]
		public JoinDataComplete ProcessorReboot = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
			new JoinMetadata { Description = "Reboot processor", JoinCapabilities = eJoinCapabilities.FromSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Is Appliance Fb
        /// </summary>
		[JoinName("IsAppliance")]
		public JoinDataComplete IsAppliance = new JoinDataComplete(new JoinData { JoinNumber = 1, JoinSpan = 1 },
			new JoinMetadata { Description = "Is appliance Fb", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Is Server Fb
        /// </summary>
		[JoinName("IsServer")]
		public JoinDataComplete IsServer = new JoinDataComplete(new JoinData { JoinNumber = 2, JoinSpan = 1 },
			new JoinMetadata { Description = "Is server Fb", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });

        /// <summary>
        /// Program Reset
        /// </summary>
		[JoinName("ProgramReset")]
		public JoinDataComplete ProgramReset = new JoinDataComplete(new JoinData { JoinNumber = 15, JoinSpan = 1 },
			new JoinMetadata { Description = "Resets the program", JoinCapabilities = eJoinCapabilities.ToSIMPL, JoinType = eJoinType.Digital });


        /// <summary>
        /// Constructor to use when instantiating this Join Map without inheriting from it
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        public SystemMonitorJoinMap(uint joinStart)
            : this(joinStart, typeof(SystemMonitorJoinMap))
        {
        }

        /// <summary>
        /// Constructor to use when extending this Join map
        /// </summary>
        /// <param name="joinStart">Join this join map will start at</param>
        /// <param name="type">Type of the child join map</param>
        protected SystemMonitorJoinMap(uint joinStart, Type type)
            : base(joinStart, type)
        {
        }
    }
}