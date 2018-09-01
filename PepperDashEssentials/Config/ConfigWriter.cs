using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials
{
    public class ConfigWriter
    {

        public static bool WriteConfig()
        {
            bool success = false;

            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Writing Configuration to file");

            var fileLock = new CCriticalSection();


            var filePath = Global.FilePathPrefix + "LocalConfig" + Global.DirectorySeparator + "configurationFile.json";

            Debug.Console(0, Debug.ErrorLogLevel.Notice, "Attempting to write config file: '{0}'", filePath);

            var configData = JsonConvert.SerializeObject(ConfigReader.ConfigObject);

            try
            {

                if (fileLock.TryEnter())
                {
                    using (StreamWriter sw = new StreamWriter(filePath))
                    {
                        sw.Write(configData);
                        sw.Flush();
                    }

                    success = true;
                }
                else
                {
                    Debug.Console(0, Debug.ErrorLogLevel.Error, "Unable to enter FileLock");
                    success = false;
                }
            }
            catch (Exception e)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Error, "Error: Config write failed: \r{0}", e);
                success = false;
            }
            finally
            {
                if (fileLock != null && !fileLock.Disposed)
                    fileLock.Leave();
                
            }

            return success;
        }
    }
}