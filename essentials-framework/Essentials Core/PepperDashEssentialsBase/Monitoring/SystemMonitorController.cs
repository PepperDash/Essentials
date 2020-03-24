using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.Diagnostics;
using PepperDash.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Core.Monitoring
{
    /// <summary>
    /// Wrapper for the static SystemMonitor class to extend functionality and provide external access
    /// to SystemMonitor via APIs
    /// </summary>
    public class SystemMonitorController : Device
    {
        protected const short LanAdapterIndex = 0;
        protected const short CsAdapterIndex = 1;
        
        public event EventHandler<EventArgs> SystemMonitorPropertiesChanged;

        public Dictionary<uint, ProgramStatusFeedbacks> ProgramStatusFeedbackCollection;

        public IntFeedback TimeZoneFeedback { get; protected set; }
        public StringFeedback TimeZoneTextFeedback { get; protected set; }

        public StringFeedback IoControllerVersionFeedback { get; protected set; }
        public StringFeedback SnmpVersionFeedback { get; protected set; }
        public StringFeedback BaCnetAppVersionFeedback { get; protected set; }
        public StringFeedback ControllerVersionFeedback { get; protected set; }

        //new feedbacks. Issue #50
        public StringFeedback FirmwareVersion { get; protected set; }
        public StringFeedback HostName { get; protected set; }
        public StringFeedback SerialNumber { get; protected set; }
        public StringFeedback Model { get; set; }
        public StringFeedback LanIpAddress { get; protected set; }
        public StringFeedback DefaultGateway { get; protected set; }
        public StringFeedback Domain { get; protected set; }
        public StringFeedback DnsServer01 { get; protected set; }
        public StringFeedback DnsServer02 { get; protected set; }
        public StringFeedback LanMacAddress { get; protected set; }
        public StringFeedback LanSubnetMask { get; protected set; }
     
        public StringFeedback CsIpAddress { get; protected set; }
        public StringFeedback CsSubnetMask { get; protected set; }


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

            ProgramStatusFeedbackCollection = new Dictionary<uint, ProgramStatusFeedbacks>();

            foreach (var prog in SystemMonitor.ProgramCollection)
            {
                var program = new ProgramStatusFeedbacks(prog);
                ProgramStatusFeedbackCollection.Add(prog.Number, program);
            }

            CreateControllerFeedbacks();

            SystemMonitor.ProgramChange += SystemMonitor_ProgramChange;
            SystemMonitor.TimeZoneInformation.TimeZoneChange += TimeZoneInformation_TimeZoneChange;
        }

        private void CreateControllerFeedbacks()
        {
            //assuming 0 = LAN, 1 = CS for devices that have CS
            FirmwareVersion = new StringFeedback(() => InitialParametersClass.FirmwareVersion);
            HostName = new StringFeedback(() => CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, LanAdapterIndex) );
            SerialNumber = new StringFeedback(() => CrestronEnvironment.SystemInfo.SerialNumber);
            Model = new StringFeedback(() => InitialParametersClass.ControllerPromptName);
            LanIpAddress = new StringFeedback(() => CrestronEthernetHelper.GetEthernetParameter(CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, LanAdapterIndex));
            DefaultGateway = new StringFeedback(() => String.Empty);
            Domain = new StringFeedback(() => String.Empty);
            DnsServer01 = new StringFeedback(() => String.Empty);
            DnsServer02 = new StringFeedback(() => String.Empty);
            LanMacAddress = new StringFeedback(() => String.Empty);
            LanSubnetMask = new StringFeedback(() => String.Empty);
            CsIpAddress = new StringFeedback(() => String.Empty);
            CsSubnetMask = new StringFeedback(() => String.Empty);
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

                var success = CrestronConsole.SendControlSystemCommand(
                    string.Format("progcomments:{0}", Program.Number), ref response);

                if (success)
                {
                    //Debug.Console(2, "Progcomments Response: \r{0}", response);

                    if (!response.ToLower().Contains("bad or incomplete"))
                    {
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
                        //Debug.Console(2, "ProgramInfo: \r{0}", JsonConvert.SerializeObject(ProgramInfo));
                    }
                    else
                    {
                        Debug.Console(2,
                            "Bad or incomplete console command response.  Initializing ProgramInfo for slot: {0}",
                            Program.Number);

                        // Assume no valid program info.  Constructing a new object will wipe all properties
                        ProgramInfo = new ProgramInfo(Program.Number)
                        {
                            OperatingState = Program.OperatingState,
                            RegistrationState = Program.RegistrationState
                        };
                    }
                }
                else
                {
                    Debug.Console(2, "Progcomments Attempt Unsuccessful for slot: {0}", Program.Number);
                }

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