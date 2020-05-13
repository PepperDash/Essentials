using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.Diagnostics;
using PepperDash.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Core.Monitoring
{
    /// <summary>
    /// Wrapper for the static SystemMonitor class to extend functionality and provide external access
    /// to SystemMonitor via APIs
    /// </summary>
    public class SystemMonitorController : EssentialsBridgeableDevice
    {
        private const long UptimePollTime = 300000;
        private CTimer _uptimePollTimer;

        private string _uptime;
        private string _lastStart;

        public event EventHandler<EventArgs> SystemMonitorPropertiesChanged;

        public Dictionary<uint, ProgramStatusFeedbacks> ProgramStatusFeedbackCollection;
        public Dictionary<short, EthernetStatusFeedbacks> EthernetStatusFeedbackCollection;

        public IntFeedback TimeZoneFeedback { get; protected set; }
        public StringFeedback TimeZoneTextFeedback { get; protected set; }

        public StringFeedback IoControllerVersionFeedback { get; protected set; }
        public StringFeedback SnmpVersionFeedback { get; protected set; }
        public StringFeedback BaCnetAppVersionFeedback { get; protected set; }
        public StringFeedback ControllerVersionFeedback { get; protected set; }

        //new feedbacks. Issue #50
        public StringFeedback SerialNumberFeedback { get; protected set; }
        public StringFeedback ModelFeedback { get; set; }

        public StringFeedback UptimeFeedback { get; set; }
        public StringFeedback LastStartFeedback { get; set; }

        public SystemMonitorController(string key)
            : base(key)
        {
            Debug.Console(2, this, "Adding SystemMonitorController.");

            SystemMonitor.ProgramInitialization.ProgramInitializationUnderUserControl = true;

            TimeZoneFeedback = new IntFeedback(() => SystemMonitor.TimeZoneInformation.TimeZoneNumber);
            TimeZoneTextFeedback = new StringFeedback(() => SystemMonitor.TimeZoneInformation.TimeZoneName);

            IoControllerVersionFeedback = new StringFeedback(() => SystemMonitor.VersionInformation.IOPVersion);
            SnmpVersionFeedback = new StringFeedback(() => SystemMonitor.VersionInformation.SNMPVersion);
            BaCnetAppVersionFeedback = new StringFeedback(() => SystemMonitor.VersionInformation.BACNetVersion);
            ControllerVersionFeedback = new StringFeedback(() => SystemMonitor.VersionInformation.ControlSystemVersion);

            SerialNumberFeedback = new StringFeedback(() => CrestronEnvironment.SystemInfo.SerialNumber);
            ModelFeedback = new StringFeedback(() => InitialParametersClass.ControllerPromptName);
            UptimeFeedback = new StringFeedback(() => _uptime);
            LastStartFeedback = new StringFeedback(()=> _lastStart);

            ProgramStatusFeedbackCollection = new Dictionary<uint, ProgramStatusFeedbacks>();

            foreach (var prog in SystemMonitor.ProgramCollection)
            {
                var program = new ProgramStatusFeedbacks(prog);
                ProgramStatusFeedbackCollection.Add(prog.Number, program);
            }

            CreateEthernetStatusFeedbacks();
            UpdateEthernetStatusFeeedbacks();

            _uptimePollTimer = new CTimer(PollUptime,null,0, UptimePollTime);

            SystemMonitor.ProgramChange += SystemMonitor_ProgramChange;
            SystemMonitor.TimeZoneInformation.TimeZoneChange += TimeZoneInformation_TimeZoneChange;
            CrestronEnvironment.EthernetEventHandler += CrestronEnvironmentOnEthernetEventHandler;
            CrestronEnvironment.ProgramStatusEventHandler += CrestronEnvironmentOnProgramStatusEventHandler;
        }

        private void CrestronEnvironmentOnProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType != eProgramStatusEventType.Stopping) return;

            _uptimePollTimer.Stop();
            _uptimePollTimer.Dispose();
            _uptimePollTimer = null;
        }

        private void PollUptime(object obj)
        {
            var consoleResponse = string.Empty;

            CrestronConsole.SendControlSystemCommand("uptime", ref consoleResponse);

            ParseUptime(consoleResponse);

            UptimeFeedback.FireUpdate();
            LastStartFeedback.FireUpdate();
        }

        private void ParseUptime(string response)
        {
            var splitString = response.Trim().Split('\r', '\n');

            var lastStartRaw = splitString[2];
            var lastStartIndex = lastStartRaw.IndexOf(':');

            _lastStart = lastStartRaw.Substring(lastStartIndex + 1).Trim();

            var uptimeRaw = splitString[0];

            var forIndex = uptimeRaw.IndexOf("for", StringComparison.Ordinal);

            //4 => "for " to get what's on the right
            _uptime = uptimeRaw.Substring(forIndex + 4);
        }

        private void CrestronEnvironmentOnEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            if (ethernetEventArgs.EthernetEventType != eEthernetEventType.LinkUp) return;

            foreach (var fb in EthernetStatusFeedbackCollection)
            {
                fb.Value.UpdateEthernetStatus();
            }
        }

        private void CreateEthernetStatusFeedbacks()
        {
            EthernetStatusFeedbackCollection = new Dictionary<short, EthernetStatusFeedbacks>();

            Debug.Console(2, "Creating {0} EthernetStatusFeedbacks", InitialParametersClass.NumberOfEthernetInterfaces);

            for (short i = 0; i < InitialParametersClass.NumberOfEthernetInterfaces; i++)
            {
                Debug.Console(2, "Creating EthernetStatusFeedback for Interface {0}", i);
                var ethernetInterface = new EthernetStatusFeedbacks(i);
                EthernetStatusFeedbackCollection.Add(i, ethernetInterface);
            }
        }

        private void UpdateEthernetStatusFeeedbacks()
        {
            foreach (var iface in EthernetStatusFeedbackCollection)
            {
                iface.Value.CurrentIpAddressFeedback.FireUpdate();
                iface.Value.CurrentSubnetMaskFeedback.FireUpdate();
                iface.Value.CurrentDefaultGatewayFeedback.FireUpdate();
                iface.Value.StaticIpAddressFeedback.FireUpdate();
                iface.Value.StaticSubnetMaskFeedback.FireUpdate();
                iface.Value.StaticDefaultGatewayFeedback.FireUpdate();
                iface.Value.HostNameFeedback.FireUpdate();
                iface.Value.DnsServerFeedback.FireUpdate();
                iface.Value.DomainFeedback.FireUpdate();
                iface.Value.DhcpStatusFeedback.FireUpdate();
                iface.Value.MacAddressFeedback.FireUpdate();
            }
        }

        /// <summary>
        /// Gets data in separate thread
        /// </summary>
        private void RefreshSystemMonitorData()
        {
            // this takes a while, launch a new thread
            CrestronInvoke.BeginInvoke(UpdateFeedback);
        }

        private void UpdateFeedback(object o)
        {
            TimeZoneFeedback.FireUpdate();
            TimeZoneTextFeedback.FireUpdate();
            IoControllerVersionFeedback.FireUpdate();
            SnmpVersionFeedback.FireUpdate();
            BaCnetAppVersionFeedback.FireUpdate();
            ControllerVersionFeedback.FireUpdate();
            SerialNumberFeedback.FireUpdate();
            ModelFeedback.FireUpdate();

            OnSystemMonitorPropertiesChanged();
        }

        private void OnSystemMonitorPropertiesChanged()
        {
            var handler = SystemMonitorPropertiesChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public override bool CustomActivate()
        {
            RefreshSystemMonitorData();

            return base.CustomActivate();
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new SystemMonitorJoinMap(joinStart);

            var joinMapSerialized = JoinMapHelper.GetSerializedJoinMapForDevice(joinMapKey);

            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<SystemMonitorJoinMap>(joinMapSerialized);

            bridge.AddJoinMap(Key, joinMap);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(2, this, "Linking API starting at join: {0}", joinStart);

            TimeZoneFeedback.LinkInputSig(trilist.UShortInput[joinMap.TimeZone.JoinNumber]);
            TimeZoneTextFeedback.LinkInputSig(trilist.StringInput[joinMap.TimeZoneName.JoinNumber]);

            IoControllerVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.IOControllerVersion.JoinNumber]);
            SnmpVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.SnmpAppVersion.JoinNumber]);
            BaCnetAppVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.BACnetAppVersion.JoinNumber]);
            ControllerVersionFeedback.LinkInputSig(trilist.StringInput[joinMap.ControllerVersion.JoinNumber]);
            SerialNumberFeedback.LinkInputSig(trilist.StringInput[joinMap.SerialNumber.JoinNumber]);
            ModelFeedback.LinkInputSig(trilist.StringInput[joinMap.Model.JoinNumber]);
            UptimeFeedback.LinkInputSig(trilist.StringInput[joinMap.Uptime.JoinNumber]);
            LastStartFeedback.LinkInputSig(trilist.StringInput[joinMap.LastBoot.JoinNumber]);

            // iterate the program status feedback collection and map all the joins
            LinkProgramInfoJoins(this, trilist, joinMap);

            LinkEthernetInfoJoins(this, trilist, joinMap);
        }

        private static void LinkEthernetInfoJoins(SystemMonitorController systemMonitorController, BasicTriList trilist, SystemMonitorJoinMap joinMap)
        {
            uint ethernetSlotJoinStart = 0;
            foreach (var fb in systemMonitorController.EthernetStatusFeedbackCollection)
            {
                fb.Value.CurrentIpAddressFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.CurrentIpAddress.JoinNumber]);
                fb.Value.CurrentSubnetMaskFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.CurrentSubnetMask.JoinNumber]);
                fb.Value.CurrentDefaultGatewayFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.CurrentDefaultGateway.JoinNumber]);
                fb.Value.StaticIpAddressFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.StaticIpAddress.JoinNumber]);
                fb.Value.StaticSubnetMaskFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.StaticSubnetMask.JoinNumber]);
                fb.Value.StaticDefaultGatewayFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.StaticDefaultGateway.JoinNumber]);
                fb.Value.HostNameFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.HostName.JoinNumber]);
                fb.Value.MacAddressFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.MacAddress.JoinNumber]);
                fb.Value.DomainFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.Domain.JoinNumber]);
                fb.Value.DnsServerFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.DnsServer.JoinNumber]);
                fb.Value.DhcpStatusFeedback.LinkInputSig(trilist.StringInput[ethernetSlotJoinStart + joinMap.DhcpStatus.JoinNumber]);

                ethernetSlotJoinStart += joinMap.EthernetOffsetJoin.JoinNumber;
            }
        }

        private static void LinkProgramInfoJoins(SystemMonitorController systemMonitorController, BasicTriList trilist,
            SystemMonitorJoinMap joinMap)
        {
            uint programSlotJoinStart = 0;

            foreach (var p in systemMonitorController.ProgramStatusFeedbackCollection)
            {
                var programNumber = p.Value.Program.Number;

                trilist.SetBoolSigAction(programSlotJoinStart + joinMap.ProgramStart.JoinNumber,
                    b => SystemMonitor.ProgramCollection[programNumber].OperatingState = eProgramOperatingState.Start);
                p.Value.ProgramStartedFeedback.LinkInputSig(trilist.BooleanInput[programSlotJoinStart + joinMap.ProgramStart.JoinNumber]);

                trilist.SetBoolSigAction(programSlotJoinStart + joinMap.ProgramStop.JoinNumber,
                    b => SystemMonitor.ProgramCollection[programNumber].OperatingState = eProgramOperatingState.Stop);
                p.Value.ProgramStoppedFeedback.LinkInputSig(trilist.BooleanInput[programSlotJoinStart + joinMap.ProgramStop.JoinNumber]);

                trilist.SetBoolSigAction(programSlotJoinStart + joinMap.ProgramRegister.JoinNumber,
                    b => SystemMonitor.ProgramCollection[programNumber].RegistrationState = eProgramRegistrationState.Register);
                p.Value.ProgramRegisteredFeedback.LinkInputSig(
                    trilist.BooleanInput[programSlotJoinStart + joinMap.ProgramRegister.JoinNumber]);

                trilist.SetBoolSigAction(programSlotJoinStart + joinMap.ProgramUnregister.JoinNumber,
                    b => SystemMonitor.ProgramCollection[programNumber].RegistrationState = eProgramRegistrationState.Unregister);
                p.Value.ProgramUnregisteredFeedback.LinkInputSig(
                    trilist.BooleanInput[programSlotJoinStart + joinMap.ProgramUnregister.JoinNumber]);

                p.Value.ProgramNameFeedback.LinkInputSig(trilist.StringInput[programSlotJoinStart + joinMap.ProgramName.JoinNumber]);
                p.Value.ProgramCompileTimeFeedback.LinkInputSig(
                    trilist.StringInput[programSlotJoinStart + joinMap.ProgramCompiledTime.JoinNumber]);
                p.Value.CrestronDataBaseVersionFeedback.LinkInputSig(
                    trilist.StringInput[programSlotJoinStart + joinMap.ProgramCrestronDatabaseVersion.JoinNumber]);
                p.Value.EnvironmentVersionFeedback.LinkInputSig(
                    trilist.StringInput[programSlotJoinStart + joinMap.ProgramEnvironmentVersion.JoinNumber]);
                p.Value.AggregatedProgramInfoFeedback.LinkInputSig(
                    trilist.StringInput[programSlotJoinStart + joinMap.AggregatedProgramInfo.JoinNumber]);

                programSlotJoinStart = programSlotJoinStart + joinMap.ProgramOffsetJoin.JoinNumber;
            }
        }

        //// Sets the time zone
        //public void SetTimeZone(int timeZone)
        //{
        //    SystemMonitor.TimeZoneInformation.TimeZoneNumber = timeZone;
        //}

        /// <summary>
        /// Responds to program change events and triggers the appropriate feedbacks to update
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SystemMonitor_ProgramChange(Program sender, ProgramEventArgs args)
        {
            Debug.Console(2, this, "Program Change Detected for slot: {0}", sender.Number);
            Debug.Console(2, this, "Event Type: {0}", args.EventType);

            var program = ProgramStatusFeedbackCollection[sender.Number];

            switch (args.EventType)
            {
                case eProgramChangeEventType.OperatingState:
                    program.ProgramStartedFeedback.FireUpdate();
                    program.ProgramStoppedFeedback.FireUpdate();
                    program.ProgramInfo.OperatingState = args.OperatingState;
                    if (args.OperatingState == eProgramOperatingState.Start)
                        program.GetProgramInfo();
                    else
                    {
                        program.AggregatedProgramInfoFeedback.FireUpdate();
                        program.OnProgramInfoChanged();
                    }
                    break;
                case eProgramChangeEventType.RegistrationState:
                    program.ProgramRegisteredFeedback.FireUpdate();
                    program.ProgramUnregisteredFeedback.FireUpdate();
                    program.ProgramInfo.RegistrationState = args.RegistrationState;
                    program.GetProgramInfo();
                    break;
            }
        }

        /// <summary>
        /// Responds to time zone changes and updates the appropriate feedbacks
        /// </summary>
        /// <param name="args"></param>
        private void TimeZoneInformation_TimeZoneChange(TimeZoneEventArgs args)
        {
            Debug.Console(2, this, "Time Zone Change Detected.");
            TimeZoneFeedback.FireUpdate();
            TimeZoneTextFeedback.FireUpdate();

            OnSystemMonitorPropertiesChanged();
        }

        public class EthernetStatusFeedbacks
        {
            public StringFeedback HostNameFeedback { get; protected set; }
            public StringFeedback DnsServerFeedback { get; protected set; }
            public StringFeedback DomainFeedback { get; protected set; }
            public StringFeedback MacAddressFeedback { get; protected set; }
            public StringFeedback DhcpStatusFeedback { get; protected set; }

            public StringFeedback CurrentIpAddressFeedback { get; protected set; }
            public StringFeedback CurrentSubnetMaskFeedback { get; protected set; }
            public StringFeedback CurrentDefaultGatewayFeedback { get; protected set; }
            
            public StringFeedback StaticIpAddressFeedback { get; protected set; }
            public StringFeedback StaticSubnetMaskFeedback { get; protected set; }
            public StringFeedback StaticDefaultGatewayFeedback { get; protected set; }

            public EthernetStatusFeedbacks(short adapterIndex)
            {
                Debug.Console(2, "Ethernet Information for interface {0}", adapterIndex);
                Debug.Console(2, "Adapter Index: {1} Hostname: {0}", CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, adapterIndex), adapterIndex);
                Debug.Console(2, "Adapter Index: {1} Current IP Address: {0}", CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, adapterIndex), adapterIndex);
                Debug.Console(2, "Adapter Index: {1} Current Subnet Mask: {0}", CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_MASK, adapterIndex), adapterIndex);
                Debug.Console(2, "Adapter Index: {1} Current Router: {0}", CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_ROUTER, adapterIndex), adapterIndex);
                Debug.Console(2, "Adapter Index: {1} Static IP Address: {0}", CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_STATIC_IPADDRESS, adapterIndex), adapterIndex);
                Debug.Console(2, "Adapter Index: {1} Static Subnet Mask: {0}", CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_STATIC_IPMASK, adapterIndex), adapterIndex);
                Debug.Console(2, "Adapter Index: {1} Static Router: {0}", CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_STATIC_ROUTER, adapterIndex), adapterIndex);
                Debug.Console(2, "Adapter Index: {1} DNS Servers: {0}", CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DNS_SERVER, adapterIndex), adapterIndex);
                Debug.Console(2, "Adapter Index: {1} DHCP State: {0}", CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_DHCP_STATE, adapterIndex), adapterIndex);
                Debug.Console(2, "Adapter Index: {1} Domain Name: {0}", CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DOMAIN_NAME, adapterIndex), adapterIndex);
                Debug.Console(2, "Adapter Index: {1} MAC Address: {0}", CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, adapterIndex), adapterIndex);
                HostNameFeedback =
                    new StringFeedback(
                        () =>
                            CrestronEthernetHelper.GetEthernetParameter(
                                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, adapterIndex));

                CurrentIpAddressFeedback =
                    new StringFeedback(
                        () =>
                            CrestronEthernetHelper.GetEthernetParameter(
                                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, adapterIndex));
                CurrentDefaultGatewayFeedback =
                    new StringFeedback(
                        () =>
                            CrestronEthernetHelper.GetEthernetParameter(
                                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_ROUTER, adapterIndex));
                CurrentSubnetMaskFeedback =
                    new StringFeedback(
                        () =>
                            CrestronEthernetHelper.GetEthernetParameter(
                                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_MASK, adapterIndex));
                StaticIpAddressFeedback =
                    new StringFeedback(
                        () =>
                            CrestronEthernetHelper.GetEthernetParameter(
                                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, adapterIndex));
                StaticDefaultGatewayFeedback =
                    new StringFeedback(
                        () =>
                            CrestronEthernetHelper.GetEthernetParameter(
                                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_ROUTER, adapterIndex));
                StaticSubnetMaskFeedback =
                    new StringFeedback(
                        () =>
                            CrestronEthernetHelper.GetEthernetParameter(
                                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_MASK, adapterIndex));
                DomainFeedback =
                    new StringFeedback(
                        () =>
                            CrestronEthernetHelper.GetEthernetParameter(
                                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DOMAIN_NAME, adapterIndex));
                DnsServerFeedback =
                    new StringFeedback(
                        () =>
                            CrestronEthernetHelper.GetEthernetParameter(
                                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DNS_SERVER, adapterIndex));
                MacAddressFeedback =
                    new StringFeedback(
                        () =>
                            CrestronEthernetHelper.GetEthernetParameter(
                                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, adapterIndex));

                DhcpStatusFeedback = new StringFeedback(
                    () =>
                        CrestronEthernetHelper.GetEthernetParameter(
                            CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_DHCP_STATE, adapterIndex));
            }

            public void UpdateEthernetStatus()
            {
                HostNameFeedback.FireUpdate();
                CurrentIpAddressFeedback.FireUpdate();
                CurrentSubnetMaskFeedback.FireUpdate();
                CurrentDefaultGatewayFeedback.FireUpdate();
                StaticIpAddressFeedback.FireUpdate();
                StaticSubnetMaskFeedback.FireUpdate();
                StaticDefaultGatewayFeedback.FireUpdate();
                DomainFeedback.FireUpdate();
                DnsServerFeedback.FireUpdate();
                MacAddressFeedback.FireUpdate();
                DhcpStatusFeedback.FireUpdate();
            }
        }


        public class ProgramStatusFeedbacks
        {
            public event EventHandler<ProgramInfoEventArgs> ProgramInfoChanged;

            public Program Program;

            public ProgramInfo ProgramInfo { get; set; }

            public BoolFeedback ProgramStartedFeedback;
            public BoolFeedback ProgramStoppedFeedback;
            public BoolFeedback ProgramRegisteredFeedback;
            public BoolFeedback ProgramUnregisteredFeedback;

            public StringFeedback ProgramNameFeedback;
            public StringFeedback ProgramCompileTimeFeedback;
            public StringFeedback CrestronDataBaseVersionFeedback;
            // SIMPL windows version
            public StringFeedback EnvironmentVersionFeedback;
            public StringFeedback AggregatedProgramInfoFeedback;

            public ProgramStatusFeedbacks(Program program)
            {
                ProgramInfo = new ProgramInfo(program.Number);

                Program = program;

                ProgramInfo.OperatingState = Program.OperatingState;
                ProgramInfo.RegistrationState = Program.RegistrationState;

                ProgramStartedFeedback = new BoolFeedback(() => Program.OperatingState == eProgramOperatingState.Start);
                ProgramStoppedFeedback = new BoolFeedback(() => Program.OperatingState == eProgramOperatingState.Stop);
                ProgramRegisteredFeedback =
                    new BoolFeedback(() => Program.RegistrationState == eProgramRegistrationState.Register);
                ProgramUnregisteredFeedback =
                    new BoolFeedback(() => Program.RegistrationState == eProgramRegistrationState.Unregister);

                ProgramNameFeedback = new StringFeedback(() => ProgramInfo.ProgramFile);
                ProgramCompileTimeFeedback = new StringFeedback(() => ProgramInfo.CompileTime);
                CrestronDataBaseVersionFeedback = new StringFeedback(() => ProgramInfo.CrestronDb);
                EnvironmentVersionFeedback = new StringFeedback(() => ProgramInfo.Environment);

                AggregatedProgramInfoFeedback = new StringFeedback(() => JsonConvert.SerializeObject(ProgramInfo));

                GetProgramInfo();
            }

            /// <summary>
            /// Retrieves information about a running program
            /// </summary>
            public void GetProgramInfo()
            {
                CrestronInvoke.BeginInvoke(GetProgramInfo);
            }

            private void GetProgramInfo(object o)
            {
                Debug.Console(2, "Attempting to get program info for slot: {0}", Program.Number);

                string response = null;

                if (Program.RegistrationState == eProgramRegistrationState.Unregister || Program.OperatingState == eProgramOperatingState.Stop)
                {
                    Debug.Console(2, "Program {0} not registered. Setting default values for program information.",
                        Program.Number);

                    ProgramInfo = new ProgramInfo(Program.Number)
                    {
                        OperatingState = Program.OperatingState,
                        RegistrationState = Program.RegistrationState
                    };

                    return;
                }

                var success = CrestronConsole.SendControlSystemCommand(
                    string.Format("progcomments:{0}", Program.Number), ref response);

                if (!success)
                {
                    Debug.Console(2, "Progcomments Attempt Unsuccessful for slot: {0}", Program.Number);
                    UpdateFeedbacks();
                    return;
                }

                if (response.ToLower().Contains("bad or incomplete"))
                {
                    Debug.Console(2,
                        "Program in slot {0} not running.  Setting default ProgramInfo for slot: {0}",
                        Program.Number);

                    // Assume no valid program info.  Constructing a new object will wipe all properties
                    ProgramInfo = new ProgramInfo(Program.Number)
                    {
                        OperatingState = Program.OperatingState,
                        RegistrationState = Program.RegistrationState
                    };

                    UpdateFeedbacks();

                    return;
                }


                // Shared properteis
                ProgramInfo.ProgramFile = ParseConsoleData(response, "Program File", ": ", "\n");
                ProgramInfo.CompilerRevision = ParseConsoleData(response, "Compiler Rev", ": ", "\n");
                ProgramInfo.CompileTime = ParseConsoleData(response, "Compiled On", ": ", "\n");
                ProgramInfo.Include4Dat = ParseConsoleData(response, "Include4.dat", ": ", "\n");


                if (ProgramInfo.ProgramFile.Contains(".dll"))
                {
                    // SSP Program
                    ProgramInfo.FriendlyName = ParseConsoleData(response, "Friendly Name", ": ", "\n");
                    ProgramInfo.ApplicationName = ParseConsoleData(response, "Application Name", ": ", "\n");
                    ProgramInfo.ProgramTool = ParseConsoleData(response, "Program Tool", ": ", "\n");
                    ProgramInfo.MinFirmwareVersion = ParseConsoleData(response, "Min Firmware Version", ": ",
                        "\n");
                    ProgramInfo.PlugInVersion = ParseConsoleData(response, "PlugInVersion", ": ", "\n");
                }
                else if (ProgramInfo.ProgramFile.Contains(".smw"))
                {
                    // SIMPL Windows Program
                    ProgramInfo.FriendlyName = ParseConsoleData(response, "Friendly Name", ":", "\n");
                    ProgramInfo.SystemName = ParseConsoleData(response, "System Name", ": ", "\n");
                    ProgramInfo.CrestronDb = ParseConsoleData(response, "CrestronDB", ": ", "\n");
                    ProgramInfo.Environment = ParseConsoleData(response, "Source Env", ": ", "\n");
                    ProgramInfo.Programmer = ParseConsoleData(response, "Programmer", ": ", "\n");
                }
                Debug.Console(2, "Program info for slot {0} successfully updated", Program.Number);

                UpdateFeedbacks();
            }

            private void UpdateFeedbacks()
            {
                ProgramNameFeedback.FireUpdate();
                ProgramCompileTimeFeedback.FireUpdate();
                CrestronDataBaseVersionFeedback.FireUpdate();
                EnvironmentVersionFeedback.FireUpdate();

                AggregatedProgramInfoFeedback.FireUpdate();

                OnProgramInfoChanged();
            }

            public void OnProgramInfoChanged()
            {
                //Debug.Console(1, "Firing ProgramInfoChanged for slot: {0}", Program.Number);
                var handler = ProgramInfoChanged;
                if (handler != null)
                {
                    handler(this, new ProgramInfoEventArgs(ProgramInfo));
                }
            }

            private string ParseConsoleData(string data, string line, string startString, string endString)
            {
                var outputData = "";

                if (data.Length <= 0) return outputData;

                try
                {
                    //Debug.Console(2, "ParseConsoleData Data: {0}, Line {1}, startStirng {2}, endString {3}", data, line, startString, endString);
                    var linePosition = data.IndexOf(line, StringComparison.Ordinal);
                    var startPosition = data.IndexOf(startString, linePosition, StringComparison.Ordinal) +
                                        startString.Length;
                    var endPosition = data.IndexOf(endString, startPosition, StringComparison.Ordinal);
                    outputData = data.Substring(startPosition, endPosition - startPosition).Trim();
                    //Debug.Console(2, "ParseConsoleData Return: {0}", outputData);
                }
                catch (Exception e)
                {
                    Debug.Console(1, "Error Parsing Console Data:\r{0}", e);
                }

                return outputData;
            }
        }
    }

    /// <summary>
    /// Class for serializing program slot information
    /// </summary>
    public class ProgramInfo
    {
        // Shared properties

        [JsonProperty("programNumber")]
        public uint ProgramNumber { get; private set; }

        [JsonConverter(typeof (StringEnumConverter))]
        [JsonProperty("operatingState")]
        public eProgramOperatingState OperatingState { get; set; }

        [JsonConverter(typeof (StringEnumConverter))]
        [JsonProperty("registrationState")]
        public eProgramRegistrationState RegistrationState { get; set; }

        [JsonProperty("programFile")]
        public string ProgramFile { get; set; }

        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("compilerRevision")]
        public string CompilerRevision { get; set; }

        [JsonProperty("compileTime")]
        public string CompileTime { get; set; }

        [JsonProperty("include4Dat")]
        public string Include4Dat { get; set; }

        // SIMPL Windows properties
        [JsonProperty("systemName")]
        public string SystemName { get; set; }

        [JsonProperty("crestronDb")]
        public string CrestronDb { get; set; }

        [JsonProperty("environment")]
        public string Environment { get; set; }

        [JsonProperty("programmer")]
        public string Programmer { get; set; }


        // SSP Properties
        [JsonProperty("applicationName")]
        public string ApplicationName { get; set; }

        [JsonProperty("programTool")]
        public string ProgramTool { get; set; }

        [JsonProperty("minFirmwareVersion")]
        public string MinFirmwareVersion { get; set; }

        [JsonProperty("plugInVersion")]
        public string PlugInVersion { get; set; }

        public ProgramInfo(uint number)
        {
            ProgramNumber = number;

            ProgramFile = "";
            FriendlyName = "";
            CompilerRevision = "";
            CompileTime = "";
            Include4Dat = "";

            SystemName = "";
            CrestronDb = "";
            Environment = "";
            Programmer = "";

            ApplicationName = "";
            ProgramTool = "";
            MinFirmwareVersion = "";
            PlugInVersion = "";
        }
    }

    public class ProgramInfoEventArgs : EventArgs
    {
        public ProgramInfo ProgramInfo;

        public ProgramInfoEventArgs(ProgramInfo progInfo)
        {
            ProgramInfo = progInfo;
        }
    }
}