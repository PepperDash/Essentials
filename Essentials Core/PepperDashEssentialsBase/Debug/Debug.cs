//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharp.CrestronDataStore;
//using Crestron.SimplSharpPro;


//namespace PepperDash.Essentials.Core
//{
//    public class Debug
//    {
//        public static uint Level { get; private set; }

//        /// <summary>
//        /// This should called from the ControlSystem Initiailize method.
//        /// </summary>
//        public static void Initialize()
//        {
//            // Add command to console
//            CrestronConsole.AddNewConsoleCommand(SetDebugFromConsole, "appdebug", 
//                "appdebug:P [0-2]: Sets the application's console debug message level", 
//                ConsoleAccessLevelEnum.AccessOperator);

//            uint level = 0;
//            var err = CrestronDataStoreStatic.GetGlobalUintValue("DebugLevel", out level);
//            if (err == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
//                SetDebugLevel(level);
//            else if (err == CrestronDataStore.CDS_ERROR.CDS_RECORD_NOT_FOUND)
//                CrestronDataStoreStatic.SetGlobalUintValue("DebugLevel", 0);
//            else
//                CrestronConsole.PrintLine("Error restoring console debug level setting: {0}", err);
//        }

//        /// <summary>
//        /// Callback for console command
//        /// </summary>
//        /// <param name="levelString"></param>
//        public static void SetDebugFromConsole(string levelString)
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(levelString.Trim()))
//                {
//                    CrestronConsole.PrintLine("AppDebug level = {0}", Level);
//                    return;
//                }

//                SetDebugLevel(Convert.ToUInt32(levelString));
//            }
//            catch
//            {
//                CrestronConsole.PrintLine("Usage: appdebug:P [0-2]");
//            }
//        }

//        /// <summary>
//        /// Sets the debug level
//        /// </summary>
//        /// <param name="level"> Valid values 0 (no debug), 1 (critical), 2 (all messages)</param>
//        public static void SetDebugLevel(uint level)
//        {
//            if (level <= 2)
//            {
//                Level = 2;
//                CrestronConsole.PrintLine("[Application {0}], Debug level set to {1}", 
//                    InitialParametersClass.ApplicationNumber, level);
//                var err = CrestronDataStoreStatic.SetGlobalUintValue("DebugLevel", level);
//                if(err != CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
//                    CrestronConsole.PrintLine("Error saving console debug level setting: {0}", err);
//            }
//        }

//        /// <summary>
//        /// Prints message to console if current debug level is equal to or higher than the level of this message.
//        /// Uses CrestronConsole.PrintLine.
//        /// </summary>
//        /// <param name="level"></param>
//        /// <param name="format">Console format string</param>
//        /// <param name="items">Object parameters</param>
//        public static void Console(uint level, string format, params object[] items)
//        {
//            if (Level >= level)
//                CrestronConsole.PrintLine("App {0}:{1}", InitialParametersClass.ApplicationNumber,
//                    string.Format(format, items));
//        }

//        /// <summary>
//        /// Appends a device Key to the beginning of a message
//        /// </summary>
//        public static void Console(uint level, IKeyed dev, string format, params object[] items)
//        {
//            if (Level >= level)
//                Console(level, "[{0}] {1}", dev.Key, string.Format(format, items));
//        }

//        public static void Console(uint level, IKeyed dev, ErrorLogLevel errorLogLevel, 
//            string format, params object[] items)
//        {
//            if (Level >= level)
//            {
//                var str = string.Format("[{0}] {1}", dev.Key, string.Format(format, items));
//                Console(level, str);
//                LogError(errorLogLevel, str);
//            }
//        }

//        public static void Console(uint level, ErrorLogLevel errorLogLevel,
//            string format, params object[] items)
//        {
//            if (Level >= level)
//            {
//                var str = string.Format(format, items);
//                Console(level, str);
//                LogError(errorLogLevel, str);
//            }
//        }

//        public static void LogError(ErrorLogLevel errorLogLevel, string str)
//        {
//            string msg = string.Format("App {0}:{1}", InitialParametersClass.ApplicationNumber, str);
//            switch (errorLogLevel)
//            {
//                case ErrorLogLevel.Error:
//                    ErrorLog.Error(msg);
//                    break;
//                case ErrorLogLevel.Warning:
//                    ErrorLog.Warn(msg);
//                    break;
//                case ErrorLogLevel.Notice:
//                    ErrorLog.Notice(msg);
//                    break;
//            }
//        }

//        public enum ErrorLogLevel
//        {
//            Error, Warning, Notice, None
//        }
//    }
//}