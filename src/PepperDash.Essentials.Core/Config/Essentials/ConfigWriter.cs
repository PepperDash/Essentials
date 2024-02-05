

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Config
{
    /// <summary>
    /// Responsible for updating config at runtime, and writing the updates out to a local file
    /// </summary>
    public class ConfigWriter
    {
        public const string LocalConfigFolder = "LocalConfig";

        public const long WriteTimeout = 30000;

        public static CTimer WriteTimer;
		static CCriticalSection fileLock = new CCriticalSection();

        /// <summary>
        /// Updates the config properties of a device
        /// </summary>
        /// <param name="deviceKey"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static bool UpdateDeviceProperties(string deviceKey, JToken properties)
        {
            bool success = false;

            // Get the current device config
            var deviceConfig = ConfigReader.ConfigObject.Devices.FirstOrDefault(d => d.Key.Equals(deviceKey));

            if (deviceConfig != null)
            {
                // Replace the current properties JToken with the new one passed into this method
                deviceConfig.Properties = properties;

                Debug.Console(1, "Updated properties of device: '{0}'", deviceKey);

                success = true;
            }

            ResetTimer();

            return success;
        }

        public static bool UpdateDeviceConfig(DeviceConfig config)
        {
            bool success = false;

            var deviceConfigIndex = ConfigReader.ConfigObject.Devices.FindIndex(d => d.Key.Equals(config.Key));

            if (deviceConfigIndex >= 0)
            {
                ConfigReader.ConfigObject.Devices[deviceConfigIndex] = config;

                Debug.Console(1, "Updated config of device: '{0}'", config.Key);

                success = true;
            }

            ResetTimer();

            return success;
        }

        public static bool UpdateRoomConfig(DeviceConfig config)
        {
            bool success = false;

			var roomConfigIndex = ConfigReader.ConfigObject.Rooms.FindIndex(d => d.Key.Equals(config.Key));

			if (roomConfigIndex >= 0)
            {
                ConfigReader.ConfigObject.Rooms[roomConfigIndex] = config;

                Debug.Console(1, "Updated room of device: '{0}'", config.Key);

                success = true;
            }

            ResetTimer();

            return success;
        }

        /// <summary>
        /// Resets (or starts) the write timer
        /// </summary>
        static void ResetTimer()
        {
            if (WriteTimer == null)
                WriteTimer = new CTimer(WriteConfigFile, WriteTimeout);

            WriteTimer.Reset(WriteTimeout);

            Debug.Console(1, "Config File write timer has been reset.");
        }

        /// <summary>
        /// Writes the current config to a file in the LocalConfig subfolder
        /// </summary>
        /// <returns></returns>
        private static void WriteConfigFile(object o)
        {
            var filePath = Global.FilePathPrefix + LocalConfigFolder + Global.DirectorySeparator + "configurationFile.json";

            var configData = JsonConvert.SerializeObject(ConfigReader.ConfigObject);

            WriteFile(filePath, configData);
        }

        /// <summary>
        /// Writes
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="o"></param>
        public static void WriteFile(string filePath, string configData)
        {
            if (WriteTimer != null)
                WriteTimer.Stop();

            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Writing Configuration to file");

            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Attempting to write config file: '{0}'", filePath);

            try
            {
                if (fileLock.TryEnter())
                {
                    using (StreamWriter sw = new StreamWriter(filePath))
                    {
                        sw.Write(configData);
                        sw.Flush();
                    }
                }
                else
                {
                    Debug.Console(0, Debug.ErrorLogLevel.Error, "Unable to enter FileLock");
                }
            }
            catch (Exception e)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Error, "Error: Config write failed: \r{0}", e);
            }
            finally
            {
                if (fileLock != null && !fileLock.Disposed)
                    fileLock.Leave();

            }
        }


    }
}