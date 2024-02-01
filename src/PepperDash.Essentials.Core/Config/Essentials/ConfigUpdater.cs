

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharpPro.Diagnostics;

using PepperDash.Core;

namespace PepperDash.Essentials.Core.Config
{
    public static class ConfigUpdater
    {
        public static event EventHandler<ConfigStatusEventArgs> ConfigStatusChanged;

        public static void GetConfigFromServer(string url)
        {
            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Attempting to get new config from '{0}'", url);

            // HTTP GET 
            var req = new HttpClientRequest();

            try
            {
                req.RequestType = RequestType.Get;
                req.Url.Parse(url);

                new HttpClient().DispatchAsync(req, (r, e) =>
                    {
                        if (e == HTTP_CALLBACK_ERROR.COMPLETED)
                        {
                            if (r.Code == 200)
                            {
                                var newConfig = r.ContentString;

                                OnStatusUpdate(eUpdateStatus.ConfigFileReceived);

                                ArchiveExistingPortalConfigs();

                                CheckForLocalConfigAndDelete();

                                WriteConfigToFile(newConfig);

                                RestartProgram();
                            }
                            else
                            {
                                Debug.Console(0, Debug.ErrorLogLevel.Error, "Config Update Process Stopped. Failed to get config file from server: {0}", r.Code);
                                OnStatusUpdate(eUpdateStatus.UpdateFailed);
                            }
                        }
                        else
                            Debug.Console(0, Debug.ErrorLogLevel.Error, "Request for config from Server Failed: {0}", e);
                    });
            }
            catch (Exception e)
            {
                Debug.Console(1, "Error Getting Config from Server: {0}", e);
            }

        }

        static void OnStatusUpdate(eUpdateStatus status)
        {
            var handler = ConfigStatusChanged;

            if(handler != null)
            {
                handler(typeof(ConfigUpdater), new ConfigStatusEventArgs(status));
            }
        }

        static void WriteConfigToFile(string configData)
        {
            var filePath = Global.FilePathPrefix+ "configurationFile-updated.json";

            try
            {
                var config = JObject.Parse(configData).ToObject<EssentialsConfig>();

                ConfigWriter.WriteFile(filePath, configData);

                OnStatusUpdate(eUpdateStatus.WritingConfigFile);
            }
            catch (Exception e)
            {
                Debug.Console(1, "Error parsing new config: {0}", e);

                OnStatusUpdate(eUpdateStatus.UpdateFailed);
            }           
        }

        /// <summary>
        /// Checks for any existing portal config files and archives them
        /// </summary>
        static void ArchiveExistingPortalConfigs()
        {
            var filePath = Global.FilePathPrefix + Global.ConfigFileName;

            var configFiles = ConfigReader.GetConfigFiles(filePath);

            if (configFiles != null)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Existing config files found.  Moving to Archive folder.");

                OnStatusUpdate(eUpdateStatus.ArchivingConfigs);

                MoveFilesToArchiveFolder(configFiles);
            }
            else
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "No Existing config files found in '{0}'. Nothing to archive", filePath);
            }
        }

        /// <summary>
        /// Checks for presence of archive folder and if found deletes contents.
        /// Moves any config files to the archive folder and adds a .bak suffix
        /// </summary>
        /// <param name="files"></param>
        static void MoveFilesToArchiveFolder(FileInfo[] files)
        {
            string archiveDirectoryPath = Global.FilePathPrefix + "archive";

            if (!Directory.Exists(archiveDirectoryPath))
            {
                // Directory does not exist, create it
                Directory.Create(archiveDirectoryPath);
            }
            else
            {
                // Directory exists, first clear any contents
                var archivedConfigFiles = ConfigReader.GetConfigFiles(archiveDirectoryPath + Global.DirectorySeparator + Global.ConfigFileName + ".bak");

                if(archivedConfigFiles != null || archivedConfigFiles.Length > 0)
                {
                    Debug.Console(0, Debug.ErrorLogLevel.Notice, "{0} Existing files found in archive folder.  Deleting.", archivedConfigFiles.Length);

                    for (int i = 0; i < archivedConfigFiles.Length; i++ )
                    {
                        var file = archivedConfigFiles[i];
                        Debug.Console(0, Debug.ErrorLogLevel.Notice, "Deleting archived file: '{0}'", file.FullName);
                        file.Delete();
                    }
                }

            }

            // Move any files from the program folder to the archive folder
            foreach (var file in files)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Moving config file '{0}' to archive folder", file.FullName);

                // Moves the file and appends the .bak extension
                var fileDest = archiveDirectoryPath + "/" + file.Name + ".bak";
                if(!File.Exists(fileDest))
                {
                  file.MoveTo(fileDest);
                }
                else
                    Debug.Console(0, Debug.ErrorLogLevel.Warning, "Cannot move file to archive folder.  Existing file already exists with same name: '{0}'", fileDest);
            }
        }

        /// <summary>
        /// Checks for LocalConfig folder in file system and deletes if found
        /// </summary>
        static void CheckForLocalConfigAndDelete()
        {
            var folderPath = Global.FilePathPrefix + ConfigWriter.LocalConfigFolder;

            if (Directory.Exists(folderPath))
            {
                OnStatusUpdate(eUpdateStatus.DeletingLocalConfig);
                Directory.Delete(folderPath);
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Local Config Found in '{0}'. Deleting.", folderPath);
            }
        }

        /// <summary>
        /// Connects to the processor via SSH and restarts the program
        /// </summary>
        static void RestartProgram()
        {
            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Attempting to Reset Program");

            OnStatusUpdate(eUpdateStatus.RestartingProgram);

            string response = string.Empty;

            CrestronConsole.SendControlSystemCommand(string.Format("progreset -p:{0}", InitialParametersClass.ApplicationNumber), ref response);

            Debug.Console(1, "Console Response: {0}", response);          
        }

    }

        public enum eUpdateStatus
    {
        UpdateStarted,
        ConfigFileReceived,
        ArchivingConfigs,
        DeletingLocalConfig,
        WritingConfigFile,
        RestartingProgram,
        UpdateSucceeded,
        UpdateFailed
    }

    public class ConfigStatusEventArgs : EventArgs
    {
        public eUpdateStatus UpdateStatus { get; private set; }

        public ConfigStatusEventArgs(eUpdateStatus status)
        {
            UpdateStatus = status;
        }
    }
}