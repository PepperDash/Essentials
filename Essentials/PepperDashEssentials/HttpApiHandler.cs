//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;
//using Crestron.SimplSharp.CrestronIO;
//using Crestron.SimplSharp.Net.Http;

//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//using PepperDash.Essentials.Core;
//using PepperDash.Essentials.Core.Http;
//using PepperDash.Core;

//namespace PepperDash.Essentials
//{
//    public class EssentialsHttpApiHandler
//    {
//        string ConfigPath;
//        string PresetsPathPrefix;
//        EssentialsHttpServer Server;

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="server">HTTP server to attach to</param>
//        /// <param name="configPath">The full path to configuration file</param>
//        /// <param name="presetsListPath">The folder prefix for the presets path, eq "\HTML\presets\"</param>
//        public EssentialsHttpApiHandler(EssentialsHttpServer server, string configPath, string presetsPathPrefix)
//        {
//            if (server == null) throw new ArgumentNullException("server");
//            Server = server;
//            ConfigPath = configPath;
//            PresetsPathPrefix = presetsPathPrefix;
//            server.ApiRequest += Server_ApiRequest;
//        }


//        void Server_ApiRequest(object sender, Crestron.SimplSharp.Net.Http.OnHttpRequestArgs args)
//        {
//            try
//            {
//                var path = args.Request.Path.ToLower();

//                if (path == "/api/config")
//                    HandleApiConfig(args);
//                else if (path.StartsWith("/api/presetslist/"))
//                    HandleApiPresetsList(args);
//                else if (path == "/api/presetslists")
//                    HandleApiGetPresetsLists(args.Request, args.Response);
//                else
//                {
//                    args.Response.Code = 404;
//                    return;
//                }
//                args.Response.Header.SetHeaderValue("Access-Control-Allow-Origin", "*");
//                args.Response.Header.SetHeaderValue("Access-Control-Allow-Methods", "GET, POST, PATCH, PUT, DELETE, OPTIONS");
//            }
//            catch (Exception e)
//            {
//                Debug.Console(1, "Uncaught HTTP server error: \n{0}", e);
//                args.Response.Code = 500;
//            }
//        }

//        /// <summary>
//        /// GET will return the running configuration.  POST will attempt to take in a new config
//        /// and restart the program.
//        /// </summary>
//        void HandleApiConfig(OnHttpRequestArgs args)
//        {
//            var request = args.Request;
//            if (request.Header.RequestType == "GET")
//            {
//                if (File.Exists(ConfigPath))
//                {
//                    Debug.Console(2, "Sending config:{0}", ConfigPath);
//                    args.Response.Header.ContentType = EssentialsHttpServer.GetContentType(new FileInfo(ConfigPath).Extension);
//                    args.Response.ContentStream = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read);
//                }
//            }
//            else if (request.Header.RequestType == "POST")
//            {
//                Debug.Console(2, "Post type: '{0}'", request.Header.ContentType);
				
//                // Make sure we're receiving at least good json
//                Debug.Console(1, "Receving new config");
//                if (GetContentStringJson(args) == null)
//                    return;

//                //---------------------------- try to move these into common method
//                // Move current file aside
//                var bakPath = ConfigPath + ".bak";
//                if (File.Exists(bakPath))
//                    File.Delete(bakPath);
//                File.Move(ConfigPath, bakPath);

//                // Write the file
//                using (FileStream fs = File.Open(ConfigPath, FileMode.OpenOrCreate))
//                using (StreamWriter sw = new StreamWriter(fs))
//                {
//                    try
//                    {
//                        sw.Write(args.Request.ContentString);
//                    }
//                    catch (Exception e)
//                    {
//                        string err = string.Format("Error writing received config file:\r{0}", e);
//                        CrestronConsole.PrintLine(err);
//                        ErrorLog.Warn(err);
//                        // Put file back
//                        File.Move(ConfigPath + ".bak", ConfigPath);
//                        args.Response.Code = 500;
//                        return;
//                    }
//                }

//                // If client says "yeah, restart" and has a good token
//                // Restart program
//                string consoleResponse = null;
//                var restart = CrestronConsole.SendControlSystemCommand("progreset -p:" +
//                    InitialParametersClass.ApplicationNumber, ref consoleResponse);
//                if (!restart) Debug.Console(0, "CAN'T DO THAT YO: {0}", consoleResponse);
//            }
//        }

//        void HandleApiPresetsList(OnHttpRequestArgs args)
//        {
//            var listPath = PresetsPathPrefix + args.Request.Path.Remove(0, 17);
//            Debug.Console(2, "Checking for preset list '{0}'", listPath);

