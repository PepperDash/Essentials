using System;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using PepperDash.Core;
using PepperDash.Core.Config;

namespace PepperDash.Essentials.Core.Config
{
	/// <summary>
	/// Loads the ConfigObject from the file
	/// </summary>
	public class ConfigReader
	{
		public static EssentialsConfig ConfigObject { get; private set; }

		public static bool LoadConfig2()
		{
			Debug.Console(0, Debug.ErrorLogLevel.Notice, "Loading unmerged system/template portal configuration file.");
			try
			{
                // Check for local config file first
                var filePath = Global.FilePathPrefix + ConfigWriter.LocalConfigFolder + Global.DirectorySeparator + Global.ConfigFileName;

                bool localConfigFound = false;

                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Attempting to load Local config file: '{0}'", filePath);

                // Check for local config directory first

                var configFiles = GetConfigFiles(filePath);

                if (configFiles != null)
                {
                    if (configFiles.Length > 1)
                    {
                        Debug.Console(0, Debug.ErrorLogLevel.Error,
                            "****Error: Multiple Local Configuration files present. Please ensure only a single file exists and reset program.****");
                        return false;
                    }
                    else if(configFiles.Length == 1)
                    {
                        localConfigFound = true;
                        Debug.Console(0, Debug.ErrorLogLevel.Notice, "Found Local config file: '{0}'", filePath);
                    }
                }
                else
                {
                    Debug.Console(0, Debug.ErrorLogLevel.Notice,
                        "Local Configuration file not present.", filePath);

                }

                // Check for Portal Config
                if(!localConfigFound)
                {
                    filePath = Global.FilePathPrefix + Global.ConfigFileName;

                    Debug.Console(0, Debug.ErrorLogLevel.Notice, "Attempting to load Portal config file: '{0}'", filePath);

                    configFiles = GetConfigFiles(filePath);

                    if (configFiles != null)
                    {
                        if (configFiles.Length > 1)
                        {
                            Debug.Console(0, Debug.ErrorLogLevel.Error,
                                "****Error: Multiple Portal Configuration files present. Please ensure only a single file exists and reset program.****");
                            return false;
                        }
                        else
                        {
                            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Found Portal config file: '{0}'", filePath);
                        }
                    }
                    else
                    {
                        Debug.Console(0, Debug.ErrorLogLevel.Error,
                            "ERROR: Portal Configuration file not present. Please load file and reset program.");
                        return false;
                    }
                }

                // Get the actual file path
                filePath = configFiles[0].FullName;

                // Read the file
                using (StreamReader fs = new StreamReader(filePath))
                {
                    Debug.Console(0, Debug.ErrorLogLevel.Notice, "Loading config file: '{0}'", filePath);

                    var directoryPrefix = string.Format("{0}Config{1}Schema{1}", Global.ApplicationDirectoryPrefix, Global.DirectorySeparator);

                    var schemaFilePath = directoryPrefix + "EssentialsConfigSchema.json";
                    Debug.Console(0, Debug.ErrorLogLevel.Notice, "Loading Schema from path: {0}", schemaFilePath);

                    var jsonConfig = fs.ReadToEnd();

                    if(File.Exists(schemaFilePath))
                    {
                        // Attempt to validate config against schema
                        ValidateSchema(jsonConfig, schemaFilePath);
                    }
                    else
                        Debug.Console(0, Debug.ErrorLogLevel.Warning, "No Schema found at path: {0}", schemaFilePath);

                    if (localConfigFound)
                    {
                        ConfigObject = JObject.Parse(jsonConfig).ToObject<EssentialsConfig>();

                        Debug.Console(0, Debug.ErrorLogLevel.Notice, "Successfully Loaded Local Config");

                        return true;
                    }
                    else
                    {
                        var doubleObj = JObject.Parse(jsonConfig);
                        ConfigObject = PortalConfigReader.MergeConfigs(doubleObj).ToObject<EssentialsConfig>();

                        // Extract SystemUrl and TemplateUrl into final config output

                        if (doubleObj["system_url"] != null)
                        {
                            ConfigObject.SystemUrl = doubleObj["system_url"].Value<string>();
                        }

                        if (doubleObj["template_url"] != null)
                        {
                            ConfigObject.TemplateUrl = doubleObj["template_url"].Value<string>();
                        }
                    }

                    Debug.Console(0, Debug.ErrorLogLevel.Notice, "Successfully Loaded Merged Config");

                    return true;
                }
			}
			catch (Exception e)
			{
                Debug.Console(0, Debug.ErrorLogLevel.Error, "ERROR: Config load failed: \r{0}", e);
				return false;
			}
		}

        /// <summary>
        /// Attempts to validate the JSON against the specified schema
        /// </summary>
        /// <param name="json">JSON to be validated</param>
        /// <param name="schemaFileName">File name of schema to validate against</param>
        public static void ValidateSchema(string json, string schemaFileName)
        {
            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Validating Config File against Schema...");
            JObject config = JObject.Parse(json);

            using (StreamReader fileStream = new StreamReader(schemaFileName))
            {
                JsonSchema schema = JsonSchema.Parse(fileStream.ReadToEnd());

                if (config.IsValid(schema))
                    Debug.Console(0, Debug.ErrorLogLevel.Notice, "Configuration successfully Validated Against Schema");
                else
                {
                    Debug.Console(0, Debug.ErrorLogLevel.Warning, "Validation Errors Found in Configuration:");
                    config.Validate(schema, Json_ValidationEventHandler);
                }              
            }
        }

        /// <summary>
        /// Event Handler callback for JSON validation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void Json_ValidationEventHandler(object sender, ValidationEventArgs args)
        {
            Debug.Console(0, "JSON Validation error at line {0} position {1}: {2}", args.Exception.LineNumber, args.Exception.LinePosition, args.Message);
        }

        /// <summary>
        /// Returns all the files from the directory specified.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static FileInfo[] GetConfigFiles(string filePath)
        {
            // Get the directory
            var dir = Path.GetDirectoryName(filePath);

            if (Directory.Exists(dir))
            {
                Debug.Console(1, "Searching in Directory '{0}'", dir);
                // Get the directory info
                var dirInfo = new DirectoryInfo(dir);

                // Get the file name
                var fileName = Path.GetFileName(filePath);
                Debug.Console(1, "For Config Files matching: '{0}'", fileName);

                // Get the files that match from the directory
                return dirInfo.GetFiles(fileName);
            }
            else
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice,
                    "Directory not found: ", dir);

                return null;
            }
        }

		/// <summary>
		/// Returns the group for a given device key in config
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
        public static string GetGroupForDeviceKey(string key)
        {
            var dev = ConfigObject.Devices.FirstOrDefault(d => d.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return dev == null ? null : dev.Group;
        }

	}
}