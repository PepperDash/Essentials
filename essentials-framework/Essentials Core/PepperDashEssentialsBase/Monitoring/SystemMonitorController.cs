using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.Diagnostics;

using PepperDash.Core;
using PepperDash.Essentials.Core;

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
        public event EventHandler<EventArgs> SystemMonitorPropertiesChanged;

        public Dictionary<uint, ProgramStatusFeedbacks> ProgramStatusFeedbackCollection;

        public IntFeedback TimeZoneFeedback { get; set; }
        public StringFeedback TimeZoneTextFeedback { get; set; }

        public StringFeedback IOControllerVersionFeedback { get; set; }
        public StringFeedback SnmpVersionFeedback { get; set; }
        public StringFeedback BACnetAppVersionFeedback { get; set; }
        public StringFeedback ControllerVersionFeedback { get; set; }
        
        public SystemMonitorController(string key)
            : base(key)
        {
            Debug.Console(2, this, "Adding SystemMonitorController.");

            SystemMonitor.ProgramInitialization.ProgramInitializationUnderUserControl = true;

            //CrestronConsole.AddNewConsoleCommand(RefreshSystemMonitorData, "RefreshSystemMonitor", "Refreshes System Monitor Feedbacks", ConsoleAccessLevelEnum.AccessOperator);

            TimeZoneFeedback = new IntFeedback(new Func<int>( () => SystemMonitor.TimeZoneInformation.TimeZoneNumber));
            TimeZoneTextFeedback = new StringFeedback(new Func<string>( () => SystemMonitor.TimeZoneInformation.TimeZoneName));

            IOControllerVersionFeedback = new StringFeedback(new Func<string>( () => SystemMonitor.VersionInformation.IOPVersion));
            SnmpVersionFeedback = new StringFeedback(new Func<string>( () => SystemMonitor.VersionInformation.SNMPVersion));
            BACnetAppVersionFeedback = new StringFeedback(new Func<string>( () => SystemMonitor.VersionInformation.BACNetVersion));
            ControllerVersionFeedback = new StringFeedback(new Func<string>( () => SystemMonitor.VersionInformation.ControlSystemVersion));

            //var status = string.Format("System Monitor Status: \r TimeZone: {0}\rTimeZoneText: {1}\rIOControllerVersion: {2}\rSnmpAppVersionFeedback: {3}\rBACnetAppVersionFeedback: {4}\rControllerVersionFeedback: {5}",
            //    SystemMonitor.TimeZoneInformation.TimeZoneNumber, SystemMonitor.TimeZoneInformation.TimeZoneName, SystemMonitor.VersionInformation.IOPVersion, SystemMonitor.VersionInformation.SNMPVersion,
            //    SystemMonitor.VersionInformation.BACNetVersion, SystemMonitor.VersionInformation.ControlSystemVersion);
                
            //Debug.Console(1, this, status);

            ProgramStatusFeedbackCollection = new Dictionary<uint, ProgramStatusFeedbacks>();

            foreach (var prog in SystemMonitor.ProgramCollection)
            {
                var program = new ProgramStatusFeedbacks(prog);
                ProgramStatusFeedbackCollection.Add(prog.Number, program);
            }

            SystemMonitor.ProgramChange += new ProgramStateChangeEventHandler(SystemMonitor_ProgramChange);
            SystemMonitor.TimeZoneInformation.TimeZoneChange += new TimeZoneChangeEventHandler(TimeZoneInformation_TimeZoneChange);
        }

        /// <summary>
        /// Gets data in separate thread
        /// </summary>
        /// <param name="command"></param>
        void RefreshSystemMonitorData(string command)
        {
            // this takes a while, launch a new thread
            CrestronInvoke.BeginInvoke((o) =>
                {
                    TimeZoneFeedback.FireUpdate();
                    TimeZoneTextFeedback.FireUpdate();
                    IOControllerVersionFeedback.FireUpdate();
                    SnmpVersionFeedback.FireUpdate();
                    BACnetAppVersionFeedback.FireUpdate();
                    ControllerVersionFeedback.FireUpdate();

                    OnSystemMonitorPropertiesChanged();
                }
            );
        }

        void OnSystemMonitorPropertiesChanged()
        {
            var handler = SystemMonitorPropertiesChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());   
            }
        }

        public override bool CustomActivate()
        {
            RefreshSystemMonitorData(null);

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
        void SystemMonitor_ProgramChange(Program sender, ProgramEventArgs args)
        {
            Debug.Console(2, this, "Program Change Detected for slot: {0}", sender.Number);
            Debug.Console(2, this, "Event Type: {0}", args.EventType);

            var program = ProgramStatusFeedbackCollection[sender.Number];

            if (args.EventType == eProgramChangeEventType.OperatingState)
            {
                program.ProgramStartedFeedback.FireUpdate();
                program.ProgramStoppedFeedback.FireUpdate();

                program.ProgramInfo.OperatingState = args.OperatingState;

				//program.GetProgramInfo();

				if (args.OperatingState == eProgramOperatingState.Start)
					program.GetProgramInfo();
				else
				{
					program.AggregatedProgramInfoFeedback.FireUpdate();
					program.OnProgramInfoChanged();
				}
            }
            else if (args.EventType == eProgramChangeEventType.RegistrationState)
            {
                program.ProgramRegisteredFeedback.FireUpdate();
                program.ProgramUnregisteredFeedback.FireUpdate();

                program.ProgramInfo.RegistrationState = args.RegistrationState;

                program.GetProgramInfo();
            }         
        }
		// TESTING JENKINS BUILD
        /// <summary>
        /// Responds to time zone changes and updates the appropriate feedbacks
        /// </summary>
        /// <param name="args"></param>
        void TimeZoneInformation_TimeZoneChange(TimeZoneEventArgs args)
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

                ProgramStartedFeedback = new BoolFeedback(new Func<bool>( () => Program.OperatingState == eProgramOperatingState.Start));
                ProgramStoppedFeedback = new BoolFeedback(new Func<bool>( () => Program.OperatingState == eProgramOperatingState.Stop));
                ProgramRegisteredFeedback = new BoolFeedback(new Func<bool>( () => Program.RegistrationState == eProgramRegistrationState.Register));
                ProgramUnregisteredFeedback = new BoolFeedback(new Func<bool>( () => Program.RegistrationState == eProgramRegistrationState.Unregister));

                ProgramNameFeedback = new StringFeedback(new Func<string>(() => ProgramInfo.ProgramFile));
                ProgramCompileTimeFeedback = new StringFeedback(new Func<string>(() => ProgramInfo.CompileTime));
                CrestronDataBaseVersionFeedback = new StringFeedback(new Func<string>(() => ProgramInfo.CrestronDB));
                EnvironmentVersionFeedback = new StringFeedback(new Func<string>(() => ProgramInfo.Environment));

                AggregatedProgramInfoFeedback = new StringFeedback(new Func<string>(() => JsonConvert.SerializeObject(ProgramInfo)));

                GetProgramInfo();
            }

            /// <summary>
            /// Retrieves information about a running program
            /// </summary>
            public void GetProgramInfo()
            {
                CrestronInvoke.BeginInvoke((o) =>
                {
                    Debug.Console(2, "Attempting to get program info for slot: {0}", Program.Number);

                    string response = null;

                    var success = CrestronConsole.SendControlSystemCommand(string.Format("progcomments:{0}", Program.Number), ref response);

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
                                ProgramInfo.MinFirmwareVersion = ParseConsoleData(response, "Min Firmware Version", ": ", "\n");
                                ProgramInfo.PlugInVersion = ParseConsoleData(response, "PlugInVersion", ": ", "\n");
                            }
                            else if (ProgramInfo.ProgramFile.Contains(".smw"))
                            {
                                // SIMPL Windows Program
                                ProgramInfo.FriendlyName = ParseConsoleData(response, "Friendly Name", ":", "\n");
                                ProgramInfo.SystemName = ParseConsoleData(response, "System Name", ": ", "\n");
                                ProgramInfo.CrestronDB = ParseConsoleData(response, "CrestronDB", ": ", "\n");
                                ProgramInfo.Environment = ParseConsoleData(response, "Source Env", ": ", "\n");
                                ProgramInfo.Programmer = ParseConsoleData(response, "Programmer", ": ", "\n");

                            }
                            //Debug.Console(2, "ProgramInfo: \r{0}", JsonConvert.SerializeObject(ProgramInfo));
                        }
                        else
                        {
                            Debug.Console(2, "Bad or incomplete console command response.  Initializing ProgramInfo for slot: {0}", Program.Number);

                            // Assume no valid program info.  Constructing a new object will wipe all properties
                            ProgramInfo = new ProgramInfo(Program.Number);

                            ProgramInfo.OperatingState = Program.OperatingState;
                            ProgramInfo.RegistrationState = Program.RegistrationState;
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
                });
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
                string outputData = "";

                if (data.Length > 0)
                {
                    try
                    {

                        //Debug.Console(2, "ParseConsoleData Data: {0}, Line {1}, startStirng {2}, endString {3}", data, line, startString, endString);
                        var linePosition = data.IndexOf(line);
                        var startPosition = data.IndexOf(startString, linePosition) + startString.Length;
                        var endPosition = data.IndexOf(endString, startPosition);
                        outputData = data.Substring(startPosition, endPosition - startPosition).Trim();
                        //Debug.Console(2, "ParseConsoleData Return: {0}", outputData);
                    }
                    catch (Exception e)
                    {
                        Debug.Console(1, "Error Parsing Console Data:\r{0}", e);
                    }
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

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("operatingState")]
        public eProgramOperatingState OperatingState { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
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
        public string CrestronDB { get; set; }
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
            CrestronDB = "";
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