//            if (args.Request.Header.RequestType == "GET")
//            {
//                if (File.Exists(listPath))
//                {
//                    Debug.Console(2, "Sending presets file:{0}", listPath);
//                    args.Response.Header.ContentType = EssentialsHttpServer.GetContentType(new FileInfo(listPath).Extension);
//                    args.Response.ContentStream = new FileStream(listPath, FileMode.Open, FileAccess.Read);
//                }
//            }
//            else if (args.Request.Header.RequestType == "POST")
//            {
//                // Make sure we're receiving at least good json
//                Debug.Console(1, "Receving new presets");
//                if (GetContentStringJson(args) == null)
//                    return;

//                //---------------------------- try to move these into common method
//                // Move current file aside
//                var bakPath = listPath + ".new";
//                Debug.Console(2, "Moving presets file to {0}", bakPath);
//                if(File.Exists(bakPath))
//                    File.Delete(bakPath);
//                File.Move(listPath, bakPath);

//                Debug.Console(2, "Writing new file");
//                // Write the file
//                using (FileStream fs = File.OpenWrite(listPath))
//                using (StreamWriter sw = new StreamWriter(fs))
//                {
//                    try
//                    {
//                        Debug.Console(2, "Writing {1}, {0} bytes", args.Request.ContentString.Length, listPath);
//                        sw.Write(args.Request.ContentString);
//                    }
//                    catch (Exception e)
//                    {
//                        string err = string.Format("Error writing received presets file:\r{0}", e);
//                        CrestronConsole.PrintLine(err);
//                        ErrorLog.Warn(err);
//                        // Put file back
//                        File.Move(listPath + ".bak", listPath);
//                        args.Response.Code = 500;
//                        return;
//                    }
//                }
//            }
//        }


//        void HandleApiGetPresetsLists(HttpServerRequest request, HttpServerResponse response)
//        {
//            if (request.Header.RequestType != "GET")
//            {
//                response.Code = 404; // This should be a 405 with an allow header
//                return;
//            }

//            if (Directory.Exists(PresetsPathPrefix))
//            {
//                //CrestronConsole.PrintLine("Parsing presets directory");
//                List<string> files = Directory.GetFiles(PresetsPathPrefix, "*.json")
//                    .ToList().Select(f => Path.GetFileName(f)).ToList();
//                if (files.Count > 0)
//                    files.Sort();
//                var json = JsonConvert.SerializeObject(files);
//                response.Header.ContentType = "application/json";
//                response.ContentString = json;
//            }

//                        //    //CrestronConsole.PrintLine("Found {0} files", files.Count);
//                        //    JObject jo = new JObject();
//                        //    JArray ja = new JArray();
						
//                        //    foreach (var filename in files)
//                        //    {
//                        //        try
//                        //        {
//                        //            using (StreamReader sr = new StreamReader(filename))
//                        //            {
//                        //                JObject tempJo = JObject.Parse(sr.ReadToEnd());
//                        //                if (tempJo.Value<string>("content").Equals("presetsList"))
//                        //                {
//                        //                    var jItem = new JObject(); // make a new object
//                        //                    jItem.Add("Name", tempJo["name"]);
//                        //                    jItem.Add("File", filename);
//                        //                    jItem.Add("Url", Uri.EscapeUriString(new Uri(
//                        //                        filename.Replace("\\html", "")
//                        //                        .Replace("\\HTML", "")
//                        //                        .Replace('\\', '/'), UriKind.Relative).ToString()));
//                        //                    ja.Add(jItem); // add to array
//                        //                }
//                        //                else
//                        //                    CrestronConsole.PrintLine("Cannot use presets file '{0}'", filename);
//                        //            }
//                        //        }
//                        //        catch
//                        //        {
//                        //            // ignore failures - maybe delete them
//                        //            CrestronConsole.PrintLine("Unable to read presets file '{0}'", filename);
//                        //        }
//                        //    }
//                        //    jo.Add("PresetChannelLists", ja);
//                        //    //CrestronConsole.PrintLine(jo.ToString());
//                        //    response.Header.ContentType = "application/json";
//                        //    response.ContentString = jo.ToString();
//                        //}
//                        //else
//                        //    CrestronConsole.PrintLine("No presets files in directory");
//        }

//        /// <summary>
//        /// Simply does what it says
//        /// </summary>
//        JObject GetContentStringJson(OnHttpRequestArgs args)
//        {
//            //var content = args.Request.ContentString;
//            //Debug.Console(1, "{0}", content);

//            try
//            {
//                // just see if it parses properly
//                return JObject.Parse(args.Request.ContentString);
//            }
//            catch (Exception e)
//            {
//                string err = string.Format("JSON Error reading config file:\r{0}", e);
//                CrestronConsole.PrintLine(err);
//                ErrorLog.Warn(err);
//                args.Response.Code = 400; // Bad request
//                return null;
//            }
//        }
//    }
//}