using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;


namespace PepperDash.Core
{
    /// <summary>
    /// Represents a debugging context
    /// </summary>
    public class DebugContext
    {
        /// <summary>
        /// Describes the folder location where a given program stores it's debug level memory. By default, the
        /// file written will be named appNdebug where N is 1-10.
        /// </summary>
        public string Key { get; private set; }

        ///// <summary>
        ///// The name of the file containing the current debug settings.
        ///// </summary>
        //string FileName = string.Format(@"\nvram\debug\app{0}Debug.json", InitialParametersClass.ApplicationNumber);

        DebugContextSaveData SaveData;

        int SaveTimeoutMs = 30000;

        CTimer SaveTimer;


        static List<DebugContext> Contexts = new List<DebugContext>();

        /// <summary>
        /// Creates or gets a debug context
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static DebugContext GetDebugContext(string key)
        {
            var context = Contexts.FirstOrDefault(c => c.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (context == null)
            {
                context = new DebugContext(key);
                Contexts.Add(context);
            }
            return context;
        }

        /// <summary>
        /// Do not use.  For S+ access.
        /// </summary>
        public DebugContext() { }

        DebugContext(string key)
        {
            Key = key;
            if (CrestronEnvironment.RuntimeEnvironment == eRuntimeEnvironment.SimplSharpPro)
            {
                // Add command to console
                CrestronConsole.AddNewConsoleCommand(SetDebugFromConsole, "appdebug",
                    "appdebug:P [0-2]: Sets the application's console debug message level",
                    ConsoleAccessLevelEnum.AccessOperator);
            }

            CrestronEnvironment.ProgramStatusEventHandler += CrestronEnvironment_ProgramStatusEventHandler;

            LoadMemory();
        }

        /// <summary>
        /// Used to save memory when shutting down
        /// </summary>
        /// <param name="programEventType"></param>
        void CrestronEnvironment_ProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            if (programEventType == eProgramStatusEventType.Stopping)
            {
                if (SaveTimer != null)
                {
                    SaveTimer.Stop();
                    SaveTimer = null;
                }
                Console(0, "Saving debug settings");
                SaveMemory();
            }
        }

        /// <summary>
        /// Callback for console command
        /// </summary>
        /// <param name="levelString"></param>
        public void SetDebugFromConsole(string levelString)
        {
            try
            {
                if (string.IsNullOrEmpty(levelString.Trim()))
                {
                    CrestronConsole.ConsoleCommandResponse("AppDebug level = {0}", SaveData.Level);
                    return;
                }

                SetDebugLevel(Convert.ToInt32(levelString));
            }
            catch
            {
                CrestronConsole.PrintLine("Usage: appdebug:P [0-2]");
            }
        }

        /// <summary>
        /// Sets the debug level
        /// </summary>
        /// <param name="level"> Valid values 0 (no debug), 1 (critical), 2 (all messages)</param>
        public void SetDebugLevel(int level)
        {
            if (level <= 2)
            {
                SaveData.Level = level;
                SaveMemoryOnTimeout();

                CrestronConsole.PrintLine("[Application {0}], Debug level set to {1}",
                    InitialParametersClass.ApplicationNumber, SaveData.Level);
            }
        }

        /// <summary>
        /// Prints message to console if current debug level is equal to or higher than the level of this message.
        /// Uses CrestronConsole.PrintLine.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="format">Console format string</param>
        /// <param name="items">Object parameters</param>
        public void Console(uint level, string format, params object[] items)
        {
            if (SaveData.Level >= level)
                CrestronConsole.PrintLine("App {0}:{1}", InitialParametersClass.ApplicationNumber,
                    string.Format(format, items));
        }

        /// <summary>
        /// Appends a device Key to the beginning of a message
        /// </summary>
        public void Console(uint level, IKeyed dev, string format, params object[] items)
        {
            if (SaveData.Level >= level)
                Console(level, "[{0}] {1}", dev.Key, string.Format(format, items));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="dev"></param>
        /// <param name="errorLogLevel"></param>
        /// <param name="format"></param>
        /// <param name="items"></param>
        public void Console(uint level, IKeyed dev, Debug.ErrorLogLevel errorLogLevel,
            string format, params object[] items)
        {
            if (SaveData.Level >= level)
            {
                var str = string.Format("[{0}] {1}", dev.Key, string.Format(format, items));
                Console(level, str);
                LogError(errorLogLevel, str);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="errorLogLevel"></param>
        /// <param name="format"></param>
        /// <param name="items"></param>
        public void Console(uint level, Debug.ErrorLogLevel errorLogLevel,
            string format, params object[] items)
        {
            if (SaveData.Level >= level)
            {
                var str = string.Format(format, items);
                Console(level, str);
                LogError(errorLogLevel, str);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorLogLevel"></param>
        /// <param name="str"></param>
        public void LogError(Debug.ErrorLogLevel errorLogLevel, string str)
        {
            string msg = string.Format("App {0}:{1}", InitialParametersClass.ApplicationNumber, str);
            switch (errorLogLevel)
            {
                case Debug.ErrorLogLevel.Error:
                    ErrorLog.Error(msg);
                    break;
                case Debug.ErrorLogLevel.Warning:
                    ErrorLog.Warn(msg);
                    break;
                case Debug.ErrorLogLevel.Notice:
                    ErrorLog.Notice(msg);
                    break;
            }
        }

        /// <summary>
        /// Writes the memory object after timeout
        /// </summary>
        void SaveMemoryOnTimeout()
        {
            if (SaveTimer == null)
                SaveTimer = new CTimer(o =>
                {
                    SaveTimer = null;
                    SaveMemory();
                }, SaveTimeoutMs);
            else
                SaveTimer.Reset(SaveTimeoutMs);
        }

        /// <summary>
        /// Writes the memory - use SaveMemoryOnTimeout
        /// </summary>
        void SaveMemory()
        {
            using (StreamWriter sw = new StreamWriter(GetMemoryFileName()))
            {
                var json = JsonConvert.SerializeObject(SaveData);
                sw.Write(json);
                sw.Flush();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void LoadMemory()
        {
            var file = GetMemoryFileName();
            if (File.Exists(file))
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    var data = JsonConvert.DeserializeObject<DebugContextSaveData>(sr.ReadToEnd());
                    if (data != null)
                    {
                        SaveData = data;
                        Debug.Console(1, "Debug memory restored from file");
                        return;
                    }
                    else
                        SaveData = new DebugContextSaveData();
                }
            }
        }

        /// <summary>
        /// Helper to get the file path for this app's debug memory
        /// </summary>
        string GetMemoryFileName()
        {
            return string.Format(@"\NVRAM\debugSettings\program{0}-{1}", InitialParametersClass.ApplicationNumber, Key);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DebugContextSaveData
    {
        /// <summary>
        /// 
        /// </summary>
        public int Level { get; set; }
    }
}