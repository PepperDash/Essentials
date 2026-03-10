

using System;
using System.Linq;
using System.Threading;
using Timer = System.Timers.Timer;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using Serilog.Events;

namespace PepperDash.Essentials.Core.Config;

/// <summary>
/// Responsible for updating config at runtime, and writing the updates out to a local file
/// </summary>
public class ConfigWriter
{
    /// <summary>
    /// The name of the subfolder where the config file will be written
    /// </summary>
    public const string LocalConfigFolder = "LocalConfig";

    /// <summary>
    /// The amount of time in milliseconds to wait after the last config update before writing the config file.  This is to prevent multiple rapid updates from causing multiple file writes.
    /// Default is 30 seconds.
    /// </summary>
    public const long WriteTimeoutInMs = 30000;

    private static Timer WriteTimer;
    static readonly object _fileLock = new();


    static ConfigWriter()
    {
        WriteTimer = new Timer(WriteTimeoutInMs);
        WriteTimer.Elapsed += (s, e) => WriteConfigFile(null);

    }

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

            Debug.LogMessage(LogEventLevel.Debug, "Updated properties of device: '{0}'", deviceKey);

            success = true;
        }

        ResetTimer();

        return success;
    }

    /// <summary>
    /// Updates the config properties of a device
    /// </summary>
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
    /// Updates the config properties of a room
    /// </summary>
    public static bool UpdateRoomConfig(DeviceConfig config)
    {
        bool success = false;

        var roomConfigIndex = ConfigReader.ConfigObject.Rooms.FindIndex(d => d.Key.Equals(config.Key));

        if (roomConfigIndex >= 0)
        {
            ConfigReader.ConfigObject.Rooms[roomConfigIndex] = config;

            Debug.LogMessage(LogEventLevel.Debug, "Updated config of room: '{0}'", config.Key);

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
        WriteTimer.Stop();
        WriteTimer.Interval = WriteTimeoutInMs;
        WriteTimer.Start();

        Debug.LogMessage(LogEventLevel.Debug, "Config File write timer has been reset.");
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
    /// Writes the specified configuration data to a file.
    /// </summary>
    /// <param name="filePath">The path of the file to write to.</param>
    /// <param name="configData">The configuration data to write.</param>
    public static void WriteFile(string filePath, string configData)
    {
        if (WriteTimer != null)
            WriteTimer.Stop();

        Debug.LogMessage(LogEventLevel.Information, "Writing Configuration to file");

        Debug.LogMessage(LogEventLevel.Information, "Attempting to write config file: '{0}'", filePath);

        var lockAcquired = false;
        try
        {
            lockAcquired = Monitor.TryEnter(_fileLock);
            if (lockAcquired)
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
            if (lockAcquired)
                Monitor.Exit(_fileLock);
        }
    }
}