using System;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.CrestronDataStore;
using Crestron.SimplSharpPro;

using PepperDash.Core;
using PepperDash.Essentials.License;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;


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
        /// The file path prefix to the applciation directory
        /// </summary>
        public static string ApplicationDirectoryPathPrefix 
        {
            get
            {
                return Crestron.SimplSharp.CrestronIO.Directory.GetApplicationDirectory();
            }
        }

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

        static string _AssemblyVersion;

        /// <summary>
        /// Gets the Assembly Version of Essentials
        /// </summary>
        /// <returns>The Assembly Version at Runtime</returns>
        public static string AssemblyVersion
        {
            get
            {
                return _AssemblyVersion;
            }
            private set
            {
                _AssemblyVersion = value;
            }
        }

        /// <summary>
        /// Sets the Assembly version to the version of the Essentials Library
        /// </summary>
        /// <param name="assemblyVersion"></param>
        public static void SetAssemblyVersion(string assemblyVersion)
        {
            AssemblyVersion = assemblyVersion;
        }

        /// <summary>
        /// Checks to see if the running version meets or exceed the minimum specified version.  For beta versions (0.xx.yy), will always return true.
        /// </summary>
        /// <param name="minimumVersion">Minimum specified version in format of xx.yy.zz</param>
        /// <returns>Returns true if the running version meets or exceeds the minimum specified version</returns>
        public static bool IsRunningMinimumVersionOrHigher(string minimumVersion)
        {
            Debug.Console(2, "Comparing running version '{0}' to minimum version '{1}'", AssemblyVersion, minimumVersion);

            if (String.IsNullOrEmpty(minimumVersion))
            {
                Debug.Console(0,"Plugin does not specify a minimum version. Loading plugin may not work as expected. Proceeding with loading plugin");
                return true;
            }
            
            var runtimeVersion = Regex.Match(AssemblyVersion, @"^(\d*).(\d*).(\d*).*");

            var runtimeVersionMajor = Int16.Parse(runtimeVersion.Groups[1].Value);
            var runtimeVersionMinor = Int16.Parse(runtimeVersion.Groups[2].Value);
            var runtimeVersionBuild = Int16.Parse(runtimeVersion.Groups[3].Value);

            var runtimeVer = new Version(runtimeVersionMajor, runtimeVersionMinor, runtimeVersionBuild);

            Version minimumVer;
            try
            {
                minimumVer = new Version(minimumVersion);
            }
            catch
            {
                Debug.Console(2, "unable to parse minimum version {0}. Bypassing plugin load.", minimumVersion);
                return false;
            }


            // Check for beta build version
            if (runtimeVer.Major != 0)
            {
                return runtimeVer.CompareTo(minimumVer) >= 0;
            }

            Debug.Console(2, "Running Local Build.  Bypassing Dependency Check.");
            return true;

            /*
            var minVersion = Regex.Match(minimumVersion, @"^(\d*).(\d*).(\d*)$");

            if(!minVersion.Success)
            {
                
            }

            var minVersionMajor = Int16.Parse(minVersion.Groups[1].Value);
            var minVersionMinor = Int16.Parse(minVersion.Groups[2].Value);
            var minVersionBuild = Int16.Parse(minVersion.Groups[3].Value);



            if (minVersionMajor > runtimeVersionMajor)
                return false;

            if (minVersionMinor > runtimeVersionMinor)
                return false;

            if (minVersionBuild > runtimeVersionBuild)
                return false;

            return true;
             */
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