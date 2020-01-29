using System;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronDataStore;
using Crestron.SimplSharpPro;

//using PepperDash.Essentials.Core.Http;
using PepperDash.Essentials.License;



namespace PepperDash.Essentials.Core
{
	public static class Global
	{
		public static CrestronControlSystem ControlSystem { get; set; }

		public static LicenseManager LicenseManager { get; set; }

        /// <summary>
        /// The file path prefix to the folder containing configuration files
        /// </summary>
        public static string FilePathPrefix { get; private set; }

        /// <summary>
        /// Returns the directory separator character based on the running OS
        /// </summary>
        public static char DirectorySeparator
        {
            get
            {
                return System.IO.Path.DirectorySeparatorChar;
            }
        }

        /// <summary>
        /// Wildcarded config file name for global reference
        /// </summary>
        public const string ConfigFileName = "*configurationFile*.json";

        /// <summary>
        /// Sets the file path prefix
        /// </summary>
        /// <param name="prefix"></param>
        public static void SetFilePathPrefix(string prefix)
        {
            FilePathPrefix = prefix;
        }

        /// <summary>
        /// Gets the Assembly Version of Essentials
        /// </summary>
        /// <returns>The Assembly Version at Runtime</returns>
        public static string GetAssemblyVersion()
        {
            var version = Crestron.SimplSharp.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        /// <summary>
        /// Checks to see if the running version meets or exceed the minimum specified version.  For beta versions (0.xx.yy), will always return true.
        /// </summary>
        /// <param name="minimumVersion">Minimum specified version in format of xx.yy.zz</param>
        /// <returns>Returns true if the running version meets or exceeds the minimum specified version</returns>
        public static bool IsRunningMinimumVersionOrHigher(string minimumVersion)
        {
            var runtimeVersion = Crestron.SimplSharp.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            Debug.Console(2, "Comparing running version '{0}' to minimum version '{1}'", GetAssemblyVersion(), minimumVersion);

            if (runtimeVersion.Major == 0)
            {
                Debug.Console(2, "Running Beta Build.  Bypassing Dependency Check.");
                return true;
            }

            var minVersion = Regex.Match(minimumVersion, @"^(\d*).(\d*).(\d*)$");

            if(!minVersion.Success)
            {
                Debug.Console(2, "minimumVersion String does not match format xx.yy.zz");
                return false;

            }

            var minVersionMajor = Int16.Parse(minVersion.Groups[1].Value);
            var minVersionMinor = Int16.Parse(minVersion.Groups[2].Value);
            var minVersionBuild = Int16.Parse(minVersion.Groups[3].Value);

            if (minVersionMajor < runtimeVersion.Major)
                return false;

            if (minVersionMinor < runtimeVersion.Minor)
                return false;

            if (minVersionBuild < runtimeVersion.Build)
                return false;

            return true;
        }

        /// <summary>
        /// Attempts to validate the JSON against the specified schema
        /// </summary>
        /// <param name="json">JSON to be validated</param>
        /// <param name="schemaFileName">File name of schema to validate against</param>
        public static void ValidateSchema(string json, string schemaFileName)
        {
            try
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Validating Config against Schema...");
                JObject config = JObject.Parse(json);

                Debug.Console(2, "Config: \n{0}", json);

                using (StreamReader fileStream = new StreamReader(schemaFileName))
                {
                    JsonSchemaResolver resolver = new JsonSchemaResolver();

                    JsonSchema schema = JsonSchema.Parse(fileStream.ReadToEnd(), resolver);

                    
                    Debug.Console(2, "Schema: \n{0}", schema.ToString());

                    if (config.IsValid(schema))
                        Debug.Console(0, Debug.ErrorLogLevel.Notice, "Configuration successfully Validated Against Schema");
                    else
                    {
                        Debug.Console(0, Debug.ErrorLogLevel.Warning, "Validation Errors Found in Configuration:");
                        config.Validate(schema, Json_ValidationEventHandler);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Console(1, "Error in ValidateSchema: {0}", e);
            }
        }



        /// <summary>
        /// Event Handler callback for JSON validation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void Json_ValidationEventHandler(object sender, ValidationEventArgs args)
        {
            Debug.Console(0, Debug.ErrorLogLevel.Error, "JSON Validation error at line {0} position {1}: {2}", args.Exception.LineNumber, args.Exception.LinePosition, args.Message);
        }

		static Global()
		{
			// Fire up CrestronDataStoreStatic
			var err = CrestronDataStoreStatic.InitCrestronDataStore();
			if (err != CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
			{
				CrestronConsole.PrintLine("Error starting CrestronDataStoreStatic: {0}", err);
				return;
			}
		}

	}
}