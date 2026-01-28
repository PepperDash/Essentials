

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Config
{
    /// <summary>
    /// Responsible for updating config at runtime, and writing the updates out to a local file
    /// </summary>
    public class ConfigWriter
    {
        /// <summary>
        /// LocalConfigFolder constant
        /// </summary>
        public const string LocalConfigFolder = "LocalConfig";

        /// <summary>
        /// WriteTimeout constant
        /// </summary>
        public const long WriteTimeout = 30000;

        /// <summary>
        /// WriteTimer variable
        /// </summary>
        public static CTimer WriteTimer;

		static CCriticalSection fileLock = new CCriticalSection();

        /// <summary>
        /// Updates the config properties of a device
        /// </summary>
        /// <param name="deviceKey">The key of the device to update</param>
        /// <param name="properties">The new properties for the device</param>
        /// <returns>True if the update was successful, otherwise false</returns>
        public static bool UpdateDeviceProperties(string deviceKey, JToken properties)
        {
            bool success = false;

            // Get the current device config
            var deviceConfig = ConfigReader.ConfigObject.Devices.FirstOrDefault(d => d.Key.Equals(deviceKey));

            if (deviceConfig != null)
            {
                // Replace the current properties JToken with the new one passed into this method
                deviceConfig.Properties = properties;

                Debug.LogMessage(LogEventLevel.Debug, "Updated properties of device: '{0}'", deviceKey);

                success = true;
            }

            ResetTimer();

            return success;
        }

        /// <summary>
        /// UpdateDeviceConfig method
        /// </summary>
        /// <param name="config">The new device config</param>
        /// <returns>True if the update was successful, otherwise false</returns>
        public static bool UpdateDeviceConfig(DeviceConfig config)
        {
            bool success = false;

            var deviceConfigIndex = ConfigReader.ConfigObject.Devices.FindIndex(d => d.Key.Equals(config.Key));

            if (deviceConfigIndex >= 0)
            {
                ConfigReader.ConfigObject.Devices[deviceConfigIndex] = config;

                Debug.LogMessage(LogEventLevel.Debug, "Updated config of device: '{0}'", config.Key);

                success = true;
            }

            ResetTimer();

            return success;
        }

        /// <summary>
        /// UpdateRoomConfig method
        /// </summary>
        /// <param name="config">The new room config</param>
        /// <returns>True if the update was successful, otherwise false</returns>
        public static bool UpdateRoomConfig(DeviceConfig config)
        {
            bool success = false;

			var roomConfigIndex = ConfigReader.ConfigObject.Rooms.FindIndex(d => d.Key.Equals(config.Key));

			if (roomConfigIndex >= 0)
            {
                ConfigReader.ConfigObject.Rooms[roomConfigIndex] = config;

                Debug.LogMessage(LogEventLevel.Debug, "Updated room of device: '{0}'", config.Key);

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

            Debug.LogMessage(LogEventLevel.Debug, "Config File write timer has been reset.");
        }

        /// <summary>
        /// Writes the current config to a file in the LocalConfig subfolder
        /// </summary>
        private static void WriteConfigFile(object o)
        {
            var filePath = Global.FilePathPrefix + LocalConfigFolder + Global.DirectorySeparator + "configurationFile.json";

            var configData = JsonConvert.SerializeObject(ConfigReader.ConfigObject);

            WriteFile(filePath, configData);
        }

        /// <summary>
        /// Writes the current config data to a file
        /// </summary>
        /// <param name="filePath">The file path to write to</param>
        /// <param name="configData">The config data to write</param>
        public static void WriteFile(string filePath, string configData)
        {
            if (WriteTimer != null)
                WriteTimer.Stop();

            Debug.LogMessage(LogEventLevel.Information, "Writing Configuration to file");

            Debug.LogMessage(LogEventLevel.Information, "Attempting to write config file: '{0}'", filePath);

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
                    Debug.LogMessage(LogEventLevel.Information, "Unable to enter FileLock");
                }
            }
            catch (Exception e)
            {
                Debug.LogMessage(LogEventLevel.Information, "Error: Config write failed: \r{0}", e);
            }
            finally
            {
                if (fileLock != null && !fileLock.Disposed)
                    fileLock.Leave();

            }
        }


    }
}