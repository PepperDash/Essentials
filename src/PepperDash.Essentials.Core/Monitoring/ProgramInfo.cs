extern alias Full;
using Crestron.SimplSharpPro.Diagnostics;
using Full::Newtonsoft.Json;
using Full::Newtonsoft.Json.Converters;

namespace PepperDash.Essentials.Core.Monitoring
{
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

            ProgramFile      = "";
            FriendlyName     = "";
            CompilerRevision = "";
            CompileTime      = "";
            Include4Dat      = "";

            SystemName  = "";
            CrestronDb  = "";
            Environment = "";
            Programmer  = "";

            ApplicationName    = "";
            ProgramTool        = "";
            MinFirmwareVersion = "";
            PlugInVersion      = "";
        }
    }
}