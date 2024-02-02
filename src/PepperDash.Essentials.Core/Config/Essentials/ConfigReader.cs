

using System;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Config;

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
                    if(configFiles.Length == 1)
                    {
                        localConfigFound = true;
                        
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
                        Debug.Console(2, "{0} config files found matching pattern", configFiles.Length);

                        if (configFiles.Length > 1)
                        {
                            Debug.Console(0, Debug.ErrorLogLevel.Error,
                                "****Error: Multiple Portal Configuration files present. Please ensure only a single file exists and reset program.****");
                            return false;
                        }
                        else if (configFiles.Length == 1)
                        {
                            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Found Portal config file: '{0}'", filePath);
                        }
                        else
                        {
                            Debug.Console(0, Debug.ErrorLogLevel.Notice, "No config file found.");
                            return false;
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

                // Generate debug statement if using a local file.
			    if (localConfigFound)
			    {
                    GetLocalFileMessage(filePath);
			    }

                // Read the file
                using (StreamReader fs = new StreamReader(filePath))
                {
                    Debug.Console(0, Debug.ErrorLogLevel.Notice, "Loading config file: '{0}'", filePath);

                    if (localConfigFound)
                    {
                        ConfigObject = JObject.Parse(fs.ReadToEnd()).ToObject<EssentialsConfig>();

                        Debug.Console(0, Debug.ErrorLogLevel.Notice, "Successfully Loaded Local Config");

                        return true;
                    }
                    else
                    {
                        var doubleObj = JObject.Parse(fs.ReadToEnd());
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

            Debug.Console(2, Debug.ErrorLogLevel.Notice, "Found Local config file: '{0}'", filePath);
            Debug.Console(0, newDebugString.ToString());
	    }

	}
}