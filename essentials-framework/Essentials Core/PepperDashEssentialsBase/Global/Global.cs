using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronDataStore;
using Crestron.SimplSharpPro;

//using PepperDash.Essentials.Core.Http;
using PepperDash.Essentials.License;



namespace PepperDash.Essentials.Core
{
    /// <summary>
    /// Global application properties
    /// </summary>
	public static class Global
	{
        /// <summary>
        /// The control system the application is running on
        /// </summary>
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
        /// The file path prefix to the folder containing the application files (including embedded resources)
        /// </summary>
        public static string ApplicationDirectoryPrefix 
        {
            get
            {
                string fmt = "00.##";
                var appNumber = InitialParametersClass.ApplicationNumber.ToString(fmt);
                return  string.Format("{0}{1}Simpl{1}app{2}{1}", Crestron.SimplSharp.CrestronIO.Directory.GetApplicationRootDirectory(), Global.DirectorySeparator,appNumber );
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