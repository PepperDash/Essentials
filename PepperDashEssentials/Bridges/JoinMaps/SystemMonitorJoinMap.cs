using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Bridges
{
    public class SystemMonitorJoinMap : JoinMapBase
    {
        /// <summary>
        /// Offset to indicate where the range of iterated program joins will start
        /// </summary>
        public uint ProgramStartJoin { get; set; }

        /// <summary>
        /// Offset to indicate where the range of iterated Ethernet joins will start
        /// </summary>
        public uint EthernetStartJoin { get; set; }

        /// <summary>
        /// Offset between each program join set
        /// </summary>
        public uint ProgramOffsetJoin { get; set; }

        /// <summary>
        /// Offset between each Ethernet Interface join set
        /// </summary>
        public uint EthernetOffsetJoin { get; set; }

        #region Digitals
        /// <summary>
        /// Range Sets and reports whether the corresponding program slot is started
        /// </summary>
        public uint ProgramStart { get; set; }
        /// <summary>
        /// Range Sets and reports whether the corresponding program slot is stopped
        /// </summary>
        public uint ProgramStop { get; set; }
        /// <summary>
        /// Range Sets and reports whether the corresponding program is registered
        /// </summary>
        public uint ProgramRegister { get; set; }
        /// <summary>
        /// Range Sets and reports whether the corresponding program is unregistered
        /// </summary>
        public uint ProgramUnregister { get; set; }
        #endregion

        #region Analogs
        /// <summary>
        /// Sets and reports the time zone
        /// </summary>
        public uint TimeZone { get; set; }
        #endregion

        #region Serials
        /// <summary>
        /// Reports the time zone name
        /// </summary>
        public uint TimeZoneName { get; set; }
        /// <summary>
        /// Reports the IO Controller Version
        /// </summary>
        public uint IOControllerVersion { get; set; }
        /// <summary>
        /// Reports the SNMP App Version
        /// </summary>
        public uint SnmpAppVersion { get; set; }
        /// <summary>
        /// Reports the BACnet App Version
        /// </summary>
        public uint BACnetAppVersion { get; set; }
        /// <summary>
        /// Reports the firmware version
        /// </summary>
        public uint ControllerVersion { get; set; }

        /// <summary>
        /// Reports the name of the corresponding program
        /// </summary>
        public uint ProgramName { get; set; }
        /// <summary>
        /// Reports the compile time of the corresponding program
        /// </summary>
        public uint ProgramCompiledTime { get; set; }
        /// <summary>
        /// Reports the Crestron Database version of the corresponding program
        /// </summary>
        public uint ProgramCrestronDatabaseVersion { get; set; }
        /// <summary>
        /// Reports the Environment Version of the corresponding program
        /// </summary>
        public uint ProgramEnvironmentVersion { get; set; }
        /// <summary>
        /// Serialized JSON output that aggregates the program info of the corresponding program
        /// </summary>
        public uint AggregatedProgramInfo { get; set; }
        /// <summary>
        /// Reports the controller serial number
        /// </summary>
        public uint SerialNumber { get; set; }
        /// <summary>
        /// Reports the controller model
        /// </summary>
        public uint Model { get; set; }
        /// <summary>
        /// Reports the Host name set on the corresponding interface
        /// </summary>
        public uint HostName { get; set; }
        /// <summary>
        /// Reports the Current IP address set on the corresponding interface. If DHCP is enabled, this will be the DHCP assigned address.
        /// </summary>
        public uint CurrentIpAddress { get; set; }
        /// <summary>
        /// Reporst the Current Default Gateway set on the corresponding interface. If DHCP is enabled, this will be the DHCP assigned gateway
        /// </summary>
        public uint CurrentDefaultGateway { get; set; }
        /// <summary>
        /// Reports the Current Subnet Mask set on the corresponding interface. If DHCP is enabled, this will be the DHCP assigned subnet mask
        /// </summary>
        public uint CurrentSubnetMask { get; set; }
        /// <summary>
        /// Reports the Static IP address set on the corresponding interface. If DHCP is disabled, this will match the Current IP address
        /// </summary>
        public uint StaticIpAddress { get; set; }
        /// <summary>
        /// Reporst the Static Default Gateway set on the corresponding interface. If DHCP is disabled, this will match the Current gateway
        /// </summary>
        public uint StaticDefaultGateway { get; set; }
        /// <summary>
        /// Reports the Current Subnet Mask set on the corresponding interface. If DHCP is enabled, this will be the DHCP assigned subnet mask
        /// </summary>
        public uint StaticSubnetMask { get; set; }
        /// <summary>
        /// Reports the current DomainFeedback on the corresponding interface
        /// </summary>
        public uint Domain { get; set; }
        /// <summary>
        /// Reports the current DNS Servers on the corresponding interface
        /// </summary>
        public uint DnsServer { get; set; }
        /// <summary>
        /// Reports the MAC Address of the corresponding interface
        /// </summary>
        public uint MacAddress { get; set; }
        /// <summary>
        /// Reports the DHCP Status of the corresponding interface
        /// </summary>
        public uint DhcpStatus { get; set; }

        /// <summary>
        /// Reports the current uptime. Updated in 5 minute intervals.
        /// </summary>
        public uint Uptime { get; set; }

        /// <summary>
        /// Reports the date of the last boot
        /// </summary>
        public uint LastBoot { get; set; }
        #endregion

        public SystemMonitorJoinMap()
        {
            TimeZone = 1;

            TimeZoneName = 1;
            IOControllerVersion = 2;
            SnmpAppVersion = 3;
            BACnetAppVersion = 4;
            ControllerVersion = 5;
            SerialNumber = 6;
            Model = 7;
            Uptime = 8;
            LastBoot = 9;


            ProgramStartJoin = 10;

            ProgramOffsetJoin = 5;

            // Offset in groups of 5 joins
            ProgramStart = 1;
            ProgramStop = 2;
            ProgramRegister = 3;
            ProgramUnregister = 4;

            ProgramName = 1;
            ProgramCompiledTime = 2;
            ProgramCrestronDatabaseVersion = 3;
            ProgramEnvironmentVersion = 4;
            AggregatedProgramInfo = 5;

            EthernetStartJoin = 75;

            EthernetOffsetJoin = 15;

            // Offset in groups of 15
            HostName = 1;
            CurrentIpAddress = 2;
            CurrentSubnetMask = 3;
            CurrentDefaultGateway = 4;
            StaticIpAddress = 5;
            StaticSubnetMask = 6;
            StaticDefaultGateway = 7;
            Domain = 8;
            DnsServer = 9;
            MacAddress = 10;
            DhcpStatus = 11;
        }

        public override void OffsetJoinNumbers(uint joinStart)
        {
            var joinOffset = joinStart - 1;

            TimeZone = TimeZone + joinOffset;

            TimeZoneName = TimeZoneName + joinOffset;
            IOControllerVersion = IOControllerVersion + joinOffset;
            SnmpAppVersion = SnmpAppVersion + joinOffset;
            BACnetAppVersion = BACnetAppVersion + joinOffset;
            ControllerVersion = ControllerVersion + joinOffset;

            // Sets the initial join value where the iterated program joins will begin
            ProgramStartJoin = ProgramStartJoin + joinOffset;
            EthernetStartJoin = EthernetStartJoin + joinOffset;
        }
    }
}