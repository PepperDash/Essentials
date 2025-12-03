

using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Config;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Config
{
	/// <summary>
	/// Loads the ConfigObject from the file
	/// </summary>
	public class ConfigReader
	{
	    public const string LocalConfigPresent =
            @"
***************************************************
************* Using Local config file *************
***************************************************";

		public static EssentialsConfig ConfigObject { get; private set; }

  /// <summary>
  /// LoadConfig2 method
  /// </summary>
		public static bool LoadConfig2()
		{
			Debug.LogMessage(LogEventLevel.Information, "Loading unmerged system/template portal configuration file.");
			try
			{
                // Check for local config file first
                var filePath = Global.FilePathPrefix + ConfigWriter.LocalConfigFolder + Global.DirectorySeparator + Global.ConfigFileName;

                bool localConfigFound = false;

                Debug.LogMessage(LogEventLevel.Information, "Attempting to load Local config file: '{0}'", filePath);

                // Check for local config directory first

                var configFiles = GetConfigFiles(filePath);

                if (configFiles != null)
                {
                    if (configFiles.Length > 1)
                    {
                        Debug.LogMessage(LogEventLevel.Information,
                            "****Error: Multiple Local Configuration files present. Please ensure only a single file exists and reset program.****");
                        return false;
                    }
                    if(configFiles.Length == 1)
                    {
                        localConfigFound = true;
                        
                    }
                }
                else
                {
                    Debug.LogMessage(LogEventLevel.Information,
                        "Local Configuration file not present.", filePath);

                }

                // Check for Portal Config
                if(!localConfigFound)
                {
                    filePath = Global.FilePathPrefix + Global.ConfigFileName;

                    Debug.LogMessage(LogEventLevel.Information, "Attempting to load Portal config file: '{0}'", filePath);

                    configFiles = GetConfigFiles(filePath);

                    if (configFiles != null)
                    {
                        Debug.LogMessage(LogEventLevel.Verbose, "{0} config files found matching pattern", configFiles.Length);

                        if (configFiles.Length > 1)
                        {
                            Debug.LogMessage(LogEventLevel.Information,
                                "****Error: Multiple Portal Configuration files present. Please ensure only a single file exists and reset program.****");
                            return false;
                        }
                        else if (configFiles.Length == 1)
                        {
                            Debug.LogMessage(LogEventLevel.Information, "Found Portal config file: '{0}'", filePath);
                        }
                        else
                        {
                            Debug.LogMessage(LogEventLevel.Information, "No config file found.");
                            return false;
                        }
                    }
                    else
                    {
                        Debug.LogMessage(LogEventLevel.Information,
                            "ERROR: Portal Configuration file not present. Please load file and reset program.");
                        return false;
                    }
                }

                // Get the actual file path
                filePath = configFiles[0].FullName;

                // Generate debug statement if using a local file.
			    if (localConfigFound)
			    {
                    GetLocalFileMessage(filePath);
			    }

                // Read the file
                using (StreamReader fs = new StreamReader(filePath))
                {
                    Debug.LogMessage(LogEventLevel.Information, "Loading config file: '{0}'", filePath);

                    if (localConfigFound)
                    {
                        ConfigObject = JObject.Parse(fs.ReadToEnd()).ToObject<EssentialsConfig>();

                        Debug.LogMessage(LogEventLevel.Information, "Successfully Loaded Local Config");

                        return true;
                    }                   
                    else
                    {
                        var parsedConfig = JObject.Parse(fs.ReadToEnd());

                        // Check if it's a v2 config (check for "version" node)
                        // this means it's already merged by the Portal API
                        // from the v2 config tool
                        var isV2Config = parsedConfig["versions"] != null;
                        
                        if (isV2Config)
                        {
                            Debug.LogMessage(LogEventLevel.Information, "Config file is a v2 format, no merge necessary.");
                            ConfigObject = parsedConfig.ToObject<EssentialsConfig>();
                            Debug.LogMessage(LogEventLevel.Information, "Successfully Loaded v2 Config");
                            return true;
                        }

                        // Extract SystemUrl and TemplateUrl into final config output
                        ConfigObject = PortalConfigReader.MergeConfigs(parsedConfig).ToObject<EssentialsConfig>();

                        if (parsedConfig["system_url"] != null)
                        {
                            ConfigObject.SystemUrl = parsedConfig["system_url"].Value<string>();
                        }

                        if (parsedConfig["template_url"] != null)
                        {
                            ConfigObject.TemplateUrl = parsedConfig["template_url"].Value<string>();
                        }
                    }

                    Debug.LogMessage(LogEventLevel.Information, "Successfully Loaded Merged Config");

                    return true;
                }
			}
			catch (Exception e)
			{
                Debug.LogMessage(LogEventLevel.Information, "ERROR: Config load failed: \r{0}", e);
				return false;
			}
		}

        /// <summary>
        /// Returns all the files from the directory specified.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <summary>
        /// GetConfigFiles method
        /// </summary>
        public static FileInfo[] GetConfigFiles(string filePath)
        {
            // Get the directory
            var dir = Path.GetDirectoryName(filePath);

            if (Directory.Exists(dir))
            {
                Debug.LogMessage(LogEventLevel.Debug, "Searching in Directory '{0}'", dir);
                // Get the directory info
                var dirInfo = new DirectoryInfo(dir);

                // Get the file name
                var fileName = Path.GetFileName(filePath);
                Debug.LogMessage(LogEventLevel.Debug, "For Config Files matching: '{0}'", fileName);

                // Get the files that match from the directory
                return dirInfo.GetFiles(fileName);
            }
            else
            {
                Debug.LogMessage(LogEventLevel.Information,
                    "Directory not found: ", dir);

                return null;
            }
        }

		/// <summary>
		/// Returns the group for a given device key in config
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
        /// <summary>
        /// GetGroupForDeviceKey method
        /// </summary>
        public static string GetGroupForDeviceKey(string key)
        {
            var dev = ConfigObject.Devices.FirstOrDefault(d => d.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return dev == null ? null : dev.Group;
        }

	    private static void GetLocalFileMessage(string filePath)
	    {
            var filePathLength = filePath.Length + 2;
            var debugStringWidth = filePathLength + 12;

            if (debugStringWidth < 51)
            {
                debugStringWidth = 51;
            }
            var qualifier = (filePathLength % 2 != 0)
                ? " Using Local Config File "
                : " Using Local  Config File ";
            var bookend1 = (debugStringWidth - qualifier.Length) / 2;
            var bookend2 = (debugStringWidth - filePathLength) / 2;


	        var newDebugString = new StringBuilder()
	            .Append(CrestronEnvironment.NewLine)
                // Line 1
	            .Append(new string('*', debugStringWidth))
	            .Append(CrestronEnvironment.NewLine)
                // Line 2
	            .Append(new string('*', debugStringWidth))
	            .Append(CrestronEnvironment.NewLine)
                // Line 3
	            .Append(new string('*', 2))
	            .Append(new string(' ', debugStringWidth - 4))
	            .Append(new string('*', 2))
	            .Append(CrestronEnvironment.NewLine)
                // Line 4
	            .Append(new string('*', 2))
	            .Append(new string(' ', bookend1 - 2))
	            .Append(qualifier)
	            .Append(new string(' ', bookend1 - 2))
	            .Append(new string('*', 2))
	            .Append(CrestronEnvironment.NewLine)
                // Line 5
	            .Append(new string('*', 2))
	            .Append(new string(' ', bookend2 - 2))
	            .Append(" " + filePath + " ")
	            .Append(new string(' ', bookend2 - 2))
	            .Append(new string('*', 2))
	            .Append(CrestronEnvironment.NewLine)
                // Line 6
	            .Append(new string('*', 2))
	            .Append(new string(' ', debugStringWidth - 4))
	            .Append(new string('*', 2))
	            .Append(CrestronEnvironment.NewLine)
                // Line 7
	            .Append(new string('*', debugStringWidth))
	            .Append(CrestronEnvironment.NewLine)
                // Line 8
	            .Append(new string('*', debugStringWidth));

            Debug.LogMessage(LogEventLevel.Verbose, "Found Local config file: '{0}'", filePath);
            Debug.LogMessage(LogEventLevel.Information, newDebugString.ToString());
	    }

	}
}